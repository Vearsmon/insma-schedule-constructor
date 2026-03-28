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
using Domain.Helpers;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.SearchModels;
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
        var saveDtoStudentGroup = await studentGroupRepository.FindAsync(lesson.StudentGroupId);
        var saveDtoTeacher = lesson.TeacherId.HasValue
            ? await teacherRepository.FindAsync(lesson.TeacherId!.Value)
            : null;
        var saveDtoRoom = lesson.RoomId.HasValue
            ? await roomRepository.FindAsync(lesson.RoomId!.Value)
            : null;
        var saveDtoAcademicDiscipline = lesson.AcademicDisciplineId.HasValue
            ? await academicDisciplineRepository.FindAsync(lesson.AcademicDisciplineId!.Value)
            : null;

        if (!(await scheduleRepository.ExistsAsync(lesson.ScheduleId))
            || saveDtoStudentGroup == null
            || (lesson.TeacherId.HasValue && saveDtoTeacher == null)
            || (lesson.RoomId.HasValue && saveDtoRoom == null)
            || (lesson.AcademicDisciplineId.HasValue && saveDtoAcademicDiscipline == null))
        {
            throw new NotImplementedException();
        }

        if (lesson.Id.HasValue)
        {
            var previousLesson = await lessonRepository.GetAsync(lesson.Id!.Value);
            if (previousLesson.ScheduleId != lesson.ScheduleId)
            {
                throw new NotImplementedException();
            }

            var affectedByEditingLessonValidationMessages = await lessonValidationMessageRepository.SearchAsync(
                new LessonValidationMessageSearchModel
                {
                    AffectedByLessonIds = [lesson.Id!.Value],
                });
            await lessonValidationMessageRepository.DeleteAsync(previousLesson.ValidationMessages
                .Select(x => x.Id!.Value)
                .Concat(affectedByEditingLessonValidationMessages.Select(x => x.Id!.Value)).ToArray());
        }

        var validationMessages = new List<LessonValidationMessage>();
        var affectedLessonNewValidationMessagesByLessonId = new Dictionary<Guid, List<LessonValidationMessage>?>();
        var lessonsWithConflictById = new Dictionary<Guid, Lesson>();

        if (lesson.AcademicDisciplineId.HasValue)
        {
            ValidateAcademicDisciplineStudentGroupMatch(validationMessages, saveDtoAcademicDiscipline!, saveDtoStudentGroup);
            validationMessages.AddErrorIf(
                !saveDtoAcademicDiscipline!.AllowedLessonTypes.Contains(lesson.AcademicDisciplineType!.Value),
                new LessonValidationPayload
                {
                    AffectedByAcademicDisciplineId = saveDtoAcademicDiscipline.Id!
                }, LessonValidationCode.MismatchedAcademicDisciplineType);
        }

        if (lesson.DateWithTimeInterval == null)
        {
            return new LessonValidationResult
            {
                Messages = validationMessages.ToArray(),
                LessonsWithConflictById = lessonsWithConflictById,
            };
        }

        var studentGroupHierarchyIds =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(lesson.StudentGroupId);
        var conflictingLessonsByGroup = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = lesson.ScheduleId,
            StudentGroupIds = studentGroupHierarchyIds,
            Date = lesson.DateWithTimeInterval.Date,
            TimeIntervals = [lesson.DateWithTimeInterval.TimeInterval],
        });
        ValidateLessonConflictByGroup(lesson, conflictingLessonsByGroup, validationMessages,
            affectedLessonNewValidationMessagesByLessonId);

        if (lesson.TeacherId.HasValue)
        {
            await ValidateTeacherPreferenceConflict(lesson, validationMessages, saveDtoTeacher!);
        }

        var conflictingLessonsByRoom = Array.Empty<Lesson>();
        if (lesson.RoomId.HasValue)
        {
            conflictingLessonsByRoom = await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = lesson.ScheduleId,
                RoomIds = [lesson.RoomId!.Value],
                Date = lesson.DateWithTimeInterval.Date,
                TimeIntervals = [lesson.DateWithTimeInterval.TimeInterval],
            });
            ValidateLessonConflictByRoom(lesson, conflictingLessonsByRoom, validationMessages,
                affectedLessonNewValidationMessagesByLessonId);
        }

        lessonsWithConflictById = conflictingLessonsByGroup.Concat(conflictingLessonsByRoom).DistinctBy(x => x.Id)
            .ToDictionary(x => x.Id!.Value);
        foreach (var (lessonId, affectedLessonValidationMessages) in affectedLessonNewValidationMessagesByLessonId)
        {
            lessonsWithConflictById[lessonId].ValidationMessages = lessonsWithConflictById[lessonId].ValidationMessages
                .Concat(affectedLessonValidationMessages!).ToArray();
        }

        return new LessonValidationResult
        {
            Messages = validationMessages.ToArray(),
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
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval
                    {
                        DayOfWeek = lesson.DateWithTimeInterval!.Date.DayOfWeek,
                        TimeInterval = lesson.DateWithTimeInterval.TimeInterval,
                    },
                    Message = message,
                });
            }
        }

        return lessonConflicts.ToArray();
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
        Lesson[] conflictingLessonsByGroup,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?> affectedLessonNewValidationMessagesByLessonId)
    {
        foreach (var conflictingLessonByGroup in conflictingLessonsByGroup)
        {
            var editedLessonPayload = new LessonValidationPayload { AffectedByLessonId = conflictingLessonByGroup.Id };
            validationMessages
                .AddWarningIf(conflictingLessonByGroup.FlexibilityType == LessonFlexibilityType.Flexible,
                    editedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByGroup);
            validationMessages
                .AddErrorIf(conflictingLessonByGroup.FlexibilityType == LessonFlexibilityType.Fixed,
                    editedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByGroup);

            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingLessonByGroup.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[conflictingLessonByGroup.Id!.Value] =
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
        Lesson[] conflictingLessonsByRoom,
        List<LessonValidationMessage> validationMessages,
        Dictionary<Guid, List<LessonValidationMessage>?> affectedLessonNewValidationMessagesByLessonId)
    {
        foreach (var conflictingLessonByRoom in conflictingLessonsByRoom)
        {
            var editedLessonPayload = new LessonValidationPayload { AffectedByLessonId = conflictingLessonByRoom.Id };
            validationMessages
                .AddWarningIf(conflictingLessonByRoom.FlexibilityType == LessonFlexibilityType.Flexible,
                    editedLessonPayload,
                    LessonValidationCode.FlexibleLessonTypeConflictByRoom);
            validationMessages
                .AddErrorIf(conflictingLessonByRoom.FlexibilityType == LessonFlexibilityType.Fixed,
                    editedLessonPayload,
                    LessonValidationCode.FixedLessonTypeConflictByRoom);

            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingLessonByRoom.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[conflictingLessonByRoom.Id!.Value] =
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