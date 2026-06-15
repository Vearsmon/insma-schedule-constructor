using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.LessonBatchInfo;
using Dal.Repositories.LessonPolicyViolations;
using Dal.Repositories.Lessons;
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

public class LessonBatchValidationService(
    ILessonRepository lessonRepository,
    ILessonPolicyViolationRepository lessonPolicyViolationRepository,
    ILessonBatchInfoRepository lessonBatchInfoRepository,
    IAcademicDisciplineRepository academicDisciplineRepository,
    ITeacherRepository teacherRepository,
    IScheduleRepository scheduleRepository,
    IRoomRepository roomRepository,
    IStudentGroupRepository studentGroupRepository,
    ITeacherPreferenceRepository teacherPreferenceRepository) : ILessonBatchValidationService
{
    public async Task<LessonPolicyViolation[]> ValidateAsync(LessonBatchInfo lessonBatchInfo)
    {
        var studentGroupIds = lessonBatchInfo.StudentGroups.Select(studentGroup => studentGroup.Id!.Value).Distinct().ToArray();
        var studentGroups = await studentGroupRepository.SelectAsync(studentGroupIds);
        var studentGroupsById = studentGroups.ToDictionary(x => x.Id!.Value);

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(studentGroupIds);

        var teacherIds = lessonBatchInfo.Teachers.Select(teacher => teacher.Id!.Value).Distinct().ToArray();
        var teachers = await teacherRepository.SelectAsync(teacherIds);
        var teachersById = teachers.ToDictionary(x => x.Id!.Value);

        var roomIds = lessonBatchInfo.Rooms.Select(room => room.Id!.Value).Distinct().ToArray();
        var rooms = await roomRepository.SelectAsync(roomIds);
        var roomsById = rooms.ToDictionary(x => x.Id!.Value);

        var batchLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            LessonBatchInfoIds = [lessonBatchInfo.Id!.Value],
        });
        var batchLessonsById = batchLessons.ToDictionary(x => x.Id!.Value);

        var batchViolations = await lessonPolicyViolationRepository.SearchAsync(new LessonPolicyViolationSearchModel
        {
            LessonBatchInfoIds = [lessonBatchInfo.Id!.Value],
        });

        var validationMessages = new List<ValidationMessage>();
        if (lessonBatchInfo.StudentGroups.Any(sg => !studentGroupsById.ContainsKey(sg.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найдены академические группы для сохранения занятия"));
        }

        if (lessonBatchInfo.Teachers.Any(t => !teachersById.ContainsKey(t.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найдены преподаватели для сохранения занятия"));
        }

        if (lessonBatchInfo.Rooms.Any(r => !roomsById.ContainsKey(r.Id!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найдены аудитории для сохранения занятия"));
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var affectedByEditingLessonsPolicyViolations = await lessonPolicyViolationRepository.SearchAsync(
            new LessonPolicyViolationSearchModel
            {
                AffectedByLessonIds = batchLessonsById.Select(lesson => lesson.Key).ToArray(),
            });

        await lessonPolicyViolationRepository.DeleteAsync(batchLessonsById
            .SelectMany(lesson => lesson.Value.Violations
                .Select(violation => violation.Id!.Value))
            .Concat(batchViolations.Select(x => x.Id!.Value))
            .Concat(affectedByEditingLessonsPolicyViolations.Select(x => x.Id!.Value))
            .ToArray());

        var schedule = await scheduleRepository.GetAsync(lessonBatchInfo.AcademicDiscipline.ScheduleId);

        var datesByTimeInterval = lessonBatchInfo.DayOfWeekTimeIntervals
            .Select(dayOfWeekTimeIntervalAssignment => (dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval, Dates: DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                lessonBatchInfo.DateInterval,
                [dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.DayOfWeek],
                lessonBatchInfo.RepeatType,
                schedule.DateInterval)))
            .ToArray();

        var conflictingLessons = await lessonRepository.SearchConflictsAsync(new LessonConflictsSearchModel
        {
            ScheduleId = lessonBatchInfo.AcademicDiscipline.ScheduleId,
            StudentGroupIds = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray(),
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DateWithTimeIntervals = datesByTimeInterval.SelectMany(x => x.Dates.Select(date => new DateWithTimeInterval
            {
                Date = date,
                TimeInterval = x.TimeInterval,
            })).ToArray(),
        });

        var conflictingLessonBatches = await lessonBatchInfoRepository.SearchConflictsAsync(new LessonBatchInfoConflictsSearchModel
        {
            ScheduleId = lessonBatchInfo.AcademicDiscipline.ScheduleId,
            StudentGroupIds = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray(),
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DateWithTimeIntervals = datesByTimeInterval.SelectMany(x => x.Dates.Select(date => new DateWithTimeInterval
            {
                Date = date,
                TimeInterval = x.TimeInterval,
            })).ToArray(),
        });

        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchConflictsAsync(new TeacherPreferenceConflictsSearchModel
        {
            ScheduleId = lessonBatchInfo.AcademicDiscipline.ScheduleId,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DayOfWeekTimeIntervals = datesByTimeInterval.SelectMany(x => x.Dates.Select(date => new DayOfWeekTimeInterval
            {
                DayOfWeek = date.DayOfWeek,
                TimeInterval = x.TimeInterval,
            })).ToArray(),
            TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
        });

        var totalLessonPolicyViolations = new List<LessonPolicyViolation>();

        ValidateAcademicDisciplineStudentGroupMatch(lessonBatchInfo,
            totalLessonPolicyViolations,
            lessonBatchInfo.AcademicDiscipline,
            studentGroups);
        ValidateAcademicDisciplineTypeMatch(lessonBatchInfo,
            totalLessonPolicyViolations,
            lessonBatchInfo.AcademicDiscipline,
            lessonBatchInfo.Type);

        if (lessonBatchInfo.DayOfWeekTimeIntervals.Length == 0) return totalLessonPolicyViolations.ToArray();

        var hierarchyIdsFlat = studentGroupHierarchyIdsByStudentGroupId
            .Where(kv => lessonBatchInfo.StudentGroups.Any(sg => sg.Id == kv.Key))
            .SelectMany(x => x.Value)
            .ToArray();

        foreach (var dayOfWeekTimeIntervalAssignment in lessonBatchInfo.DayOfWeekTimeIntervals)
        {
            var currentBatchConflictingLessons = FilterCurrentConflictingLessons(dayOfWeekTimeIntervalAssignment, conflictingLessons);

            var currentBatchConflictingBatches = FilterCurrentConflictingLessonBatches(dayOfWeekTimeIntervalAssignment);

            var currentBatchConflictingTeacherPreferences = Array.Empty<TeacherPreference>();
            if (lessonBatchInfo.Teachers.Length > 0)
            {
                currentBatchConflictingTeacherPreferences = FilterCurrentConflictingTeacherPreferences(dayOfWeekTimeIntervalAssignment);
            }

            BuildPolicyViolations(totalLessonPolicyViolations,
                dayOfWeekTimeIntervalAssignment,
                studentGroupHierarchyIdsByStudentGroupId,
                currentBatchConflictingLessons,
                currentBatchConflictingBatches,
                lessonBatchInfo,
                teacherIds,
                roomIds,
                currentBatchConflictingTeacherPreferences,
                schedule);
        }

        return totalLessonPolicyViolations.ToArray();

        Lesson[] FilterCurrentConflictingLessons(DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment, Lesson[] conflicting) => conflicting
            .Where(conflictingLesson =>
                (conflictingLesson.DateWithTimeInterval!.Date.IntersectsEvenWeek(schedule.DateInterval)
                    ? lessonBatchInfo.RepeatType != DisciplineLessonRepeatType.OddWeeks
                    : lessonBatchInfo.RepeatType != DisciplineLessonRepeatType.EvenWeeks)
                && dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.HasIntersection(conflictingLesson.DateWithTimeInterval!)
                && (conflictingLesson.StudentGroups.Any(studentGroup =>
                        hierarchyIdsFlat.Contains(studentGroup.Id!.Value))
                    || (lessonBatchInfo.Teachers.Length != 0 && conflictingLesson.Teachers
                        .Any(conflictingLessonTeacher => lessonBatchInfo.Teachers
                            .Any(lessonTeacher => lessonTeacher.Id == conflictingLessonTeacher.Id)))
                    || (lessonBatchInfo.Rooms.Length != 0 && conflictingLesson.Rooms
                        .Any(conflictingLessonRoom => lessonBatchInfo.Rooms
                            .Any(lessonRoom => lessonRoom.Id == conflictingLessonRoom.Id))))
                && (!lessonBatchInfo.AllowCombining || !conflictingLesson.AllowCombining))
            .ToArray();

        LessonBatchInfo[] FilterCurrentConflictingLessonBatches(DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment) => conflictingLessonBatches
            .Where(conflictingLessonBatch =>
                conflictingLessonBatch.Id != lessonBatchInfo.Id
                && (lessonBatchInfo.RepeatType == DisciplineLessonRepeatType.EvenWeeks
                    ? conflictingLessonBatch.RepeatType != DisciplineLessonRepeatType.OddWeeks
                    : lessonBatchInfo.RepeatType != DisciplineLessonRepeatType.OddWeeks
                      || conflictingLessonBatch.RepeatType != DisciplineLessonRepeatType.EvenWeeks)
                && conflictingLessonBatch.DayOfWeekTimeIntervals.Any(x =>
                    x.DayOfWeekTimeInterval.HasIntersection(dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval))
                && (conflictingLessonBatch.StudentGroups.Any(studentGroup =>
                        hierarchyIdsFlat.Contains(studentGroup.Id!.Value))
                    || (lessonBatchInfo.Teachers.Length != 0 && conflictingLessonBatch.Teachers
                        .Any(conflictingLessonTeacher => lessonBatchInfo.Teachers
                            .Any(lessonTeacher => lessonTeacher.Id == conflictingLessonTeacher.Id)))
                    || (lessonBatchInfo.Rooms.Length != 0 && conflictingLessonBatch.Rooms
                        .Any(conflictingLessonRoom => lessonBatchInfo.Rooms
                            .Any(lessonRoom => lessonRoom.Id == conflictingLessonRoom.Id))))
                && (!lessonBatchInfo.AllowCombining || !conflictingLessonBatch.AllowCombining))
            .ToArray();

        TeacherPreference[] FilterCurrentConflictingTeacherPreferences(DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment) => conflictingTeacherPreferences
            .Where(conflictingTeacherPreference =>
                lessonBatchInfo.Teachers.Any(teacher => teacher.Id!.Value == conflictingTeacherPreference.TeacherId)
                && (!conflictingTeacherPreference.RoomId.HasValue
                    || lessonBatchInfo.Rooms.Length == 0
                    || lessonBatchInfo.Rooms.Any(room => room.Id!.Value == conflictingTeacherPreference.RoomId!.Value))
                && (conflictingTeacherPreference.DayOfWeekTimeInterval == null
                    || (conflictingTeacherPreference.DayOfWeekTimeInterval!.HasIntersection(dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval))))
            .ToArray();
    }

    public async Task<LessonSeriesConflictDto[]> FillValidationMessages(LessonBatchInfo[] batches)
    {
        var lessonConflicts = new List<LessonSeriesConflictDto>();

        var messages = await GetValidationResultMessageAsync(
            batches.SelectMany(batch => batch.Violations).ToArray());
        var messagesByBatchId = messages.ToDictionary(message => message.LessonBatchInfoId!.Value, message => message.MessagesByViolationId);
        lessonConflicts.AddRange(batches.SelectMany(batch => batch.Violations
            .Select(violation => new LessonSeriesConflictDto
            {
                LessonBatchInfoIds = [batch.Id!.Value],
                DayOfWeekTimeInterval = violation.Timestamp?.ToDayOfWeekTimeInterval(),
                Messages = [new LessonSeriesConflictMessageDto
                {
                    TimeInterval = violation.Timestamp?.TimeInterval,
                    Message = messagesByBatchId[batch.Id!.Value][violation.Id!.Value],
                    ErrorType = violation.ErrorType,
                }],
                MaxErrorType = violation.ErrorType,
            })));

        return lessonConflicts.ToArray();
    }

    public async Task<LessonValidationMessageBatchDto[]> GetValidationResultMessageAsync(LessonPolicyViolation[] violations)
    {
        var sourceLessonBatchInfoIds = violations.Where(x => x.LessonBatchInfoId.HasValue).Select(x => x.LessonBatchInfoId!.Value).ToArray();
        var sourceLessonBatchInfosById = (await lessonBatchInfoRepository.SelectAsync(sourceLessonBatchInfoIds)).ToDictionary(x => x.Id!.Value);
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
        foreach (var violation in violations.Where(x => x.LessonBatchInfoId.HasValue))
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
            if (!result.TryGetValue(violation.LessonBatchInfoId!.Value, out var lessonBatchMessages))
            {
                lessonBatchMessages = [];
                result[violation.LessonBatchInfoId!.Value] = lessonBatchMessages;
            }

            lessonBatchMessages[violation.Id!.Value] = violation.Code switch
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
                    sourceLessonBatchInfosById[violation.LessonBatchInfoId!.Value].StudentGroups.Any(x => x.Id == studentGroup.Id)
                        ? "которая совпадает с отмеченной группой"
                        : sourceLessonBatchInfosById[violation.LessonBatchInfoId!.Value].StudentGroups.SelectMany(x => x.Parents).Any(x => x.Id == studentGroup.Id)
                            ? "которой принадлежит отмеченная группа"
                            : "которая принадлежит отмеченной группе"),
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup => string.Format(
                    LessonPolicyViolationTemplates.FlexibleLessonTypeConflictByGroupTemplate,
                    $"\"{affectedByLesson?.LessonBatchInfo.AcademicDiscipline.Name ?? affectedByLessonBatch!.AcademicDiscipline.Name} " +
                    $"({affectedByLesson?.LessonBatchInfo.Type.GetDescription() ?? affectedByLessonBatch!.Type.GetDescription()})\" ",
                    studentGroup!.Name,
                    sourceLessonBatchInfosById[violation.LessonBatchInfoId!.Value].StudentGroups.Any(x => x.Id == studentGroup.Id)
                        ? "которая совпадает с отмеченной группой"
                        : sourceLessonBatchInfosById[violation.LessonBatchInfoId!.Value].StudentGroups.SelectMany(x => x.Parents).Any(x => x.Id == studentGroup.Id)
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

    public void BuildPolicyViolations(List<LessonPolicyViolation> lessonPolicyViolations,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
        Dictionary<Guid,List<Guid>> studentGroupHierarchyIdsByStudentGroupId,
        Lesson[] conflictingLessons,
        LessonBatchInfo[] conflictingBatches,
        LessonBatchInfo batch,
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
            ValidateConflictByGroup(batch, dayOfWeekTimeIntervalAssignment, conflictingByGroupHierarchyLessons,
                conflictingByGroupHierarchyBatches, lessonPolicyViolations, hierarchyIds.ToArray(), schedule,
                includeTiming);
        }

        var conflictingByTeacherLessons = conflictingLessons
            .Where(x => x.Teachers.Any(y => teacherIds.Contains(y.Id!.Value)))
            .ToArray();
        var conflictingByTeacherBatches = conflictingBatches
            .Where(x => x.Teachers.Any(y => teacherIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateConflictByTeacher(batch, dayOfWeekTimeIntervalAssignment, teacherIds, conflictingByTeacherLessons,
            conflictingByTeacherBatches, lessonPolicyViolations, schedule, includeTiming);

        ValidateTeacherPreferenceConflict(batch, dayOfWeekTimeIntervalAssignment, conflictingTeacherPreferences,
            lessonPolicyViolations, schedule, includeTiming);

        var conflictingByRoomLessons = conflictingLessons
            .Where(x => x.Rooms.Any(y => roomIds.Contains(y.Id!.Value)))
            .ToArray();
        var conflictingByRoomBatches = conflictingBatches
            .Where(x => x.Rooms.Any(y => roomIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateConflictByRoom(batch, dayOfWeekTimeIntervalAssignment, roomIds, conflictingByRoomLessons,
            conflictingByRoomBatches, lessonPolicyViolations, schedule, includeTiming);
    }

    public void ValidateAcademicDisciplineStudentGroupMatch(LessonBatchInfo? lessonBatch,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        StudentGroup[] studentGroups)
    {
        foreach (var studentGroup in studentGroups)
        {
            var targetIdentities = new[]
            {
                new LessonPolicyViolationTargetIdentity(academicDiscipline.Id!.Value, LessonPolicyViolationTargetType.AcademicDiscipline),
                new LessonPolicyViolationTargetIdentity(studentGroup.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
            };
            violations
                .AddErrorIf(academicDiscipline.SemesterNumber != null
                            && studentGroup.SemesterNumber != null
                            && academicDiscipline.SemesterNumber != studentGroup.SemesterNumber,
                    targetIdentities, LessonPolicyViolationCode.MismatchedSemesterNumber, lessonBatchId: lessonBatch?.Id);
        }
    }

    public void ValidateAcademicDisciplineTypeMatch(LessonBatchInfo? lessonBatch,
        List<LessonPolicyViolation> violations,
        AcademicDiscipline academicDiscipline,
        AcademicDisciplineType lessonAcademicDisciplineType)
    {
        var targetIdentities = new[]
        {
            new LessonPolicyViolationTargetIdentity(academicDiscipline.Id!.Value, LessonPolicyViolationTargetType.AcademicDiscipline),
            new LessonPolicyViolationTargetIdentity(lessonBatch?.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
        };
        violations.AddErrorIf(
            !academicDiscipline.AllowedLessonTypes.Contains(lessonAcademicDisciplineType),
            targetIdentities,
            LessonPolicyViolationCode.MismatchedAcademicDisciplineType,
            lessonBatchId: lessonBatch?.Id);
    }

    public void ValidateConflictByGroup(LessonBatchInfo batch,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
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
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByGroupLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: new DateWithTimeInterval { Date = conflictingByGroupLesson.DateWithTimeInterval!.Date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                violations
                    .AddErrorIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByGroupLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: new DateWithTimeInterval { Date = conflictingByGroupLesson.DateWithTimeInterval!.Date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
            }

            foreach (var lessonHierarchyGroup in batch.StudentGroups
                         .Where(x => hierarchyIds.Contains(x.Id!.Value)))
            {
                var existedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(batch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                    new LessonPolicyViolationTargetIdentity(lessonHierarchyGroup.Id!.Value, LessonPolicyViolationTargetType.StudentGroup),
                };
                violations
                    .AddErrorIf(batch.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                        lessonId: conflictingByGroupLesson.Id!.Value,
                        conflictingDayOfWeekTimeInterval: includeTiming
                            ? new DayOfWeekTimeInterval { DayOfWeek = conflictingByGroupLesson.DateWithTimeInterval!.Date.DayOfWeek, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval }
                            : null,
                        violationTimestamp: conflictingByGroupLesson.DateWithTimeInterval);
                violations
                    .AddWarningIf(batch.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                        lessonId: conflictingByGroupLesson.Id!.Value,
                        conflictingDayOfWeekTimeInterval: includeTiming
                            ? new DayOfWeekTimeInterval { DayOfWeek = conflictingByGroupLesson.DateWithTimeInterval!.Date.DayOfWeek, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval }
                            : null,
                        violationTimestamp: conflictingByGroupLesson.DateWithTimeInterval);
            }
        }

        foreach (var conflictingByGroupBatch in conflictingByGroupBatches)
        {
            var matchedTimeAssignments = conflictingByGroupBatch.DayOfWeekTimeIntervals
                .Where(x => x.DayOfWeekTimeInterval.HasIntersection(dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval))
                .ToArray();
            foreach (var conflictingTimeAssignment in matchedTimeAssignments)
            {
                var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                    batch.DateInterval,
                    [dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.DayOfWeek],
                    batch.RepeatType,
                    schedule.DateInterval);

                var conflictingDates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                        conflictingByGroupBatch.DateInterval,
                        [conflictingTimeAssignment.DayOfWeekTimeInterval.DayOfWeek],
                        conflictingByGroupBatch.RepeatType,
                        schedule.DateInterval)
                    .Intersect(dates)
                    .ToArray();

                foreach (var conflictingGroup in conflictingByGroupBatch.StudentGroups
                         .Where(x => hierarchyIds.Contains(x.Id!.Value)))
                {
                    foreach (var conflictingDate in conflictingDates)
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
                                lessonBatchId: batch.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                        violations
                            .AddErrorIf(conflictingByGroupBatch.FlexibilityType == LessonFlexibilityType.Fixed,
                                editedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                                lessonBatchId: batch.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                    }
                }

                foreach (var lessonHierarchyGroup in batch.StudentGroups
                             .Where(x => hierarchyIds.Contains(x.Id!.Value)))
                {
                    foreach (var conflictingDate in conflictingDates)
                    {
                        var existedLessonTargetIdentities = new[]
                        {
                            new LessonPolicyViolationTargetIdentity(batch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                            new LessonPolicyViolationTargetIdentity(lessonHierarchyGroup.Id!.Value, LessonPolicyViolationTargetType.StudentGroup),
                        };
                        violations
                            .AddErrorIf(batch.FlexibilityType == LessonFlexibilityType.Fixed,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
                                lessonBatchId: conflictingByGroupBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: conflictingTimeAssignment.DayOfWeekTimeInterval,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                        violations
                            .AddWarningIf(batch.FlexibilityType == LessonFlexibilityType.Flexible,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                                lessonBatchId: conflictingByGroupBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: conflictingTimeAssignment.DayOfWeekTimeInterval,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                    }
                }
            }
        }
    }

    public void ValidateTeacherPreferenceConflict(LessonBatchInfo batch,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
        TeacherPreference[] conflictingTeacherPreferences,
        List<LessonPolicyViolation> violations,
        Schedule schedule,
        bool includeTiming = false)
    {
        var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
            batch.DateInterval,
            [dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.DayOfWeek],
            batch.RepeatType,
            schedule.DateInterval);

        foreach (var conflictingTeacherPreference in conflictingTeacherPreferences)
        {
            var targetIdentities = new[]
            {
                new LessonPolicyViolationTargetIdentity(conflictingTeacherPreference.Id!.Value, LessonPolicyViolationTargetType.TeacherPreference),
                new LessonPolicyViolationTargetIdentity(conflictingTeacherPreference.TeacherId, LessonPolicyViolationTargetType.Teacher),
            };
            var dayOfWeekTimeInterval = includeTiming ? conflictingTeacherPreference.DayOfWeekTimeInterval : null;

            foreach (var date in dates)
            {
                violations
                    .AddWarningIf(
                        conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                        targetIdentities,
                        LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict,
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval,
                        violationTimestamp: new DateWithTimeInterval { Date = date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                violations
                    .AddErrorIf(
                        conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                        targetIdentities,
                        LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict,
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval,
                        violationTimestamp: new DateWithTimeInterval { Date = date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                violations
                    .AddWarningIf(
                        conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                        targetIdentities,
                        LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict,
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval,
                        violationTimestamp: new DateWithTimeInterval { Date = date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                violations
                    .AddErrorIf(
                        conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                        targetIdentities,
                        LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict,
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: dayOfWeekTimeInterval,
                        violationTimestamp: new DateWithTimeInterval { Date = date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
            }
        }
    }

    private void ValidateConflictByTeacher(LessonBatchInfo batch,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
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
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByTeacherLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: new DateWithTimeInterval { Date = conflictingByTeacherLesson.DateWithTimeInterval!.Date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                violations
                    .AddErrorIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByTeacherLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: new DateWithTimeInterval { Date = conflictingByTeacherLesson.DateWithTimeInterval!.Date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });

                var existedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(batch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                    new LessonPolicyViolationTargetIdentity(teacher.Id!.Value, LessonPolicyViolationTargetType.Teacher),
                };
                violations
                    .AddErrorIf(batch.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                        lessonId: conflictingByTeacherLesson.Id!.Value,
                        conflictingDayOfWeekTimeInterval: includeTiming
                            ? new DayOfWeekTimeInterval { DayOfWeek = conflictingByTeacherLesson.DateWithTimeInterval!.Date.DayOfWeek, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval }
                            : null,
                        violationTimestamp: conflictingByTeacherLesson.DateWithTimeInterval);
                violations
                    .AddWarningIf(batch.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                        lessonId: conflictingByTeacherLesson.Id!.Value,
                        conflictingDayOfWeekTimeInterval: includeTiming
                            ? new DayOfWeekTimeInterval { DayOfWeek = conflictingByTeacherLesson.DateWithTimeInterval!.Date.DayOfWeek, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval }
                            : null,
                        violationTimestamp: conflictingByTeacherLesson.DateWithTimeInterval);
            }
        }

        foreach (var conflictingByTeacherBatch in conflictingByTeacherBatches)
        {
            var matchedTimeAssignments = conflictingByTeacherBatch.DayOfWeekTimeIntervals
                .Where(x => x.DayOfWeekTimeInterval.HasIntersection(dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval))
                .ToArray();
            foreach (var conflictingTimeAssignment in matchedTimeAssignments)
            {
                var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                    batch.DateInterval,
                    [dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.DayOfWeek],
                    batch.RepeatType,
                    schedule.DateInterval);

                var conflictingDates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                        conflictingByTeacherBatch.DateInterval,
                        [conflictingTimeAssignment.DayOfWeekTimeInterval.DayOfWeek],
                        conflictingByTeacherBatch.RepeatType,
                        schedule.DateInterval)
                    .Intersect(dates)
                    .ToArray();

                foreach (var teacher in conflictingByTeacherBatch.Teachers.Where(x => teacherIds.Contains(x.Id!.Value)))
                {
                    foreach (var conflictingDate in conflictingDates)
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
                                lessonBatchId: batch.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                        violations
                            .AddErrorIf(conflictingByTeacherBatch.FlexibilityType == LessonFlexibilityType.Fixed,
                                editedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                                lessonBatchId: batch.Id,
                                conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });

                        var existedLessonTargetIdentities = new[]
                        {
                            new LessonPolicyViolationTargetIdentity(batch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                            new LessonPolicyViolationTargetIdentity(teacher.Id!.Value, LessonPolicyViolationTargetType.Teacher),
                        };
                        violations
                            .AddErrorIf(batch.FlexibilityType == LessonFlexibilityType.Fixed,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher,
                                lessonBatchId: conflictingByTeacherBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: conflictingTimeAssignment.DayOfWeekTimeInterval,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                        violations
                            .AddWarningIf(batch.FlexibilityType == LessonFlexibilityType.Flexible,
                                existedLessonTargetIdentities,
                                LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher,
                                lessonBatchId: conflictingByTeacherBatch.Id!.Value,
                                conflictingDayOfWeekTimeInterval: conflictingTimeAssignment.DayOfWeekTimeInterval,
                                violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                    }
                }
            }
        }
    }

    private void ValidateConflictByRoom(LessonBatchInfo batch,
        DayOfWeekTimeIntervalAssignment dayOfWeekTimeIntervalAssignment,
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
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByRoomLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: new DateWithTimeInterval { Date = conflictingByRoomLesson.DateWithTimeInterval!.Date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                violations
                    .AddErrorIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                        lessonBatchId: batch.Id,
                        conflictingDayOfWeekTimeInterval: includeTiming ? conflictingByRoomLesson.DateWithTimeInterval!.ToDayOfWeekTimeInterval() : null,
                        violationTimestamp: new DateWithTimeInterval { Date = conflictingByRoomLesson.DateWithTimeInterval!.Date, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });

                var existedLessonTargetIdentities = new[]
                {
                    new LessonPolicyViolationTargetIdentity(batch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                    new LessonPolicyViolationTargetIdentity(room.Id!.Value, LessonPolicyViolationTargetType.Room),
                };
                violations
                    .AddErrorIf(batch.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                        lessonId: conflictingByRoomLesson.Id!.Value,
                        conflictingDayOfWeekTimeInterval: includeTiming
                            ? new DayOfWeekTimeInterval { DayOfWeek = conflictingByRoomLesson.DateWithTimeInterval!.Date.DayOfWeek, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval }
                            : null,
                        violationTimestamp: conflictingByRoomLesson.DateWithTimeInterval);
                violations
                    .AddWarningIf(batch.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonTargetIdentities,
                        LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                        lessonId: conflictingByRoomLesson.Id!.Value,
                        conflictingDayOfWeekTimeInterval: includeTiming
                            ? new DayOfWeekTimeInterval { DayOfWeek = conflictingByRoomLesson.DateWithTimeInterval!.Date.DayOfWeek, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval }
                            : null,
                        violationTimestamp: conflictingByRoomLesson.DateWithTimeInterval);
            }
        }

        foreach (var conflictingByRoomBatch in conflictingByRoomBatches)
        {
            var matchedTimeAssignments = conflictingByRoomBatch.DayOfWeekTimeIntervals
                .Where(x => x.DayOfWeekTimeInterval.HasIntersection(dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval))
                .ToArray();
            foreach (var conflictingTimeAssignment in matchedTimeAssignments)
            {
                var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                    batch.DateInterval,
                    [dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.DayOfWeek],
                    batch.RepeatType,
                    schedule.DateInterval);

                var conflictingDates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                        conflictingByRoomBatch.DateInterval,
                        [conflictingTimeAssignment.DayOfWeekTimeInterval.DayOfWeek],
                        conflictingByRoomBatch.RepeatType,
                        schedule.DateInterval)
                    .Intersect(dates)
                    .ToArray();

                foreach (var room in conflictingByRoomBatch.Rooms.Where(x => roomIds.Contains(x.Id!.Value)))
                {
                    foreach (var conflictingDate in conflictingDates)
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
                            lessonBatchId: batch.Id,
                            conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                            violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });
                    violations
                        .AddErrorIf(conflictingByRoomBatch.FlexibilityType == LessonFlexibilityType.Fixed,
                            editedLessonTargetIdentities,
                            LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                            lessonBatchId: batch.Id,
                            conflictingDayOfWeekTimeInterval: includeTiming ? conflictingTimeAssignment.DayOfWeekTimeInterval : null,
                            violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = dayOfWeekTimeIntervalAssignment.DayOfWeekTimeInterval.TimeInterval });

                    var existedLessonTargetIdentities = new[]
                    {
                        new LessonPolicyViolationTargetIdentity(batch.Id!.Value, LessonPolicyViolationTargetType.LessonBatch),
                        new LessonPolicyViolationTargetIdentity(room.Id!.Value, LessonPolicyViolationTargetType.Room),
                    };
                    violations
                        .AddErrorIf(batch.FlexibilityType == LessonFlexibilityType.Fixed,
                            existedLessonTargetIdentities,
                            LessonPolicyViolationCode.FixedLessonTypeConflictByRoom,
                            lessonBatchId: conflictingByRoomBatch.Id!.Value,
                            conflictingDayOfWeekTimeInterval: conflictingTimeAssignment.DayOfWeekTimeInterval,
                            violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                    violations
                        .AddWarningIf(batch.FlexibilityType == LessonFlexibilityType.Flexible,
                            existedLessonTargetIdentities,
                            LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom,
                            lessonBatchId: conflictingByRoomBatch.Id!.Value,
                            conflictingDayOfWeekTimeInterval: conflictingTimeAssignment.DayOfWeekTimeInterval,
                            violationTimestamp: new DateWithTimeInterval { Date = conflictingDate, TimeInterval = conflictingTimeAssignment.DayOfWeekTimeInterval.TimeInterval });
                    }
                }
            }
        }
    }
}