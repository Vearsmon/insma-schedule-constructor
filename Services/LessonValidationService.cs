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
        var saveDtoStudentGroup = await studentGroupRepository.GetAsync(lesson.StudentGroupId);
        var saveDtoTeacher = lesson.TeacherId.HasValue
            ? await teacherRepository.GetAsync(lesson.TeacherId!.Value)
            : null;
        var saveDtoAcademicDiscipline = lesson.AcademicDisciplineId.HasValue
            ? await academicDisciplineRepository.GetAsync(lesson.AcademicDisciplineId!.Value)
            : null;
        var previousLesson = lesson.Id.HasValue
            ? await lessonRepository.GetAsync(lesson.Id!.Value)
            : null;

        if (!(await scheduleRepository.ExistsAsync(lesson.ScheduleId)))
        {
            validationMessages.Add(new ValidationMessage("Не найден проект расписания для сохранения занятия"));
        }

        if (lesson.RoomId.HasValue && !(await roomRepository.ExistsAsync(lesson.RoomId!.Value)))
        {
            validationMessages.Add(new ValidationMessage("Не найдена аудитория для сохранения занятия"));
        }

        if (previousLesson != null && previousLesson.ScheduleId != lesson.ScheduleId)
        {
            validationMessages.Add(new ValidationMessage("Запрещено менять проект расписания для занятия"));
        }

        if (previousLesson is { CreatedFromDiscipline: true }
            && !CreatedFromDisciplineLessonVersionsAreEqual(previousLesson, lesson))
        {
            validationMessages.Add(new ValidationMessage("Для созданного через дисциплину занятия позволяется изменять только возможность совмещения"));
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
        var lessonsWithConflictById = new Dictionary<Guid, Lesson>();

        if (lesson.AcademicDisciplineId.HasValue)
        {
            ValidateAcademicDisciplineStudentGroupMatch(lessonValidationMessages, saveDtoAcademicDiscipline!,
                saveDtoStudentGroup);
            lessonValidationMessages.AddErrorIf(
                !saveDtoAcademicDiscipline!.AllowedLessonTypes.Contains(lesson.AcademicDisciplineType!.Value),
                new LessonValidationPayload { AffectedByAcademicDisciplineId = saveDtoAcademicDiscipline.Id! },
                LessonValidationCode.MismatchedAcademicDisciplineType);
        }

        if (lesson.DateWithTimeInterval == null)
        {
            return new LessonValidationResult
            {
                Messages = lessonValidationMessages.ToArray(),
                LessonsWithConflictById = lessonsWithConflictById,
            };
        }

        var studentGroupHierarchyIds =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(lesson.StudentGroupId);
        var conflictingByGroupLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = lesson.ScheduleId,
            StudentGroupIds = studentGroupHierarchyIds,
            Date = lesson.DateWithTimeInterval.Date,
            TimeIntervals = [lesson.DateWithTimeInterval.TimeInterval],
            ExcludeAllowCombining = lesson.AllowCombining,
        });
        ValidateLessonConflictByGroup(lesson, conflictingByGroupLessons, lessonValidationMessages,
            affectedLessonNewValidationMessagesByLessonId);

        if (lesson.TeacherId.HasValue)
        {
            var conflictingByTeacherLessons = await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = lesson.ScheduleId,
                TeacherId = lesson.TeacherId!.Value,
                Date = lesson.DateWithTimeInterval.Date,
                TimeIntervals = [lesson.DateWithTimeInterval.TimeInterval],
                ExcludeAllowCombining = lesson.AllowCombining,
            });
            ValidateLessonConflictByTeacher(lesson, conflictingByTeacherLessons, lessonValidationMessages,
                affectedLessonNewValidationMessagesByLessonId);
            await ValidateTeacherPreferenceConflict(lesson, lessonValidationMessages, saveDtoTeacher!);
        }

        var conflictingByRoomLessons = Array.Empty<Lesson>();
        if (lesson.RoomId.HasValue)
        {
            conflictingByRoomLessons = await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = lesson.ScheduleId,
                RoomIds = [lesson.RoomId!.Value],
                Date = lesson.DateWithTimeInterval.Date,
                TimeIntervals = [lesson.DateWithTimeInterval.TimeInterval],
                ExcludeAllowCombining = lesson.AllowCombining,
            });
            ValidateLessonConflictByRoom(lesson, conflictingByRoomLessons, lessonValidationMessages,
                affectedLessonNewValidationMessagesByLessonId);
        }

        lessonsWithConflictById = conflictingByGroupLessons.Concat(conflictingByRoomLessons).DistinctBy(x => x.Id)
            .ToDictionary(x => x.Id!.Value);
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

    public async Task<LessonWeekConflictDto[]> FillValidationMessages(Lesson[] lessons)
    {
        var lessonConflicts = new List<LessonWeekConflictDto>();
        foreach (var lesson in lessons)
        {
            foreach (var validationMessage in lessons.SelectMany(x => x.ValidationMessages))
            {
                var discipline = validationMessage.Payload.AffectedByAcademicDisciplineId.HasValue
                    ? await academicDisciplineRepository.GetAsync(
                        validationMessage.Payload.AffectedByAcademicDisciplineId!.Value)
                    : null;
                var studentGroup = validationMessage.Payload.AffectedByStudentGroupId.HasValue
                    ? await studentGroupRepository.GetAsync(validationMessage.Payload.AffectedByStudentGroupId!.Value)
                    : null;
                var affectedByLesson = validationMessage.Payload.AffectedByLessonId.HasValue
                    ? await lessonRepository.GetAsync(validationMessage.Payload.AffectedByLessonId!.Value)
                    : null;
                var teacher = validationMessage.Payload.AffectedByTeacherId.HasValue
                    ? await teacherRepository.GetAsync(validationMessage.Payload.AffectedByTeacherId!.Value)
                    : null;
                var message = validationMessage.Code switch
                {
                    LessonValidationCode.MismatchedCyphers => string.Format(
                        LessonValidationMessageTemplates.MismatchedCyphersTemplate,
                        discipline!.Name,
                        studentGroup!.Name,
                        studentGroup.Cypher,
                        discipline.Cypher),
                    LessonValidationCode.MismatchedSemesterNumber => string.Format(
                        LessonValidationMessageTemplates.MismatchedSemesterNumberTemplate,
                        discipline!.Name,
                        studentGroup!.Name,
                        studentGroup.SemesterNumber,
                        discipline.SemesterNumber),
                    LessonValidationCode.FixedLessonTypeConflictByGroup => string.Format(
                        LessonValidationMessageTemplates.FixedLessonTypeConflictByGroupTemplate,
                        affectedByLesson!.AcademicDiscipline == null
                            ? string.Empty
                            : affectedByLesson.AcademicDiscipline.Name,
                        affectedByLesson.StudentGroup.Name),
                    LessonValidationCode.FlexibleLessonTypeConflictByGroup => string.Format(
                        LessonValidationMessageTemplates.FlexibleLessonTypeConflictByGroupTemplate,
                        affectedByLesson!.AcademicDiscipline == null
                            ? string.Empty
                            : affectedByLesson.AcademicDiscipline.Name,
                        affectedByLesson.StudentGroup.Name),
                    LessonValidationCode.FixedLessonTypeConflictByTeacher => string.Format(
                        LessonValidationMessageTemplates.FixedLessonTypeConflictByTeacherTemplate,
                        affectedByLesson!.AcademicDiscipline == null
                            ? string.Empty
                            : affectedByLesson.AcademicDiscipline.Name,
                        affectedByLesson.Teacher!.Fullname),
                    LessonValidationCode.FlexibleLessonTypeConflictByTeacher => string.Format(
                        LessonValidationMessageTemplates.FlexibleLessonTypeConflictByTeacherTemplate,
                        affectedByLesson!.AcademicDiscipline == null
                            ? string.Empty
                            : affectedByLesson.AcademicDiscipline.Name,
                        affectedByLesson.Teacher!.Fullname),
                    LessonValidationCode.RestrictedTeacherPreferenceTypeConflict => string.Format(
                        LessonValidationMessageTemplates.RestrictedTeacherPreferenceTypeConflictTemplate,
                        teacher!.Fullname),
                    LessonValidationCode.UndesirableTeacherPreferenceTypeConflict => string.Format(
                        LessonValidationMessageTemplates.UndesirableTeacherPreferenceTypeConflictTemplate,
                        teacher!.Fullname),
                    LessonValidationCode.FixedLessonTypeConflictByRoom => string.Format(
                        LessonValidationMessageTemplates.FixedLessonTypeConflictByRoomTemplate,
                        affectedByLesson!.AcademicDiscipline == null
                            ? string.Empty
                            : affectedByLesson.AcademicDiscipline.Name,
                        affectedByLesson.Room!.Name),
                    LessonValidationCode.FlexibleLessonTypeConflictByRoom => string.Format(
                        LessonValidationMessageTemplates.FlexibleLessonTypeConflictByRoomTemplate,
                        affectedByLesson!.AcademicDiscipline == null
                            ? string.Empty
                            : affectedByLesson.AcademicDiscipline.Name,
                        affectedByLesson.Room!.Name),
                    _ => throw new NotImplementedException(),
                };
                lessonConflicts.Add(new LessonWeekConflictDto
                {
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval(lesson.DateWithTimeInterval!.Date.DayOfWeek,
                        lesson.DateWithTimeInterval.TimeInterval),
                    Message = message,
                });
            }
        }

        return lessonConflicts.ToArray();
    }

    private static bool CreatedFromDisciplineLessonVersionsAreEqual(Lesson previousLesson, Lesson nextLesson)
    {
        return previousLesson.Id == nextLesson.Id
               && previousLesson.ScheduleId == nextLesson.ScheduleId
               && previousLesson.AcademicDisciplineId == nextLesson.AcademicDisciplineId
               && previousLesson.AcademicDisciplineType == nextLesson.AcademicDisciplineType
               && previousLesson.StudentGroupId == nextLesson.StudentGroupId
               && previousLesson.TeacherId == nextLesson.TeacherId
               && previousLesson.RoomId == nextLesson.RoomId
               && (previousLesson.DateWithTimeInterval == null
                   ? nextLesson.DateWithTimeInterval == null
                   : previousLesson.DateWithTimeInterval!.Equals(nextLesson.DateWithTimeInterval))
               && previousLesson.FlexibilityType == nextLesson.FlexibilityType
               && previousLesson.HoursCost == nextLesson.HoursCost
               && previousLesson.CreatedFromDiscipline == nextLesson.CreatedFromDiscipline;
    }

    private static void ValidateAcademicDisciplineStudentGroupMatch(List<LessonValidationMessage> validationMessages,
        AcademicDiscipline saveDtoAcademicDiscipline,
        StudentGroup saveDtoStudentGroup)
    {
        var payload = new LessonValidationPayload
        {
            AffectedByAcademicDisciplineId = saveDtoAcademicDiscipline.Id,
            AffectedByStudentGroupId = saveDtoStudentGroup.Id,
        };
        validationMessages
            .AddErrorIf(saveDtoAcademicDiscipline.Cypher != saveDtoStudentGroup.Cypher,
                payload, LessonValidationCode.MismatchedCyphers);
        validationMessages
            .AddErrorIf(saveDtoAcademicDiscipline.SemesterNumber != saveDtoStudentGroup.SemesterNumber,
                payload, LessonValidationCode.MismatchedSemesterNumber);
    }

    private static void ValidateLessonConflictByGroup(Lesson lesson,
        Lesson[] conflictingByGroupLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?> affectedLessonNewValidationMessagesByLessonId)
    {
        foreach (var conflictingByGroupLesson in conflictingByGroupLessons)
        {
            var editedLessonPayload = new LessonValidationPayload { AffectedByLessonId = conflictingByGroupLesson.Id };
            validationMessages
                .AddWarningIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                    editedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByGroup);
            validationMessages
                .AddErrorIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                    editedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByGroup);

            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingByGroupLesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[conflictingByGroupLesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            var existedLessonPayload = new LessonValidationPayload { AffectedByLessonId = lesson.Id };
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

    private void ValidateLessonConflictByTeacher(Lesson lesson,
        Lesson[] conflictingByTeacherLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?> affectedLessonNewValidationMessagesByLessonId)
    {
        foreach (var conflictingByTeacherLesson in conflictingByTeacherLessons)
        {
            var editedLessonPayload = new LessonValidationPayload
                { AffectedByLessonId = conflictingByTeacherLesson.Id };
            validationMessages
                .AddWarningIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                    editedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByTeacher);
            validationMessages
                .AddErrorIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                    editedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByTeacher);

            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingByTeacherLesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[conflictingByTeacherLesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            var existedLessonPayload = new LessonValidationPayload { AffectedByLessonId = lesson.Id };
            affectedLessonValidationMessages!
                .AddErrorIf(lesson.FlexibilityType == LessonFlexibilityType.Fixed,
                    existedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByTeacher);
            affectedLessonValidationMessages!
                .AddWarningIf(lesson.FlexibilityType == LessonFlexibilityType.Flexible,
                    existedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByTeacher);
        }
    }

    private async Task ValidateTeacherPreferenceConflict(Lesson lesson,
        List<LessonValidationMessage> validationMessages,
        Teacher saveDtoTeacher)
    {
        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchAsync(
            new TeacherPreferenceSearchModel
            {
                ScheduleId = lesson.ScheduleId,
                TeacherId = lesson.TeacherId,
                DaysOfWeek = [lesson.DateWithTimeInterval!.Date.DayOfWeek],
                TimeInterval = lesson.DateWithTimeInterval!.TimeInterval,
                TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable]
            });
        foreach (var conflictingTeacherPreference in conflictingTeacherPreferences)
        {
            var payload = new LessonValidationPayload
            {
                AffectedByTeacherPreferenceId = conflictingTeacherPreference.Id,
                AffectedByTeacherId = saveDtoTeacher.Id,
            };
            validationMessages
                .AddWarningIf(conflictingTeacherPreference.TeacherPreferenceType == TeacherPreferenceType.Undesirable,
                    payload,
                    LessonValidationCode.UndesirableTeacherPreferenceTypeConflict);
            validationMessages
                .AddErrorIf(conflictingTeacherPreference.TeacherPreferenceType == TeacherPreferenceType.Restricted,
                    payload,
                    LessonValidationCode.RestrictedTeacherPreferenceTypeConflict);
        }
    }

    private static void ValidateLessonConflictByRoom(Lesson lesson,
        Lesson[] conflictingByRoomLessons,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?> affectedLessonNewValidationMessagesByLessonId)
    {
        foreach (var conflictingByRoomLesson in conflictingByRoomLessons)
        {
            var editedLessonPayload = new LessonValidationPayload { AffectedByLessonId = conflictingByRoomLesson.Id };
            validationMessages
                .AddWarningIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                    editedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByRoom);
            validationMessages
                .AddErrorIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                    editedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByRoom);

            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingByRoomLesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[conflictingByRoomLesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            var existedLessonPayload = new LessonValidationPayload { AffectedByLessonId = lesson.Id };
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