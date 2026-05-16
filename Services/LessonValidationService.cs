using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonPolicyViolations;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Constants;
using Domain.Dto;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;

namespace Services;

public class LessonValidationService(
    ILessonRepository lessonRepository,
    ILessonPolicyViolationRepository lessonPolicyViolationRepository,
    IScheduleRepository scheduleRepository,
    ITeacherRepository teacherRepository,
    IAcademicDisciplineRepository academicDisciplineRepository,
    IRoomRepository roomRepository,
    IStudentGroupRepository studentGroupRepository,
    ITeacherPreferenceRepository teacherPreferenceRepository) : ILessonValidationService
{
    public async Task SaveAllAsync(LessonPolicyViolation[] violations)
    {
        await lessonPolicyViolationRepository.SaveAllAsync(violations);
    }

    public async Task<LessonPolicyViolation[]> ValidateAsync(Lesson[] lessons)
    {
        var studentGroupIds = lessons.SelectMany(lesson => lesson.StudentGroups.Select(studentGroup => studentGroup.Id!.Value)).Distinct().ToArray();
        var studentGroups = await studentGroupRepository.SelectAsync(studentGroupIds);
        var studentGroupsById = studentGroups.ToDictionary(x => x.Id!.Value);

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(studentGroupIds);

        var teacherIds = lessons.SelectMany(lesson => lesson.Teachers.Select(x => x.Id!.Value)).Distinct().ToArray();
        var teachers = await teacherRepository.SelectAsync(teacherIds);
        var teachersById = teachers.ToDictionary(x => x.Id!.Value);

        var roomIds = lessons.SelectMany(lesson => lesson.Rooms.Select(x => x.Id!.Value)).Distinct().ToArray();
        var rooms = await roomRepository.SelectAsync(roomIds);
        var roomsById = rooms.ToDictionary(x => x.Id!.Value);

        var academicDisciplineIds = lessons.Where(lesson => lesson.AcademicDisciplineId.HasValue).Select(lesson => lesson.AcademicDisciplineId!.Value).Distinct().ToArray();
        var academicDisciplines = await academicDisciplineRepository.SelectAsync(academicDisciplineIds);
        var academicDisciplinesById = academicDisciplines.ToDictionary(x => x.Id!.Value);

        var previousLessonIds = lessons.Where(lesson => lesson.Id.HasValue).Select(lesson => lesson.Id!.Value).Distinct().ToArray();
        var previousLessons = await lessonRepository.SelectAsync(previousLessonIds);
        var previousLessonsById = previousLessons.ToDictionary(x => x.Id!.Value);

        var validationMessages = new List<ValidationMessage>();
        foreach (var lesson in lessons)
        {
            if (!await scheduleRepository.ExistsAsync(lesson.ScheduleId))
            {
                validationMessages.Add(new ValidationMessage("Не найден проект расписания для сохранения занятия"));
            }

            if (lesson.StudentGroups.Any(sg => !studentGroupsById.ContainsKey(sg.Id!.Value)))
            {
                validationMessages.Add(new ValidationMessage("Не найдены академические группы для сохранения занятия"));
            }

            if (lesson.Teachers.Any(t => !teachersById.ContainsKey(t.Id!.Value)))
            {
                validationMessages.Add(new ValidationMessage("Не найдены преподаватели для сохранения занятия"));
            }

            if (lesson.Rooms.Any(r => !roomsById.ContainsKey(r.Id!.Value)))
            {
                validationMessages.Add(new ValidationMessage("Не найдены аудитории для сохранения занятия"));
            }

            if (lesson.Id.HasValue && previousLessonsById[lesson.Id!.Value].ScheduleId != lesson.ScheduleId)
            {
                validationMessages.Add(new ValidationMessage("Запрещено менять проект расписания для занятия"));
            }
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var affectedByEditingLessonsPolicyViolations = await lessonPolicyViolationRepository.SearchAsync(
            new LessonPolicyViolationSearchModel
            {
                AffectedByLessonIds = previousLessonsById.Select(lesson => lesson.Key).ToArray(),
            });

        await lessonPolicyViolationRepository.DeleteAsync(previousLessonsById
            .SelectMany(lesson => lesson.Value.Violations
                .Select(violation => violation.Id!.Value))
            .Concat(affectedByEditingLessonsPolicyViolations
                .Select(x => x.Id!.Value))
            .ToArray());

        var conflictingLessons = await lessonRepository.SearchConflictsAsync(new LessonConflictsSearchModel
        {
            ScheduleId = lessons.First().ScheduleId,
            StudentGroupIds = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray(),
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DateWithTimeIntervals = lessons
                .Where(lesson => lesson.DateWithTimeInterval != null)
                .Select(lesson => lesson.DateWithTimeInterval!)
                .ToArray(),
        });

        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchConflictsAsync(new TeacherPreferenceConflictsSearchModel
        {
            ScheduleId = lessons.First().ScheduleId,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DayOfWeekTimeIntervals = lessons
                .Where(lesson => lesson.DateWithTimeInterval != null)
                .Select(lesson => lesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval())
                .ToArray(),
            TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
        });

        var totalLessonPolicyViolations = new List<LessonPolicyViolation>();
        foreach (var lesson in lessons)
        {
            var lessonPolicyViolations = new List<LessonPolicyViolation>();

            if (lesson.AcademicDisciplineId.HasValue)
            {
                ValidateAcademicDisciplineStudentGroupMatch(lesson,
                    lessonPolicyViolations,
                    academicDisciplinesById[lesson.AcademicDisciplineId!.Value],
                    studentGroups);
                ValidateAcademicDisciplineTypeMatch(lesson,
                    lessonPolicyViolations,
                    academicDisciplinesById[lesson.AcademicDisciplineId!.Value],
                    lesson.AcademicDisciplineType!.Value);
            }

            if (lesson.DateWithTimeInterval == null)
            {
                totalLessonPolicyViolations.AddRange(lessonPolicyViolations);
                continue;
            }

            var hierarchyIdsFlat = studentGroupHierarchyIdsByStudentGroupId
                .Where(kv => lesson.StudentGroups.Any(sg => sg.Id == kv.Key))
                .SelectMany(x => x.Value)
                .ToArray();

            var currentLessonConflictingLessons = FilterCurrentConflictingLessons(lesson, conflictingLessons,
                hierarchyIdsFlat);

            var currentLessonConflictingTeacherPreferences = Array.Empty<TeacherPreference>();
            if (lesson.Teachers.Length > 0)
            {
                currentLessonConflictingTeacherPreferences = FilterCurrentConflictingTeacherPreferences(lesson,
                    conflictingTeacherPreferences);
            }

            BuildPolicyViolations(lessonPolicyViolations,
                studentGroupHierarchyIdsByStudentGroupId,
                currentLessonConflictingLessons,
                lesson,
                teacherIds,
                roomIds,
                currentLessonConflictingTeacherPreferences);

            totalLessonPolicyViolations.AddRange(lessonPolicyViolations);
        }

        return totalLessonPolicyViolations.ToArray();

        Lesson[] FilterCurrentConflictingLessons(Lesson lesson, Lesson[] conflicting, Guid[] hierarchyIdsFlat) => conflicting
            .Where(conflictingLesson =>
                conflictingLesson.Id != lesson.Id
                && conflictingLesson.DateWithTimeInterval!.HasIntersection(lesson.DateWithTimeInterval!)
                && (conflictingLesson.StudentGroups.Any(studentGroup =>
                        hierarchyIdsFlat.Contains(studentGroup.Id!.Value))
                    || (lesson.Teachers.Length != 0 && conflictingLesson.Teachers
                        .Any(conflictingLessonTeacher => lesson.Teachers
                            .Any(lessonTeacher => lessonTeacher.Id == conflictingLessonTeacher.Id)))
                    || (lesson.Rooms.Length != 0 && conflictingLesson.Rooms
                        .Any(conflictingLessonRoom => lesson.Rooms
                            .Any(lessonRoom => lessonRoom.Id == conflictingLessonRoom.Id))))
                && (!lesson.AllowCombining || !conflictingLesson.AllowCombining))
            .ToArray();

        TeacherPreference[] FilterCurrentConflictingTeacherPreferences(Lesson lesson, TeacherPreference[] conflicting) => conflicting
            .Where(conflictingTeacherPreference =>
                lesson.Teachers.Any(teacher => teacher.Id!.Value == conflictingTeacherPreference.TeacherId)
                && (!conflictingTeacherPreference.RoomId.HasValue
                    || lesson.Rooms.Length == 0
                    || lesson.Rooms.Any(room => room.Id!.Value == conflictingTeacherPreference.RoomId!.Value))
                && (conflictingTeacherPreference.DayOfWeekTimeInterval == null
                    || (conflictingTeacherPreference.DayOfWeekTimeInterval!.HasIntersection(lesson.DateWithTimeInterval!))))
            .ToArray();
    }

    public void BuildPolicyViolations(List<LessonPolicyViolation> lessonPolicyViolations,
        Dictionary<Guid,List<Guid>> studentGroupHierarchyIdsByStudentGroupId,
        Lesson[] conflictingLessons,
        Lesson lesson,
        Guid[] teacherIds,
        Guid[] roomIds,
        TeacherPreference[] conflictingTeacherPreferences,
        bool includeTiming = false)
    {
        foreach (var hierarchyIds in studentGroupHierarchyIdsByStudentGroupId.Values)
        {
            var conflictingByGroupHierarchyLessons = conflictingLessons
                .Where(x => x.StudentGroups.Any(y => hierarchyIds.Contains(y.Id!.Value)))
                .ToArray();
            ValidateLessonConflictByGroup(lesson, conflictingByGroupHierarchyLessons, lessonPolicyViolations, hierarchyIds.ToArray(), includeTiming);
        }

        var conflictingByTeacherLessons = conflictingLessons
            .Where(x => x.Teachers.Any(y => teacherIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateLessonConflictByTeacher(lesson, teacherIds, conflictingByTeacherLessons,
            lessonPolicyViolations, includeTiming);

        ValidateTeacherPreferenceConflict(lesson, conflictingTeacherPreferences, lessonPolicyViolations, includeTiming);

        var conflictingByRoomLessons = conflictingLessons
            .Where(x => x.Rooms.Any(y => roomIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateLessonConflictByRoom(lesson, roomIds, conflictingByRoomLessons, lessonPolicyViolations, includeTiming);
    }

    public async Task<LessonSeriesConflictDto[]> FillValidationMessages(Lesson[] lessons)
    {
        var lessonConflicts = new List<LessonSeriesConflictDto>();

        var affectedByLessonBatchInfoIds = lessons
            .SelectMany(lesson => lesson.Violations
                .Where(violation => violation.Payload.AffectedByLessonBatchInfoId.HasValue)
                .Select(violation => violation.Payload.AffectedByLessonBatchInfoId!.Value))
            .ToArray();

        var currentBatchLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            LessonBatchInfoIds = affectedByLessonBatchInfoIds,
        });

        var currentBatchLessonsTotalHoursByLessonId = lessons
            .ToDictionary(lesson => lesson.Id!.Value, lesson => currentBatchLessons
                .Where(currentBatchLesson => currentBatchLesson.LessonBatchInfoId == lesson.LessonBatchInfoId)
                .Sum(currentBatchLesson => currentBatchLesson.HoursCost ?? 0));

        var messages = await GetValidationResultMessageAsync(
            lessons.SelectMany(lesson => lesson.Violations).ToArray(),
            currentBatchLessonsTotalHoursByLessonId);
        var messagesByLessonId = messages.ToDictionary(message => message.LessonId, message => message.MessagesByViolationId);
        lessonConflicts.AddRange(lessons.SelectMany(lesson => lesson.Violations
            .Select(violation => new LessonSeriesConflictDto
            {
                LessonIds = [lesson.Id!.Value],
                DayOfWeekTimeInterval = lesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval(),
                Messages = [new LessonSeriesConflictMessageDto
                {
                    TimeInterval = lesson.DateWithTimeInterval!.TimeInterval,
                    Message = messagesByLessonId[lesson.Id!.Value][violation.Id!.Value],
                    ErrorType = violation.ErrorType,
                }],
                MaxErrorType = violation.ErrorType,
            })));

        return lessonConflicts.ToArray();
    }

    public async Task<LessonValidationMessageBatchDto[]> GetValidationResultMessageAsync(LessonPolicyViolation[] violations,
        Dictionary<Guid, int>? currentBatchLessonsTotalHoursByLessonId = null)
    {
        var disciplineIds = violations.Where(x => x.Payload.AffectedByAcademicDisciplineId.HasValue)
            .Select(x => x.Payload.AffectedByAcademicDisciplineId!.Value)
            .Distinct()
            .ToArray();
        var disciplinesById = (await academicDisciplineRepository.SelectAsync(disciplineIds)).ToDictionary(x => x.Id!.Value);
        var studentGroupsIds = violations.Where(x => x.Payload.AffectedByStudentGroupId.HasValue)
            .Select(x => x.Payload.AffectedByStudentGroupId!.Value)
            .Distinct()
            .ToArray();
        var studentGroupsById = (await studentGroupRepository.SelectAsync(studentGroupsIds)).ToDictionary(x => x.Id!.Value);
        var lessonIds = violations.Where(x => x.Payload.AffectedByLessonId.HasValue)
            .Select(x => x.Payload.AffectedByLessonId!.Value)
            .Distinct()
            .ToArray();
        var lessonsById = (await lessonRepository.SelectAsync(lessonIds)).ToDictionary(x => x.Id!.Value);
        var teacherIds = violations.Where(x => x.Payload.AffectedByTeacherId.HasValue)
            .Select(x => x.Payload.AffectedByTeacherId!.Value)
            .Distinct()
            .ToArray();
        var teachersById = (await teacherRepository.SelectAsync(teacherIds)).ToDictionary(x => x.Id!.Value);
        var roomIds = violations.Where(x => x.Payload.AffectedByRoomId.HasValue)
            .Select(x => x.Payload.AffectedByRoomId!.Value)
            .Distinct()
            .ToArray();
        var roomsById = (await roomRepository.SelectAsync(roomIds)).ToDictionary(x => x.Id!.Value);

        var result = new Dictionary<Guid, Dictionary<Guid, string>>();
        foreach (var violation in violations)
        {
            var discipline = violation.Payload.AffectedByAcademicDisciplineId.HasValue
                ? disciplinesById[violation.Payload.AffectedByAcademicDisciplineId!.Value]
                : null;
            var studentGroup = violation.Payload.AffectedByStudentGroupId.HasValue
                ? studentGroupsById[violation.Payload.AffectedByStudentGroupId!.Value]
                : null;
            var affectedByLesson = violation.Payload.AffectedByLessonId.HasValue
                ? lessonsById[violation.Payload.AffectedByLessonId!.Value]
                : null;
            var teacher = violation.Payload.AffectedByTeacherId.HasValue
                ? teachersById[violation.Payload.AffectedByTeacherId!.Value]
                : null;
            var room = violation.Payload.AffectedByRoomId.HasValue
                ? roomsById[violation.Payload.AffectedByRoomId!.Value]
                : null;
            if (!result.TryGetValue(violation.LessonId, out var lessonMessages))
            {
                lessonMessages = [];
                result[violation.LessonId] = lessonMessages;
            }

            lessonMessages[violation.Id!.Value] = violation.Code switch
            {
                LessonPolicyViolationCode.MismatchedSemesterNumber => string.Format(
                    LessonPolicyViolationTemplates.MismatchedSemesterNumberTemplate,
                    studentGroup!.Name,
                    studentGroup.SemesterNumber,
                    discipline!.Name,
                    discipline.SemesterNumber),
                LessonPolicyViolationCode.MismatchedAcademicDisciplineType => string.Format(
                    LessonPolicyViolationTemplates.MismatchedAcademicDisciplineTypeTemplate,
                    affectedByLesson!.AcademicDisciplineType!.GetDescription(),
                    discipline!.Name),
                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByGroupTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : $"\"{affectedByLesson.AcademicDiscipline.Name} ({affectedByLesson.AcademicDisciplineType!.GetDescription()})\"",
                    studentGroup!.Name,
                    studentGroup.ChildrenFlat.Any(x => x.Id == violation.LessonId)
                        ? "которой принадлежит отмеченная группы"
                        : "которая принадлежит отмеченной группе"),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByGroupTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : $"\"{affectedByLesson.AcademicDiscipline.Name} ({affectedByLesson.AcademicDisciplineType!.GetDescription()})\"",
                    studentGroup!.Name,
                    studentGroup.ChildrenFlat.Any(x => x.Id == violation.LessonId)
                        ? "которой принадлежит отмеченная группы"
                        : "которая принадлежит отмеченной группе"),
                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByTeacherTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : $"\"{affectedByLesson.AcademicDiscipline.Name} ({affectedByLesson.AcademicDisciplineType!.GetDescription()})\"",
                    teacher!.Fullname),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByTeacherTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : $"\"{affectedByLesson.AcademicDiscipline.Name} ({affectedByLesson.AcademicDisciplineType!.GetDescription()})\"",
                    teacher!.Fullname),
                LessonPolicyViolationCode.FixedLessonTypeConflictByRoom => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByRoomTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : $"\"{affectedByLesson.AcademicDiscipline.Name} ({affectedByLesson.AcademicDisciplineType!.GetDescription()})\"",
                    room!.Name),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByRoomTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : $"\"{affectedByLesson.AcademicDiscipline.Name} ({affectedByLesson.AcademicDisciplineType!.GetDescription()})\"",
                    room!.Name),
                LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict => string.Format(
                    LessonPolicyViolationTemplates.RestrictedTimeTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict => string.Format(
                    LessonPolicyViolationTemplates.UndesirableTimeTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict => string.Format(
                    LessonPolicyViolationTemplates.RestrictedRoomTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict => string.Format(
                    LessonPolicyViolationTemplates.UndesirableRoomTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonPolicyViolationCode.MismatchedAcademicDisciplineTypeTotalHoursCount => string.Format(
                    LessonPolicyViolationTemplates.MismatchedAcademicDisciplineTypeTotalHoursCountTemplate,
                    discipline!.Name,
                    violation.Payload.AffectedByAcademicDisciplineType!.Value.GetDescription(),
                    currentBatchLessonsTotalHoursByLessonId![affectedByLesson!.Id!.Value],
                    violation.Payload.AffectedByLessonBatchInfo!.TotalHoursCount,
                    studentGroup!.Name),
                _ => throw new NotSupportedException(),
            };
        }

        return result.Select(kv => new LessonValidationMessageBatchDto { LessonId = kv.Key, MessagesByViolationId = kv.Value }).ToArray();
    }

    public async Task RemovePolicyViolations(Guid[] lessonIds, LessonPolicyViolationCode[] validationCodes)
    {
        var selfViolations = await lessonPolicyViolationRepository.SearchAsync(
            new LessonPolicyViolationSearchModel
            {
                LessonIds = lessonIds,
                ValidationCodes = validationCodes,
            });
        var affectedViolations = await lessonPolicyViolationRepository.SearchAsync(
            new LessonPolicyViolationSearchModel
            {
                AffectedByLessonIds = lessonIds,
                ValidationCodes = validationCodes,
            });
        await lessonPolicyViolationRepository.DeleteAsync(selfViolations
            .Concat(affectedViolations)
            .Select(x => x.Id!.Value)
            .ToArray());
    }

    public async Task RemovePolicyViolations(Guid academicDisciplineId)
    {
        var violations = await lessonPolicyViolationRepository.SearchAsync(
            new LessonPolicyViolationSearchModel
            {
                AffectedByAcademicDisciplineIds = [academicDisciplineId],
            });
        await lessonPolicyViolationRepository.DeleteAsync(violations.Select(x => x.Id!.Value).ToArray());
    }

    public void ValidateAcademicDisciplineStudentGroupMatch(Lesson? lesson,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        StudentGroup[] studentGroups)
    {
        foreach (var studentGroup in studentGroups)
        {
            var payload = new LessonValidationPayload
            {
                AffectedByAcademicDisciplineId = academicDiscipline.Id,
                AffectedByStudentGroupId = studentGroup.Id,
            };
            violations
                .AddErrorIf(academicDiscipline.SemesterNumber != null
                            && studentGroup.SemesterNumber != null
                            && academicDiscipline.SemesterNumber != studentGroup.SemesterNumber,
                    payload, LessonPolicyViolationCode.MismatchedSemesterNumber, lesson?.Id);
        }
    }

    public void ValidateAcademicDisciplineTypeMatch(Lesson? lesson,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        AcademicDisciplineType lessonAcademicDisciplineType)
    {
        var payload = new LessonValidationPayload
        {
            AffectedByAcademicDisciplineId = academicDiscipline.Id!,
            AffectedByLessonId = lesson?.Id,
        };
        violations.AddErrorIf(
            !academicDiscipline.AllowedLessonTypes.Contains(lessonAcademicDisciplineType),
            payload,
            LessonPolicyViolationCode.MismatchedAcademicDisciplineType,
            lesson?.Id);
    }

    public void ValidateLessonConflictByGroup(Lesson lesson,
        Lesson[] conflictingByGroupLessons,
        List<LessonPolicyViolation> violations,
        Guid[] hierarchyIds,
        bool includeTiming = false)
    {
        foreach (var conflictingByGroupLesson in conflictingByGroupLessons)
        {
            foreach (var conflictingGroup in conflictingByGroupLesson.StudentGroups
                         .Where(x => hierarchyIds.Contains(x.Id!.Value)))
            {
                var editedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = conflictingByGroupLesson.Id,
                    AffectedByStudentGroupId = conflictingGroup.Id!.Value,
                    DayOfWeekTimeInterval = includeTiming
                        ? conflictingByGroupLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                };
                violations
                    .AddWarningIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                        lesson.Id);
                violations
                    .AddErrorIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                        lesson.Id);
            }

            foreach (var lessonHierarchyGroup in lesson.StudentGroups
                         .Where(x => hierarchyIds.Contains(x.Id!.Value)))
            {
                var existedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = lesson.Id,
                    AffectedByStudentGroupId = lessonHierarchyGroup.Id!.Value,
                };
                violations
                    .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                        conflictingByGroupLesson.Id!.Value);
                violations
                    .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                        conflictingByGroupLesson.Id!.Value);
            }
        }
    }

    public void ValidateTeacherPreferenceConflict(Lesson lesson,
        TeacherPreference[] conflictingTeacherPreferences,
        List<LessonPolicyViolation> violations,
        bool includeTiming = false)
    {
        foreach (var conflictingTeacherPreference in conflictingTeacherPreferences)
        {
            var payload = new LessonValidationPayload
            {
                AffectedByTeacherPreferenceId = conflictingTeacherPreference.Id,
                AffectedByTeacherId = conflictingTeacherPreference.TeacherId,
                DayOfWeekTimeInterval = includeTiming ? conflictingTeacherPreference.DayOfWeekTimeInterval : null,
            };
            violations
                .AddWarningIf(
                    conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    payload,
                    LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict,
                    lesson.Id);
            violations
                .AddErrorIf(
                    conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict,
                    lesson.Id);
            violations
                .AddWarningIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    payload,
                    LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict,
                    lesson.Id);
            violations
                .AddErrorIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict,
                    lesson.Id);
        }
    }

    private void ValidateLessonConflictByTeacher(Lesson lesson,
        Guid[] teacherIds,
        Lesson[] conflictingByTeacherLessons,
        List<LessonPolicyViolation> violations,
        bool includeTiming = false)
    {
        foreach (var conflictingByTeacherLesson in conflictingByTeacherLessons)
        {
            foreach (var teacher in conflictingByTeacherLesson.Teachers.Where(x => teacherIds.Contains(x.Id!.Value)))
            {
                var editedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = conflictingByTeacherLesson.Id,
                    AffectedByTeacherId = teacher.Id,
                    DayOfWeekTimeInterval = includeTiming
                        ? conflictingByTeacherLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                };
                violations
                    .AddWarningIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                        lesson.Id);
                violations
                    .AddErrorIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                        lesson.Id);

                var existedLessonPayload = new LessonValidationPayload { AffectedByLessonId = lesson.Id };
                violations
                    .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                        conflictingByTeacherLesson.Id!.Value);
                violations
                    .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                        conflictingByTeacherLesson.Id!.Value);
            }
        }
    }

    private void ValidateLessonConflictByRoom(Lesson lesson,
        Guid[] roomIds,
        Lesson[] conflictingByRoomLessons,
        List<LessonPolicyViolation> violations,
        bool includeTiming = false)
    {
        foreach (var conflictingByRoomLesson in conflictingByRoomLessons)
        {
            foreach (var room in conflictingByRoomLesson.Rooms.Where(x => roomIds.Contains(x.Id!.Value)))
            {
                var editedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = conflictingByRoomLesson.Id,
                    AffectedByRoomId = room.Id,
                    DayOfWeekTimeInterval = includeTiming
                        ? conflictingByRoomLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                };
                violations
                    .AddWarningIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                        lesson.Id);
                violations
                    .AddErrorIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                        lesson.Id);

                var existedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = lesson.Id,
                    AffectedByRoomId = room.Id,
                };
                violations
                    .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                        conflictingByRoomLesson.Id!.Value);
                violations
                    .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                        conflictingByRoomLesson.Id!.Value);
            }
        }
    }
}