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
using Domain.Models.Common;
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
    public async Task<LessonPolicyViolation[]> ValidateAsync(Lesson lesson)
    {
        var validationMessages = new List<ValidationMessage>();
        var studentGroupIds = lesson.StudentGroups.Select(x => x.Id!.Value).ToArray();
        var studentGroups = await studentGroupRepository.SelectAsync(studentGroupIds);
        var teacherIds = lesson.Teachers.Select(x => x.Id!.Value).ToArray();
        var teachers = await teacherRepository.SelectAsync(teacherIds);
        var roomIds = lesson.Rooms.Select(x => x.Id!.Value).ToArray();
        var rooms = await roomRepository.SelectAsync(roomIds);
        var academicDiscipline = lesson.AcademicDisciplineId.HasValue
            ? await academicDisciplineRepository.GetAsync(lesson.AcademicDisciplineId!.Value)
            : null;
        var previousLesson = lesson.Id.HasValue
            ? await lessonRepository.GetAsync(lesson.Id!.Value)
            : null;

        if (!await scheduleRepository.ExistsAsync(lesson.ScheduleId))
        {
            validationMessages.Add(new ValidationMessage("Не найден проект расписания для сохранения занятия"));
        }

        if (studentGroups.Length != lesson.StudentGroups.Length)
        {
            validationMessages.Add(new ValidationMessage("Не найдены академические группы для сохранения занятия"));
        }

        if (teachers.Length != lesson.Teachers.Length)
        {
            validationMessages.Add(new ValidationMessage("Не найдены преподаватели для сохранения занятия"));
        }

        if (rooms.Length != lesson.Rooms.Length)
        {
            validationMessages.Add(new ValidationMessage("Не найдены аудитории для сохранения занятия"));
        }

        if (previousLesson != null && previousLesson.ScheduleId != lesson.ScheduleId)
        {
            validationMessages.Add(new ValidationMessage("Запрещено менять проект расписания для занятия"));
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        if (previousLesson != null)
        {
            var affectedByEditingLessonPolicyViolations = await lessonPolicyViolationRepository.SearchAsync(
                new LessonPolicyViolationSearchModel { AffectedByLessonIds = [lesson.Id!.Value] });
            await lessonPolicyViolationRepository.DeleteAsync(previousLesson.Violations
                .Select(x => x.Id!.Value)
                .Concat(affectedByEditingLessonPolicyViolations.Select(x => x.Id!.Value)).ToArray());
        }

        var lessonPolicyViolations = new List<LessonPolicyViolation>();

        if (lesson.AcademicDisciplineId.HasValue)
        {
            ValidateAcademicDisciplineStudentGroupMatch(lesson, lessonPolicyViolations, academicDiscipline!,
                studentGroups);
            ValidateAcademicDisciplineTypeMatch(lesson, lessonPolicyViolations, academicDiscipline!,
                lesson.AcademicDisciplineType!.Value);
        }

        if (lesson.DateWithTimeInterval == null)
        {
            return lessonPolicyViolations.ToArray();
        }

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(studentGroupIds);
        var hierarchyIdsFlat = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray();

        var conflictingLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = lesson.ScheduleId,
            StudentGroupIds = hierarchyIdsFlat,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            Date = lesson.DateWithTimeInterval.Date,
            TimeIntervals = [lesson.DateWithTimeInterval.TimeInterval],
            ExcludeAllowCombining = lesson.AllowCombining,
            ExcludeLessonIds = lesson.Id.HasValue ? [lesson.Id!.Value] : [],
            SearchForConflicts = true,
        });

        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = lesson.ScheduleId,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DaysOfWeek = [lesson.DateWithTimeInterval.Date.DayOfWeek],
            TimeInterval = lesson.DateWithTimeInterval.TimeInterval,
            TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
        });

        BuildPolicyViolations(lessonPolicyViolations, studentGroupHierarchyIdsByStudentGroupId,
            conflictingLessons, lesson, teacherIds, roomIds, conflictingTeacherPreferences);

        var lessonsWithConflictById = conflictingLessons.DistinctBy(x => x.Id).ToDictionary(x => x.Id!.Value);
        var affectedLessonNewViolationsByLessonId = lessonPolicyViolations
            .Where(x => x.LessonId != Guid.Empty && x.LessonId != lesson.Id)
            .GroupBy(x => x.LessonId)
            .ToDictionary(x => x.Key);
        foreach (var (lessonId, affectedLessonPolicyViolations) in affectedLessonNewViolationsByLessonId)
        {
            lessonsWithConflictById[lessonId].Violations = lessonsWithConflictById[lessonId].Violations
                .Concat(affectedLessonPolicyViolations).ToArray();
        }

        return lessonPolicyViolations.ToArray();
    }

    public async Task DeleteViolationLinksAsync(Guid[] ids)
    {
        await lessonPolicyViolationRepository.DeleteViolationLinksAsync(ids);
    }

    public void BuildPolicyViolations(List<LessonPolicyViolation> lessonPolicyViolations,
        Dictionary<Guid,List<Guid>> studentGroupHierarchyIdsByStudentGroupId,
        Lesson[] conflictingLessons,
        Lesson? lesson,
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
        var studentGroupAcademicDisciplineLessonsCache = new Dictionary<(Guid, Guid, Guid), List<Lesson>>();
        foreach (var lesson in lessons)
        {
            if (lesson.AcademicDisciplineId != null)
            {
                var nonCachedStudentGroupIds = lesson.StudentGroups
                    .Where(x => !studentGroupAcademicDisciplineLessonsCache.ContainsKey((lesson.ScheduleId,
                        lesson.AcademicDisciplineId!.Value, x.Id!.Value)))
                    .Select(x => x.Id!.Value)
                    .ToArray();
                if (nonCachedStudentGroupIds.Length > 0)
                {
                    var lessonsToCache = await lessonRepository.SearchAsync(new LessonSearchModel
                    {
                        ScheduleId = lesson.ScheduleId,
                        AcademicDisciplineId = lesson.AcademicDisciplineId!.Value,
                        StudentGroupIds = nonCachedStudentGroupIds,
                    });
                    foreach (var lessonToCache in lessonsToCache)
                    {
                        var lessonNonCachedStudentGroupIds = lessonToCache.StudentGroups
                            .Where(x => nonCachedStudentGroupIds.Contains(x.Id!.Value))
                            .Select(x => x.Id!.Value);
                        foreach (var studentGroupId in lessonNonCachedStudentGroupIds)
                        {
                            if (!studentGroupAcademicDisciplineLessonsCache.ContainsKey((lesson.ScheduleId, lesson.AcademicDisciplineId!.Value, studentGroupId)))
                            {
                                studentGroupAcademicDisciplineLessonsCache[
                                    (lesson.ScheduleId, lesson.AcademicDisciplineId!.Value, studentGroupId)] = [];
                            }

                            studentGroupAcademicDisciplineLessonsCache[
                                (lesson.ScheduleId, lesson.AcademicDisciplineId!.Value, studentGroupId)].Add(lessonToCache);
                        }
                    }
                }
            }

            var messages = await GetValidationResultMessageAsync(lesson.Violations, lesson, studentGroupAcademicDisciplineLessonsCache);
            var index = 0;
            foreach (var violation in lesson.Violations)
            {
                lessonConflicts.Add(new LessonSeriesConflictDto
                {
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval
                    {
                        DayOfWeek = lesson.DateWithTimeInterval!.Date.DayOfWeek,
                        TimeInterval = lesson.DateWithTimeInterval.TimeInterval,
                    },
                    Messages = [new LessonSeriesConflictMessageDto
                    {
                        TimeInterval = lesson.DateWithTimeInterval!.TimeInterval,
                        Message = messages[index++],
                    }],
                    ErrorType = violation.ErrorType,
                });
            }
        }

        return lessonConflicts.ToArray();
    }

    public async Task<string[]> GetValidationResultMessageAsync(LessonPolicyViolation[] violations,
        Lesson? lesson = null, Dictionary<(Guid, Guid, Guid),List<Lesson>>? studentGroupAcademicDisciplineLessonsCache = null)
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

        var result = new List<string>();
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
            result.Add(violation.Code switch
            {
                LessonPolicyViolationCode.MismatchedSemesterNumber => string.Format(
                    LessonPolicyViolationTemplates.MismatchedSemesterNumberTemplate,
                    discipline!.Name,
                    studentGroup!.Name,
                    studentGroup.SemesterNumber,
                    discipline.SemesterNumber),
                LessonPolicyViolationCode.MismatchedAcademicDisciplineType => string.Format(
                    LessonPolicyViolationTemplates.MismatchedAcademicDisciplineTypeTemplate,
                    discipline!.Name,
                    lesson!.AcademicDisciplineType!.GetDescription()),
                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByGroupTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    studentGroup!.Name),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByGroupTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    studentGroup!.Name),
                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByTeacherTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    teacher!.Fullname),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByTeacherTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    teacher!.Fullname),
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
                LessonPolicyViolationCode.FixedLessonTypeConflictByRoom => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByRoomTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    room!.Name),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByRoomTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    room!.Name),
                LessonPolicyViolationCode.MismatchedAcademicDisciplineTypeTotalHoursCount => string.Format(
                    LessonPolicyViolationTemplates.MismatchedAcademicDisciplineTypeTotalHoursCountTemplate,
                    violation.Payload.AffectedByAcademicDisciplineType!.Value.GetDescription(),
                    discipline!.Name,
                    studentGroupAcademicDisciplineLessonsCache![(lesson!.ScheduleId, lesson.AcademicDisciplineId!.Value, studentGroup!.Id!.Value)]
                        .Where(x => x.AcademicDisciplineType == violation.Payload.AffectedByAcademicDisciplineType)
                        .Sum(x => x.HoursCost),
                    discipline.GetPayloadByType(violation.Payload.AffectedByAcademicDisciplineType!.Value)!.TotalHoursCount,
                    studentGroup.Name),
                _ => throw new NotSupportedException(),
            });
        }

        return result.ToArray();
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
        };
        violations.AddErrorIf(
            !academicDiscipline.AllowedLessonTypes.Contains(lessonAcademicDisciplineType),
            payload,
            LessonPolicyViolationCode.MismatchedAcademicDisciplineType,
            lesson?.Id);
    }

    public void ValidateLessonConflictByGroup(Lesson? lesson,
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
                    DateWithTimeInterval = includeTiming ? conflictingByGroupLesson.DateWithTimeInterval : null,
                };
                violations
                    .AddWarningIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                        lesson?.Id);
                violations
                    .AddErrorIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                        lesson?.Id);
            }

            if (lesson == null)
            {
                continue;
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

    public void ValidateLessonConflictByTeacher(Lesson? lesson,
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
                    DateWithTimeInterval = includeTiming ? conflictingByTeacherLesson.DateWithTimeInterval : null,
                };
                violations
                    .AddWarningIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                        lesson?.Id);
                violations
                    .AddErrorIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                        lesson?.Id);

                if (lesson == null)
                {
                    continue;
                }

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

    public void ValidateTeacherPreferenceConflict(Lesson? lesson, TeacherPreference[] conflictingTeacherPreferences,
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
                    lesson?.Id);
            violations
                .AddErrorIf(
                    conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict,
                    lesson?.Id);
            violations
                .AddWarningIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    payload,
                    LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict,
                    lesson?.Id);
            violations
                .AddErrorIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict,
                    lesson?.Id);
        }
    }

    public void ValidateLessonConflictByRoom(Lesson? lesson,
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
                    DateWithTimeInterval = includeTiming ? conflictingByRoomLesson.DateWithTimeInterval : null,
                };
                violations
                    .AddWarningIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                        lesson?.Id);
                violations
                    .AddErrorIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                        lesson?.Id);

                if (lesson == null)
                {
                    continue;
                }

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