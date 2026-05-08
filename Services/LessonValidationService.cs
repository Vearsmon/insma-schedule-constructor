using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonValidationMessages;
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
    ILessonValidationMessageRepository lessonValidationMessageRepository,
    IScheduleRepository scheduleRepository,
    ITeacherRepository teacherRepository,
    IAcademicDisciplineRepository academicDisciplineRepository,
    IRoomRepository roomRepository,
    IStudentGroupRepository studentGroupRepository,
    ITeacherPreferenceRepository teacherPreferenceRepository) : ILessonValidationService
{
    public async Task<LessonValidationResult> ValidateAsync(Lesson lesson)
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

        if (!(await scheduleRepository.ExistsAsync(lesson.ScheduleId)))
        {
            validationMessages.Add(new ValidationMessage("Не найден проект расписания для сохранения занятия"));
        }

        if (studentGroups.Length != lesson.StudentGroups.Length)
        {
            validationMessages.Add(new ValidationMessage("Не найдены академические группы для сохранения занятия"));
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
            var affectedByEditingLessonValidationMessages = await lessonValidationMessageRepository.SearchAsync(
                new LessonValidationMessageSearchModel { AffectedByLessonIds = [lesson.Id!.Value] });
            await lessonValidationMessageRepository.DeleteAsync(previousLesson.ValidationMessages
                .Select(x => x.Id!.Value)
                .Concat(affectedByEditingLessonValidationMessages.Select(x => x.Id!.Value)).ToArray());
        }

        var lessonValidationMessages = new List<LessonValidationMessage>();
        var affectedLessonNewValidationMessagesByLessonId = new Dictionary<Guid, List<LessonValidationMessage>?>();

        if (lesson.AcademicDisciplineId.HasValue)
        {
            ValidateAcademicDisciplineStudentGroupMatch(lessonValidationMessages, academicDiscipline!,
                studentGroups);
            ValidateAcademicDisciplineTypeMatch(lessonValidationMessages, academicDiscipline!,
                lesson.AcademicDisciplineType!.Value);
        }

        if (lesson.DateWithTimeInterval == null)
        {
            return new LessonValidationResult
            {
                Messages = lessonValidationMessages.ToArray(),
                LessonsWithConflictById = new Dictionary<Guid, Lesson>(),
            };
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

        BuildValidationMessages(lessonValidationMessages, studentGroupHierarchyIdsByStudentGroupId,
            conflictingLessons, lesson, teacherIds, roomIds, conflictingTeacherPreferences,
            affectedLessonNewValidationMessagesByLessonId);

        var lessonsWithConflictById = conflictingLessons.DistinctBy(x => x.Id).ToDictionary(x => x.Id!.Value);
        foreach (var (lessonId, affectedLessonValidationMessages) in affectedLessonNewValidationMessagesByLessonId)
        {
            lessonsWithConflictById[lessonId].ValidationMessages = lessonsWithConflictById[lessonId].ValidationMessages
                .Concat(affectedLessonValidationMessages!).ToArray();
        }

        return new LessonValidationResult
        {
            Messages = lessonValidationMessages.ToArray(),
            LessonsWithConflictById = lessonsWithConflictById,
        };
    }

    public void BuildValidationMessages(List<LessonValidationMessage> lessonValidationMessages,
        Dictionary<Guid,List<Guid>> studentGroupHierarchyIdsByStudentGroupId,
        Lesson[] conflictingLessons,
        Lesson? lesson,
        Guid[] teacherIds,
        Guid[] roomIds,
        TeacherPreference[] conflictingTeacherPreferences,
        Dictionary<Guid,List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
        bool includeTiming = false)
    {
        foreach (var hierarchyIds in studentGroupHierarchyIdsByStudentGroupId.Values)
        {
            var conflictingByGroupHierarchyLessons = conflictingLessons
                .Where(x => x.StudentGroups.Any(y => hierarchyIds.Contains(y.Id!.Value)))
                .ToArray();
            ValidateLessonConflictByGroup(lesson, conflictingByGroupHierarchyLessons, lessonValidationMessages,
                affectedLessonNewValidationMessagesByLessonId, hierarchyIds.ToArray(), includeTiming);
        }

        var conflictingByTeacherLessons = conflictingLessons
            .Where(x => x.Teachers.Any(y => teacherIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateLessonConflictByTeacher(lesson?.Id, lesson?.FlexibilityType, teacherIds, conflictingByTeacherLessons,
            lessonValidationMessages, affectedLessonNewValidationMessagesByLessonId, includeTiming);

        ValidateTeacherPreferenceConflict(conflictingTeacherPreferences, lessonValidationMessages, includeTiming);

        var conflictingByRoomLessons = conflictingLessons
            .Where(x => x.Rooms.Any(y => roomIds.Contains(y.Id!.Value)))
            .ToArray();
        ValidateLessonConflictByRoom(lesson, roomIds, conflictingByRoomLessons, lessonValidationMessages,
            affectedLessonNewValidationMessagesByLessonId, includeTiming);
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

            var messages = await GetValidationResultMessageAsync(lesson.ValidationMessages, lesson, studentGroupAcademicDisciplineLessonsCache);
            var index = 0;
            foreach (var validationMessage in lesson.ValidationMessages)
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
                    ErrorType = validationMessage.ErrorType,
                });
            }
        }

        return lessonConflicts.ToArray();
    }

    public async Task<string[]> GetValidationResultMessageAsync(LessonValidationMessage[] validationMessages,
        Lesson? lesson = null, Dictionary<(Guid, Guid, Guid),List<Lesson>>? studentGroupAcademicDisciplineLessonsCache = null)
    {
        var disciplineIds = validationMessages.Where(x => x.Payload.AffectedByAcademicDisciplineId.HasValue)
            .Select(x => x.Payload.AffectedByAcademicDisciplineId!.Value)
            .Distinct()
            .ToArray();
        var disciplinesById = (await academicDisciplineRepository.SelectAsync(disciplineIds)).ToDictionary(x => x.Id!.Value);
        var studentGroupsIds = validationMessages.Where(x => x.Payload.AffectedByStudentGroupId.HasValue)
            .Select(x => x.Payload.AffectedByStudentGroupId!.Value)
            .Distinct()
            .ToArray();
        var studentGroupsById = (await studentGroupRepository.SelectAsync(studentGroupsIds)).ToDictionary(x => x.Id!.Value);
        var lessonIds = validationMessages.Where(x => x.Payload.AffectedByLessonId.HasValue)
            .Select(x => x.Payload.AffectedByLessonId!.Value)
            .Distinct()
            .ToArray();
        var lessonsById = (await lessonRepository.SelectAsync(lessonIds)).ToDictionary(x => x.Id!.Value);
        var teacherIds = validationMessages.Where(x => x.Payload.AffectedByTeacherId.HasValue)
            .Select(x => x.Payload.AffectedByTeacherId!.Value)
            .Distinct()
            .ToArray();
        var teachersById = (await teacherRepository.SelectAsync(teacherIds)).ToDictionary(x => x.Id!.Value);
        var roomIds = validationMessages.Where(x => x.Payload.AffectedByRoomId.HasValue)
            .Select(x => x.Payload.AffectedByRoomId!.Value)
            .Distinct()
            .ToArray();
        var roomsById = (await roomRepository.SelectAsync(roomIds)).ToDictionary(x => x.Id!.Value);

        var result = new List<string>();
        foreach (var validationMessage in validationMessages)
        {
            var discipline = validationMessage.Payload.AffectedByAcademicDisciplineId.HasValue
                ? disciplinesById[validationMessage.Payload.AffectedByAcademicDisciplineId!.Value]
                : null;
            var studentGroup = validationMessage.Payload.AffectedByStudentGroupId.HasValue
                ? studentGroupsById[validationMessage.Payload.AffectedByStudentGroupId!.Value]
                : null;
            var affectedByLesson = validationMessage.Payload.AffectedByLessonId.HasValue
                ? lessonsById[validationMessage.Payload.AffectedByLessonId!.Value]
                : null;
            var teacher = validationMessage.Payload.AffectedByTeacherId.HasValue
                ? teachersById[validationMessage.Payload.AffectedByTeacherId!.Value]
                : null;
            var room = validationMessage.Payload.AffectedByRoomId.HasValue
                ? roomsById[validationMessage.Payload.AffectedByRoomId!.Value]
                : null;
            result.Add(validationMessage.Code switch
            {
                LessonValidationCode.MismatchedSemesterNumber => string.Format(
                    LessonValidationMessageTemplates.MismatchedSemesterNumberTemplate,
                    discipline!.Name,
                    studentGroup!.Name,
                    studentGroup.SemesterNumber,
                    discipline.SemesterNumber),
                LessonValidationCode.MismatchedAcademicDisciplineType => string.Format(
                    LessonValidationMessageTemplates.MismatchedAcademicDisciplineTypeTemplate,
                    discipline!.Name,
                    lesson!.AcademicDisciplineType!.GetDescription()),
                LessonValidationCode.FixedLessonTypeConflictByGroup => string.Format(
                    LessonValidationMessageTemplates.FixedLessonTypeConflictByGroupTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    studentGroup!.Name),
                LessonValidationCode.FlexibleLessonTypeConflictByGroup => string.Format(
                    LessonValidationMessageTemplates.FlexibleLessonTypeConflictByGroupTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    studentGroup!.Name),
                LessonValidationCode.FixedLessonTypeConflictByTeacher => string.Format(
                    LessonValidationMessageTemplates.FixedLessonTypeConflictByTeacherTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    teacher!.Fullname),
                LessonValidationCode.FlexibleLessonTypeConflictByTeacher => string.Format(
                    LessonValidationMessageTemplates.FlexibleLessonTypeConflictByTeacherTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    teacher!.Fullname),
                LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict => string.Format(
                    LessonValidationMessageTemplates.RestrictedTimeTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonValidationCode.UndesirableTimeTeacherPreferenceTypeConflict => string.Format(
                    LessonValidationMessageTemplates.UndesirableTimeTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict => string.Format(
                    LessonValidationMessageTemplates.RestrictedRoomTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonValidationCode.UndesirableRoomTeacherPreferenceTypeConflict => string.Format(
                    LessonValidationMessageTemplates.UndesirableRoomTeacherPreferenceTypeConflictTemplate,
                    teacher!.Fullname),
                LessonValidationCode.FixedLessonTypeConflictByRoom => string.Format(
                    LessonValidationMessageTemplates.FixedLessonTypeConflictByRoomTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    room!.Name),
                LessonValidationCode.FlexibleLessonTypeConflictByRoom => string.Format(
                    LessonValidationMessageTemplates.FlexibleLessonTypeConflictByRoomTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    room!.Name),
                LessonValidationCode.MismatchedAcademicDisciplineTypeTotalHoursCount => string.Format(
                    LessonValidationMessageTemplates.MismatchedAcademicDisciplineTypeTotalHoursCountTemplate,
                    validationMessage.Payload.AffectedByAcademicDisciplineType!.Value.GetDescription(),
                    discipline!.Name,
                    studentGroupAcademicDisciplineLessonsCache![(lesson!.ScheduleId, lesson.AcademicDisciplineId!.Value, studentGroup!.Id!.Value)]
                        .Where(x => x.AcademicDisciplineType == validationMessage.Payload.AffectedByAcademicDisciplineType)
                        .Sum(x => x.HoursCost),
                    discipline.GetPayloadByType(validationMessage.Payload.AffectedByAcademicDisciplineType!.Value)!.TotalHoursCount,
                    studentGroup.Name),
                _ => throw new NotSupportedException(),
            });
        }

        return result.ToArray();
    }

    public async Task RemoveValidationMessages(Guid[] lessonIds, LessonValidationCode[] validationCodes)
    {
        var selfValidationMessages = await lessonValidationMessageRepository.SearchAsync(
            new LessonValidationMessageSearchModel
            {
                LessonIds = lessonIds,
                ValidationCodes = validationCodes,
            });
        var affectedValidationMessages = await lessonValidationMessageRepository.SearchAsync(
            new LessonValidationMessageSearchModel
            {
                AffectedByLessonIds = lessonIds,
                ValidationCodes = validationCodes,
            });
        await lessonValidationMessageRepository.DeleteAsync(selfValidationMessages
            .Concat(affectedValidationMessages)
            .Select(x => x.Id!.Value)
            .ToArray());
    }

    public async Task RemoveValidationMessages(Guid academicDisciplineId)
    {
        var validationMessages = await lessonValidationMessageRepository.SearchAsync(
            new LessonValidationMessageSearchModel
            {
                AffectedByAcademicDisciplineIds = [academicDisciplineId],
            });
        await lessonValidationMessageRepository.DeleteAsync(validationMessages.Select(x => x.Id!.Value).ToArray());
    }

    public void ValidateAcademicDisciplineStudentGroupMatch(List<LessonValidationMessage> validationMessages,
        AcademicDiscipline saveDtoAcademicDiscipline,
        StudentGroup[] saveDtoStudentGroups)
    {
        foreach (var saveDtoStudentGroup in saveDtoStudentGroups)
        {
            var payload = new LessonValidationPayload
            {
                AffectedByAcademicDisciplineId = saveDtoAcademicDiscipline.Id,
                AffectedByStudentGroupId = saveDtoStudentGroup.Id,
            };
            validationMessages
                .AddErrorIf(saveDtoAcademicDiscipline.SemesterNumber != null
                            && saveDtoStudentGroup.SemesterNumber != null
                            && saveDtoAcademicDiscipline.SemesterNumber != saveDtoStudentGroup.SemesterNumber,
                    payload, LessonValidationCode.MismatchedSemesterNumber);
        }
    }

    public void ValidateAcademicDisciplineTypeMatch(List<LessonValidationMessage> validationMessages,
        AcademicDiscipline saveDtoAcademicDiscipline,
        AcademicDisciplineType lessonAcademicDisciplineType)
    {
        var payload = new LessonValidationPayload
        {
            AffectedByAcademicDisciplineId = saveDtoAcademicDiscipline.Id!,
        };
        validationMessages.AddErrorIf(
            !saveDtoAcademicDiscipline.AllowedLessonTypes.Contains(lessonAcademicDisciplineType),
            payload,
            LessonValidationCode.MismatchedAcademicDisciplineType);
    }

    public void ValidateLessonConflictByGroup(Lesson? lesson,
        Lesson[] conflictingByGroupLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
        Guid[] hierarchyIds,
        bool includeTiming = false)
    {
        foreach (var conflictingByGroupLesson in conflictingByGroupLessons)
        {
            var editedLessonPayload = new LessonValidationPayload
            {
                AffectedByLessonId = conflictingByGroupLesson.Id,
                AffectedByStudentGroupId = conflictingByGroupLesson.StudentGroups
                    .Single(x => hierarchyIds.Contains(x.Id!.Value)).Id!.Value,
                DateWithTimeInterval = includeTiming ? conflictingByGroupLesson.DateWithTimeInterval : null,
            };
            validationMessages
                .AddWarningIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                    editedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByGroup);
            validationMessages
                .AddErrorIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                    editedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByGroup);

            if (lesson == null)
            {
                return;
            }

            if (!affectedLessonNewValidationMessagesByLessonId!.TryGetValue(conflictingByGroupLesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[conflictingByGroupLesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            var existedLessonPayload = new LessonValidationPayload
            {
                AffectedByLessonId = lesson.Id,
                AffectedByStudentGroupId = lesson.StudentGroups
                    .Single(x => hierarchyIds.Contains(x.Id!.Value)).Id!.Value,
            };
            affectedLessonValidationMessages!
                .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                    existedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByGroup);
            affectedLessonValidationMessages!
                .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                    existedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByGroup);
        }
    }

    public void ValidateLessonConflictByTeacher(Guid? lessonId,
        LessonFlexibilityType? lessonFlexibilityType,
        Guid[] teacherIds,
        Lesson[] conflictingByTeacherLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
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
                validationMessages
                    .AddWarningIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonValidationCode.FlexibleLessonTypeConflictByTeacher);
                validationMessages
                    .AddErrorIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonValidationCode.FixedLessonTypeConflictByTeacher);

                if (!lessonId.HasValue)
                {
                    continue;
                }

                if (!affectedLessonNewValidationMessagesByLessonId!.TryGetValue(conflictingByTeacherLesson.Id!.Value,
                        out var affectedLessonValidationMessages))
                {
                    affectedLessonValidationMessages = [];
                    affectedLessonNewValidationMessagesByLessonId[conflictingByTeacherLesson.Id!.Value] =
                        affectedLessonValidationMessages;
                }

                var existedLessonPayload = new LessonValidationPayload { AffectedByLessonId = lessonId };
                affectedLessonValidationMessages!
                    .AddErrorIf(lessonFlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonPayload,
                        LessonValidationCode.FixedLessonTypeConflictByTeacher);
                affectedLessonValidationMessages!
                    .AddWarningIf(lessonFlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonPayload,
                        LessonValidationCode.FlexibleLessonTypeConflictByTeacher);
            }
        }
    }

    public void ValidateTeacherPreferenceConflict(TeacherPreference[] conflictingTeacherPreferences,
        List<LessonValidationMessage> validationMessages,
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
            validationMessages
                .AddWarningIf(
                    conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    payload,
                    LessonValidationCode.UndesirableTimeTeacherPreferenceTypeConflict);
            validationMessages
                .AddErrorIf(
                    conflictingTeacherPreference is { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict);
            validationMessages
                .AddWarningIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    payload,
                    LessonValidationCode.UndesirableRoomTeacherPreferenceTypeConflict);
            validationMessages
                .AddErrorIf(
                    conflictingTeacherPreference is { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict);
        }
    }

    public void ValidateLessonConflictByRoom(Lesson? lesson,
        Guid[] roomIds,
        Lesson[] conflictingByRoomLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?>? affectedLessonNewValidationMessagesByLessonId,
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
                validationMessages
                    .AddWarningIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonValidationCode.FlexibleLessonTypeConflictByRoom);
                validationMessages
                    .AddErrorIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonValidationCode.FixedLessonTypeConflictByRoom);

                if (lesson == null)
                {
                    return;
                }

                if (!affectedLessonNewValidationMessagesByLessonId!.TryGetValue(conflictingByRoomLesson.Id!.Value,
                        out var affectedLessonValidationMessages))
                {
                    affectedLessonValidationMessages = [];
                    affectedLessonNewValidationMessagesByLessonId[conflictingByRoomLesson.Id!.Value] =
                        affectedLessonValidationMessages;
                }

                var existedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = lesson.Id,
                    AffectedByRoomId = room.Id,
                };
                affectedLessonValidationMessages!
                    .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        existedLessonPayload,
                        LessonValidationCode.FixedLessonTypeConflictByRoom);
                affectedLessonValidationMessages!
                    .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        existedLessonPayload,
                        LessonValidationCode.FlexibleLessonTypeConflictByRoom);
            }
        }
    }
}