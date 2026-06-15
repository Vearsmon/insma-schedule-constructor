using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.LessonBatchInfo;
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
    ILessonBatchInfoRepository lessonBatchInfoRepository,
    ILessonPolicyViolationRepository lessonPolicyViolationRepository,
    ITeacherRepository teacherRepository,
    IAcademicDisciplineRepository academicDisciplineRepository,
    IRoomRepository roomRepository,
    IStudentGroupRepository studentGroupRepository,
    IScheduleRepository scheduleRepository,
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

        var previousLessonIds = lessons.Where(lesson => lesson.Id.HasValue).Select(lesson => lesson.Id!.Value).Distinct().ToArray();
        var previousLessons = await lessonRepository.SelectAsync(previousLessonIds);
        var previousLessonsById = previousLessons.ToDictionary(x => x.Id!.Value);

        var validationMessages = new List<ValidationMessage>();
        foreach (var lesson in lessons)
        {
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
            ScheduleId = lessons.First().LessonBatchInfo.AcademicDiscipline.ScheduleId,
            StudentGroupIds = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray(),
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DateWithTimeIntervals = lessons
                .Where(lesson => lesson.DateWithTimeInterval != null)
                .Select(lesson => lesson.DateWithTimeInterval!)
                .ToArray(),
        });

        var conflictingBatches = await lessonBatchInfoRepository.SearchConflictsAsync(new LessonBatchInfoConflictsSearchModel
        {
            ScheduleId = lessons.First().LessonBatchInfo.AcademicDiscipline.ScheduleId,
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
            ScheduleId = lessons.First().LessonBatchInfo.AcademicDiscipline.ScheduleId,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DayOfWeekTimeIntervals = lessons
                .Where(lesson => lesson.DateWithTimeInterval != null)
                .Select(lesson => lesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval())
                .ToArray(),
            TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
        });

        var schedule = await scheduleRepository.GetAsync(lessons.First().LessonBatchInfo.AcademicDiscipline.ScheduleId);

        var totalLessonPolicyViolations = new List<LessonPolicyViolation>();
        foreach (var lesson in lessons)
        {
            var lessonPolicyViolations = new List<LessonPolicyViolation>();

            ValidateAcademicDisciplineStudentGroupMatch(lesson,
                lessonPolicyViolations,
                lesson.LessonBatchInfo.AcademicDiscipline,
                studentGroups);
            ValidateAcademicDisciplineTypeMatch(lesson,
                lessonPolicyViolations,
                lesson.LessonBatchInfo.AcademicDiscipline,
                lesson.LessonBatchInfo.Type);

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

            var isEvenWeek = lesson.DateWithTimeInterval.Date.IntersectsEvenWeek(schedule.DateInterval);

            var currentLessonConflictingLessonBatches = FilterCurrentConflictingLessonBatches(lesson,
                conflictingBatches, hierarchyIdsFlat, isEvenWeek);

            var currentLessonConflictingTeacherPreferences = Array.Empty<TeacherPreference>();
            if (lesson.Teachers.Length > 0)
            {
                currentLessonConflictingTeacherPreferences = FilterCurrentConflictingTeacherPreferences(lesson,
                    conflictingTeacherPreferences);
            }

            BuildPolicyViolations(lessonPolicyViolations,
                studentGroupHierarchyIdsByStudentGroupId,
                currentLessonConflictingLessons,
                currentLessonConflictingLessonBatches,
                lesson,
                teacherIds,
                roomIds,
                currentLessonConflictingTeacherPreferences,
                schedule);

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

        LessonBatchInfo[] FilterCurrentConflictingLessonBatches(Lesson lesson, LessonBatchInfo[] conflicting, Guid[] hierarchyIdsFlat, bool isEvenWeek) => conflicting
            .Where(conflictingBatch =>
                (isEvenWeek
                    ? conflictingBatch.RepeatType != DisciplineLessonRepeatType.OddWeeks
                    : conflictingBatch.RepeatType != DisciplineLessonRepeatType.EvenWeeks)
                && conflictingBatch.DayOfWeekTimeIntervals
                    .Any(x => x.Id != lesson.DayOfWeekTimeIntervalAssignmentId
                              && x.DayOfWeekTimeInterval.HasIntersection(lesson.DateWithTimeInterval!))
                && (conflictingBatch.StudentGroups.Any(studentGroup =>
                        hierarchyIdsFlat.Contains(studentGroup.Id!.Value))
                    || (lesson.Teachers.Length != 0 && conflictingBatch.Teachers
                        .Any(conflictingLessonTeacher => lesson.Teachers
                            .Any(lessonTeacher => lessonTeacher.Id == conflictingLessonTeacher.Id)))
                    || (lesson.Rooms.Length != 0 && conflictingBatch.Rooms
                        .Any(conflictingLessonRoom => lesson.Rooms
                            .Any(lessonRoom => lessonRoom.Id == conflictingLessonRoom.Id))))
                && (!lesson.AllowCombining || !conflictingBatch.AllowCombining))
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
        LessonBatchInfo[] conflictingBatches,
        Lesson lesson,
        Guid[] teacherIds,
        Guid[] roomIds,
        TeacherPreference[] conflictingTeacherPreferences,
        Schedule schedule,
        bool includeTiming = false)
    {
        foreach (var hierarchyIds in studentGroupHierarchyIdsByStudentGroupId.Values)
        {
            var conflictingByGroupHierarchyLessons = conflictingLessons
                .Where(x => x.StudentGroups.Any(y => hierarchyIds.Contains(y.Id!.Value)))
                .ToArray();
            var conflictingByGroupHierarchyBatches = conflictingBatches
                .Where(x => x.StudentGroups.Any(y => hierarchyIds.Contains(y.Id!.Value)))
                .ToArray();
            ValidateConflictByGroup(lesson, conflictingByGroupHierarchyLessons, conflictingByGroupHierarchyBatches, lessonPolicyViolations, hierarchyIds.ToArray(), schedule, includeTiming);
        }

        var conflictingByTeacherLessons = conflictingLessons
            .Where(x => x.Teachers.Any(y => teacherIds.Contains(y.Id!.Value)))
            .ToArray();
        var conflictingByTeacherBatches = conflictingBatches
            .Where(x => x.Teachers.Any(y => teacherIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateConflictByTeacher(lesson, teacherIds, conflictingByTeacherLessons, conflictingByTeacherBatches,
            lessonPolicyViolations, schedule, includeTiming);

        ValidateTeacherPreferenceConflict(lesson, conflictingTeacherPreferences, lessonPolicyViolations, includeTiming);

        var conflictingByRoomLessons = conflictingLessons
            .Where(x => x.Rooms.Any(y => roomIds.Contains(y.Id!.Value)))
            .ToArray();
        var conflictingByRoomBatches = conflictingBatches
            .Where(x => x.Rooms.Any(y => roomIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateConflictByRoom(lesson, roomIds, conflictingByRoomLessons, conflictingByRoomBatches, lessonPolicyViolations, schedule, includeTiming);
    }

    public async Task<LessonSeriesConflictDto[]> FillValidationMessages(Lesson[] lessons)
    {
        var lessonConflicts = new List<LessonSeriesConflictDto>();

        var messages = await GetValidationResultMessageAsync(
            lessons.SelectMany(lesson => lesson.Violations).ToArray());
        var messagesByLessonId = messages.ToDictionary(message => message.LessonId!.Value, message => message.MessagesByViolationId);
        lessonConflicts.AddRange(lessons.SelectMany(lesson => lesson.Violations
            .Select(violation => new LessonSeriesConflictDto
            {
                LessonIds = [lesson.Id!.Value],
                DayOfWeekTimeInterval = lesson.DateWithTimeInterval?.ToDayOfWeekTimeInterval(),
                Messages = [new LessonSeriesConflictMessageDto
                {
                    TimeInterval = lesson.DateWithTimeInterval?.TimeInterval,
                    Message = messagesByLessonId[lesson.Id!.Value][violation.Id!.Value],
                    ErrorType = violation.ErrorType,
                }],
                MaxErrorType = violation.ErrorType,
            })));

        return lessonConflicts.ToArray();
    }

    public async Task<LessonValidationMessageBatchDto[]> GetValidationResultMessageAsync(LessonPolicyViolation[] violations)
    {
        var sourceLessonIds = violations.Where(x => x.LessonId.HasValue).Select(x => x.LessonId!.Value).ToArray();
        var sourceLessonsById = (await lessonRepository.SelectAsync(sourceLessonIds)).ToDictionary(x => x.Id!.Value);
        var disciplineIds = violations
            .SelectMany(x => x.Targets)
            .Where(x => x.TargetType == LessonPolicyViolationTargetType.AcademicDiscipline)
            .Select(x => x.TargetId)
            .Distinct()
            .ToArray();
        var disciplinesById = (await academicDisciplineRepository.SelectAsync(disciplineIds)).ToDictionary(x => x.Id!.Value);
        var studentGroupsIds = violations
            .SelectMany(x => x.Targets)
            .Where(x => x.TargetType == LessonPolicyViolationTargetType.StudentGroup)
            .Select(x => x.TargetId)
            .Distinct()
            .ToArray();
        var studentGroupsById = (await studentGroupRepository.SelectAsync(studentGroupsIds)).ToDictionary(x => x.Id!.Value);
        var lessonIds = violations
            .SelectMany(x => x.Targets)
            .Where(x => x.TargetType == LessonPolicyViolationTargetType.Lesson)
            .Select(x => x.TargetId)
            .Distinct()
            .ToArray();
        var lessonsById = (await lessonRepository.SelectAsync(lessonIds)).ToDictionary(x => x.Id!.Value);
        var lessonBatchIds = violations
            .SelectMany(x => x.Targets)
            .Where(x => x.TargetType == LessonPolicyViolationTargetType.LessonBatch)
            .Select(x => x.TargetId)
            .Distinct()
            .ToArray();
        var lessonBatchesById = (await lessonBatchInfoRepository.SelectAsync(lessonBatchIds)).ToDictionary(x => x.Id!.Value);
        var teacherIds = violations
            .SelectMany(x => x.Targets)
            .Where(x => x.TargetType == LessonPolicyViolationTargetType.Teacher)
            .Select(x => x.TargetId)
            .Distinct()
            .ToArray();
        var teachersById = (await teacherRepository.SelectAsync(teacherIds)).ToDictionary(x => x.Id!.Value);
        var roomIds = violations
            .SelectMany(x => x.Targets)
            .Where(x => x.TargetType == LessonPolicyViolationTargetType.Room)
            .Select(x => x.TargetId)
            .Distinct()
            .ToArray();
        var roomsById = (await roomRepository.SelectAsync(roomIds)).ToDictionary(x => x.Id!.Value);

        var result = new Dictionary<Guid, Dictionary<Guid, string>>();
        foreach (var violation in violations.Where(x => x.LessonId.HasValue))
        {
            var disciplineViolationTarget = violation.Targets.FirstOrDefault(x => x.TargetType == LessonPolicyViolationTargetType.AcademicDiscipline);
            var discipline = disciplineViolationTarget != null
                ? disciplinesById[disciplineViolationTarget.TargetId]
                : null;
            var studentGroupViolationTarget = violation.Targets.FirstOrDefault(x => x.TargetType == LessonPolicyViolationTargetType.StudentGroup);
            var studentGroup = studentGroupViolationTarget != null
                ? studentGroupsById[studentGroupViolationTarget.TargetId]
                : null;
            var lessonViolationTarget = violation.Targets.FirstOrDefault(x => x.TargetType == LessonPolicyViolationTargetType.Lesson);
            var affectedByLesson = lessonViolationTarget != null
                ? lessonsById[lessonViolationTarget.TargetId]
                : null;
            var lessonBatchViolationTarget = violation.Targets.FirstOrDefault(x => x.TargetType == LessonPolicyViolationTargetType.LessonBatch);
            var affectedByLessonBatch = lessonBatchViolationTarget != null
                ? lessonBatchesById[lessonBatchViolationTarget.TargetId]
                : null;
            var teacherViolationTarget = violation.Targets.FirstOrDefault(x => x.TargetType == LessonPolicyViolationTargetType.Teacher);
            var teacher = teacherViolationTarget != null
                ? teachersById[teacherViolationTarget.TargetId]
                : null;
            var roomViolationTarget = violation.Targets.FirstOrDefault(x => x.TargetType == LessonPolicyViolationTargetType.Room);
            var room = roomViolationTarget != null
                ? roomsById[roomViolationTarget.TargetId]
                : null;
            if (!result.TryGetValue(violation.LessonId!.Value, out var lessonMessages))
            {
                lessonMessages = [];
                result[violation.LessonId!.Value] = lessonMessages;
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
                    affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription(),
                    discipline!.Name),
                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByGroupTemplate,
                    $"\"{affectedByLesson?.LessonBatchInfo.AcademicDiscipline.Name ?? affectedByLessonBatch!.AcademicDiscipline.Name} " +
                    $"({affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription()})\" ",
                    studentGroup!.Name,
                    sourceLessonsById[violation.LessonId!.Value].StudentGroups.Any(x => x.Id == studentGroup.Id)
                        ? "которая совпадает с отмеченной группой"
                        : sourceLessonsById[violation.LessonId!.Value].StudentGroups.SelectMany(x => x.Parents).Any(x => x.Id == studentGroup.Id)
                            ? "которой принадлежит отмеченная группа"
                            : "которая принадлежит отмеченной группе"),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByGroupTemplate,
                    $"\"{affectedByLesson?.LessonBatchInfo.AcademicDiscipline.Name ?? affectedByLessonBatch!.AcademicDiscipline.Name} " +
                    $"({affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription()})\" ",
                    studentGroup!.Name,
                    sourceLessonsById[violation.LessonId!.Value].StudentGroups.Any(x => x.Id == studentGroup.Id)
                        ? "которая совпадает с отмеченной группой"
                        : sourceLessonsById[violation.LessonId!.Value].StudentGroups.SelectMany(x => x.Parents).Any(x => x.Id == studentGroup.Id)
                            ? "которой принадлежит отмеченная группа"
                            : "которая принадлежит отмеченной группе"),
                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByTeacherTemplate,
                    $"\"{affectedByLesson?.LessonBatchInfo.AcademicDiscipline.Name ?? affectedByLessonBatch!.AcademicDiscipline.Name} " +
                    $"({affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription()})\" ",
                    teacher!.Fullname),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByTeacherTemplate,
                    $"\"{affectedByLesson?.LessonBatchInfo.AcademicDiscipline.Name ?? affectedByLessonBatch!.AcademicDiscipline.Name} " +
                    $"({affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription()})\" ",
                    teacher!.Fullname),
                LessonPolicyViolationCode.FixedLessonTypeConflictByRoom => string.Format(
                    LessonPolicyViolationTemplates.FixedLessonTypeConflictByRoomTemplate,
                    $"\"{affectedByLesson?.LessonBatchInfo.AcademicDiscipline.Name ?? affectedByLessonBatch!.AcademicDiscipline.Name} " +
                    $"({affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription()})\" ",
                    room!.Name),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByRoomTemplate,
                    $"\"{affectedByLesson?.LessonBatchInfo.AcademicDiscipline.Name ?? affectedByLessonBatch!.AcademicDiscipline.Name} " +
                    $"({affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription()})\" ",
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
            var targetIdentities = new[]
            {
                new LessonPolicyViolationTargetIdentity(academicDiscipline.Id!.Value, LessonPolicyViolationTargetType.AcademicDiscipline),
                new LessonPolicyViolationTargetIdentity(studentGroup.Id!.Value, LessonPolicyViolationTargetType.StudentGroup),
            };
            violations
                .AddErrorIf(academicDiscipline.SemesterNumber != null
                            && studentGroup.SemesterNumber != null
                            && academicDiscipline.SemesterNumber != studentGroup.SemesterNumber,
                    targetIdentities, LessonPolicyViolationCode.MismatchedSemesterNumber, lesson?.Id);
        }
    }

    public void ValidateAcademicDisciplineTypeMatch(Lesson? lesson,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        AcademicDisciplineType lessonAcademicDisciplineType)
    {
        var targetIdentities = new[]
        {
            new LessonPolicyViolationTargetIdentity(academicDiscipline.Id!.Value, LessonPolicyViolationTargetType.AcademicDiscipline),
            new LessonPolicyViolationTargetIdentity(lesson?.Id!.Value, LessonPolicyViolationTargetType.Lesson),
        };
        violations.AddErrorIf(
            !academicDiscipline.AllowedLessonTypes.Contains(lessonAcademicDisciplineType),
            targetIdentities,
            LessonPolicyViolationCode.MismatchedAcademicDisciplineType,
            lesson?.Id);
    }

    public void ValidateConflictByGroup(Lesson lesson,
        Lesson[] conflictingByGroupLessons,
        LessonBatchInfo[] conflictingByGroupBatches,
        List<LessonPolicyViolation> violations,
        Guid[] hierarchyIds,
        Schedule schedule,
        bool includeTiming = false)
    {
        foreach (var conflictingByGroupLesson in conflictingByGroupLessons)
        {
            foreach (var conflictingGroup in conflictingByGroupLesson.StudentGroups
                         .Where(x => hierarchyIds.Contains(x.Id!.Value)))
            {
                var editedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(conflictingByGroupLesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                    new LessonPolicyViolationTargetIdentity(conflictingGroup.Id!.Value, LessonPolicyViolationTargetType.StudentGroup),
                };
                violations
                    .AddWarningIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                        lessonId: lesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByGroupLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: lesson.DateWithTimeInterval);
                violations
                    .AddErrorIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                        lessonId: lesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByGroupLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: lesson.DateWithTimeInterval);
            }

            foreach (var lessonHierarchyGroup in lesson.StudentGroups
                         .Where(x => hierarchyIds.Contains(x.Id!.Value)))
            {
                var existedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(lesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                    new LessonPolicyViolationTargetIdentity(lessonHierarchyGroup.Id!.Value, LessonPolicyViolationTargetType.StudentGroup),
                };
                violations
                    .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                        lessonId: conflictingByGroupLesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? lesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: conflictingByGroupLesson.DateWithTimeInterval);
                violations
                    .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                        lessonId: conflictingByGroupLesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? lesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: conflictingByGroupLesson.DateWithTimeInterval);
            }
        }

        foreach (var conflictingByGroupBatch in conflictingByGroupBatches)
        {
            var matchedTimeAssignments = conflictingByGroupBatch.DayOfWeekTimeIntervals
                .Where(x => x.DayOfWeekTimeInterval.HasIntersection(lesson.DateWithTimeInterval))
                .ToArray();
            foreach (var conflictingTimeAssignment in matchedTimeAssignments)
            {
                var conflictingDates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                        conflictingByGroupBatch.DateInterval,
                        [conflictingTimeAssignment.DayOfWeekTimeInterval.DayOfWeek],
                        conflictingByGroupBatch.RepeatType,
                        schedule.DateInterval)
                    .Intersect([lesson.DateWithTimeInterval!.Date])
                    .ToArray();
                var intersectsByDate = conflictingDates.Length == 1;

                if (intersectsByDate)
                {
                    foreach (var conflictingGroup in conflictingByGroupBatch.StudentGroups
                                 .Where(x => hierarchyIds.Contains(x.Id!.Value)))
                    {
                        var editedLessonTargetIdentities = new[]
                        {
                            new LessonPolicyViolationTargetIdentity(conflictingByGroupBatch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                            new LessonPolicyViolationTargetIdentity(conflictingGroup.Id!.Value, LessonPolicyViolationTargetType.StudentGroup),
                        };
                        violations
                            .AddWarningIf(conflictingByGroupBatch.FlexibilityType == LessonFlexibilityType.Flexible,
                                editedLessonTargetIdentities,
                                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                                lessonId: lesson.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: lesson.DateWithTimeInterval);
                        violations
                            .AddErrorIf(conflictingByGroupBatch.FlexibilityType == LessonFlexibilityType.Fixed,
                                editedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                                lessonId: lesson.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: lesson.DateWithTimeInterval);
                    }

                    foreach (var lessonHierarchyGroup in lesson.StudentGroups
                                 .Where(x => hierarchyIds.Contains(x.Id!.Value)))
                    {
                        var existedLessonTargetIdentities = new[]
                        {
                            new LessonPolicyViolationTargetIdentity(lesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                            new LessonPolicyViolationTargetIdentity(lessonHierarchyGroup.Id!.Value, LessonPolicyViolationTargetType.StudentGroup),
                        };
                        violations
                            .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                                lessonBatchId: conflictingByGroupBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval(),
                                violationTimestamp: new DateWithTimeInterval { Date = lesson.DateWithTimeInterval.Date, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                        violations
                            .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                                lessonBatchId: conflictingByGroupBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval(),
                                violationTimestamp: new DateWithTimeInterval { Date = lesson.DateWithTimeInterval.Date, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                    }
                }
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
            var targetIdentities = new[]
            {
                new LessonPolicyViolationTargetIdentity(conflictingTeacherPreference.Id!.Value, LessonPolicyViolationTargetType.TeacherPreference),
                new LessonPolicyViolationTargetIdentity(conflictingTeacherPreference.TeacherId, LessonPolicyViolationTargetType.Teacher),
            };
            var dayOfWeekTimeInterval = includeTiming ? conflictingTeacherPreference.DayOfWeekTimeInterval : null;
            violations
                .AddWarningIf(
                    conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    targetIdentities,
                    LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict,
                    lesson.Id,
                    conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval);
            violations
                .AddErrorIf(
                    conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    targetIdentities,
                    LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict,
                    lesson.Id,
                    conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval);
            violations
                .AddWarningIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    targetIdentities,
                    LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict,
                    lesson.Id,
                    conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval);
            violations
                .AddErrorIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    targetIdentities,
                    LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict,
                    lesson.Id,
                    conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval);
        }
    }

    private void ValidateConflictByTeacher(Lesson lesson,
        Guid[] teacherIds,
        Lesson[] conflictingByTeacherLessons,
        LessonBatchInfo[] conflictingByTeacherBatches,
        List<LessonPolicyViolation> violations,
        Schedule schedule,
        bool includeTiming = false)
    {
        foreach (var conflictingByTeacherLesson in conflictingByTeacherLessons)
        {
            foreach (var teacher in conflictingByTeacherLesson.Teachers.Where(x => teacherIds.Contains(x.Id!.Value)))
            {
                var editedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(conflictingByTeacherLesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                    new LessonPolicyViolationTargetIdentity(teacher.Id!.Value, LessonPolicyViolationTargetType.Teacher),
                };
                violations
                    .AddWarningIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                        lesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByTeacherLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null);
                violations
                    .AddErrorIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                        lesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByTeacherLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null);

                var existedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(lesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                    new LessonPolicyViolationTargetIdentity(teacher.Id!.Value, LessonPolicyViolationTargetType.Teacher),
                };
                violations
                    .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                        conflictingByTeacherLesson.Id!.Value);
                violations
                    .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                        conflictingByTeacherLesson.Id!.Value);
            }
        }

        foreach (var conflictingByTeacherBatch in conflictingByTeacherBatches)
        {
            var matchedTimeAssignments = conflictingByTeacherBatch.DayOfWeekTimeIntervals
                .Where(x => x.DayOfWeekTimeInterval.HasIntersection(lesson.DateWithTimeInterval))
                .ToArray();
            foreach (var conflictingTimeAssignment in matchedTimeAssignments)
            {
                var conflictingDates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                        conflictingByTeacherBatch.DateInterval,
                        [conflictingTimeAssignment.DayOfWeekTimeInterval.DayOfWeek],
                        conflictingByTeacherBatch.RepeatType,
                        schedule.DateInterval)
                    .Intersect([lesson.DateWithTimeInterval!.Date])
                    .ToArray();

                var intersectsByDate = conflictingDates.Length == 1;

                if (intersectsByDate)
                {
                    foreach (var teacher in conflictingByTeacherBatch.Teachers.Where(x => teacherIds.Contains(x.Id!.Value)))
                    {
                        var editedLessonTargetIdentities = new[]
                        {
                            new LessonPolicyViolationTargetIdentity(conflictingByTeacherBatch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                            new LessonPolicyViolationTargetIdentity(teacher.Id!.Value, LessonPolicyViolationTargetType.Teacher),
                        };

                        violations
                            .AddWarningIf(conflictingByTeacherBatch.FlexibilityType == LessonFlexibilityType.Flexible,
                                editedLessonTargetIdentities,
                                LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                                lessonId: lesson.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: lesson.DateWithTimeInterval);
                        violations
                            .AddErrorIf(conflictingByTeacherBatch.FlexibilityType == LessonFlexibilityType.Fixed,
                                editedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                                lessonId: lesson.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: lesson.DateWithTimeInterval);

                        var existedLessonTargetIdentities = new[]
                        {
                            new LessonPolicyViolationTargetIdentity(lesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                            new LessonPolicyViolationTargetIdentity(teacher.Id!.Value, LessonPolicyViolationTargetType.Teacher),
                        };
                        violations
                            .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                                lessonBatchId: conflictingByTeacherBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval(),
                                violationTimestamp: new DateWithTimeInterval { Date = lesson.DateWithTimeInterval.Date, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                        violations
                            .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                                lessonBatchId: conflictingByTeacherBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval(),
                                violationTimestamp: new DateWithTimeInterval { Date = lesson.DateWithTimeInterval.Date, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                    }
                }
            }
        }
    }

    private void ValidateConflictByRoom(Lesson lesson,
        Guid[] roomIds,
        Lesson[] conflictingByRoomLessons,
        LessonBatchInfo[] conflictingByRoomBatches,
        List<LessonPolicyViolation> violations,
        Schedule schedule,
        bool includeTiming = false)
    {
        foreach (var conflictingByRoomLesson in conflictingByRoomLessons)
        {
            foreach (var room in conflictingByRoomLesson.Rooms.Where(x => roomIds.Contains(x.Id!.Value)))
            {
                var editedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(conflictingByRoomLesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                    new LessonPolicyViolationTargetIdentity(room.Id!.Value, LessonPolicyViolationTargetType.Room),
                };
                violations
                    .AddWarningIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                        lesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByRoomLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null);
                violations
                    .AddErrorIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                        lesson.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByRoomLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null);

                var existedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(lesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                    new LessonPolicyViolationTargetIdentity(room.Id!.Value, LessonPolicyViolationTargetType.Room),
                };
                violations
                    .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                        conflictingByRoomLesson.Id!.Value);
                violations
                    .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                        conflictingByRoomLesson.Id!.Value);
            }
        }

        foreach (var conflictingByRoomBatch in conflictingByRoomBatches)
        {
            var matchedTimeAssignments = conflictingByRoomBatch.DayOfWeekTimeIntervals
                .Where(x => x.DayOfWeekTimeInterval.HasIntersection(lesson.DateWithTimeInterval))
                .ToArray();
            foreach (var conflictingTimeAssignment in matchedTimeAssignments)
            {
                var conflictingDates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                        conflictingByRoomBatch.DateInterval,
                        [conflictingTimeAssignment.DayOfWeekTimeInterval.DayOfWeek],
                        conflictingByRoomBatch.RepeatType,
                        schedule.DateInterval)
                    .Intersect([lesson.DateWithTimeInterval!.Date])
                    .ToArray();

                foreach (var room in conflictingByRoomBatch.Rooms.Where(x => roomIds.Contains(x.Id!.Value)))
                {
                    var editedLessonTargetIdentities = new[]
                    {
                        new LessonPolicyViolationTargetIdentity(conflictingByRoomBatch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                        new LessonPolicyViolationTargetIdentity(room.Id!.Value, LessonPolicyViolationTargetType.Room),
                    };
                    violations
                        .AddWarningIf(conflictingByRoomBatch.FlexibilityType == LessonFlexibilityType.Flexible,
                            editedLessonTargetIdentities,
                            LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                            lessonId: lesson.Id,
                            conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                            violationTimestamp: lesson.DateWithTimeInterval);
                    violations
                        .AddErrorIf(conflictingByRoomBatch.FlexibilityType == LessonFlexibilityType.Fixed,
                            editedLessonTargetIdentities,
                            LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                            lessonId: lesson.Id,
                            conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                            violationTimestamp: lesson.DateWithTimeInterval);

                    var existedLessonTargetIdentities = new[]
                    {
                        new LessonPolicyViolationTargetIdentity(lesson.Id!.Value, LessonPolicyViolationTargetType.Lesson),
                        new LessonPolicyViolationTargetIdentity(room.Id!.Value, LessonPolicyViolationTargetType.Room),
                    };
                    violations
                        .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                            existedLessonTargetIdentities,
                            LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                            lessonBatchId: conflictingByRoomBatch.Id!.Value,
                            conflictingDayOfWeekTimeInterval: lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval(),
                            violationTimestamp: new DateWithTimeInterval { Date = lesson.DateWithTimeInterval.Date, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                    violations
                        .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                            existedLessonTargetIdentities,
                            LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                            lessonBatchId: conflictingByRoomBatch.Id!.Value,
                            conflictingDayOfWeekTimeInterval: lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval(),
                            violationTimestamp: new DateWithTimeInterval { Date = lesson.DateWithTimeInterval.Date, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                }
            }
        }
    }
}