using Dal.RegistryRepositories.Lesson;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Domain.Constants;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Helpers;
using Domain.Mapping;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class LessonService(
    ILessonRepository lessonRepository,
    ILessonRegistryRepository lessonRegistryRepository,
    ILessonValidationService lessonValidationService,
    IAcademicDisciplineRepository academicDisciplineRepository,
    IStudentGroupRepository studentGroupRepository,
    ITeacherPreferenceRepository teacherPreferenceRepository) : ILessonService
{
    public async Task<RegistryDto<LessonRegistryItemDto>> SearchAsync(LessonRegistrySearchModel searchModel)
    {
        var registryEntries =
            await lessonRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<LessonRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<LessonViewDto> GetViewAsync(Guid lessonId)
    {
        var lesson = await lessonRepository.GetAsync(lessonId);
        return DtoMappingRegister.Map(lesson)!;
    }

    public async Task SaveAsync(SaveLessonDto saveLessonDto)
    {
        var lesson = DtoMappingRegister.Map(saveLessonDto)!;
        var validationResult = await lessonValidationService.ValidateAsync(lesson);
        lesson.ValidationMessages = validationResult.Messages.ToArray();

        await lessonRepository.SaveAllAsync(validationResult.LessonsWithConflictById
            .Select(x => x.Value)
            .Concat([lesson])
            .ToArray());
    }

    public async Task RecalculateConflictsForUpdatedAcademicDiscipline(AcademicDiscipline academicDiscipline)
    {
        var affectedLessonNewValidationMessagesByLessonId = new Dictionary<Guid, List<LessonValidationMessage>?>();
        var lessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = academicDiscipline.ScheduleId,
            AcademicDisciplineId = academicDiscipline.Id,
        });
        foreach (var lesson in lessons
                     .Where(x => x.StudentGroup.Cypher != academicDiscipline.Cypher
                                 || x.StudentGroup.SemesterNumber != academicDiscipline.SemesterNumber
                                 || !academicDiscipline.AllowedLessonTypes.Contains(x.AcademicDisciplineType!.Value)))
        {
            var payload = new LessonValidationPayload
            {
                AffectedByAcademicDisciplineId = academicDiscipline.Id,
                AffectedByStudentGroupId = lesson.StudentGroupId,
            };
            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(lesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[lesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            affectedLessonValidationMessages!
                .AddErrorIf(lesson.StudentGroup.Cypher != academicDiscipline.Cypher,
                    payload, LessonValidationCode.MismatchedCyphers);
            affectedLessonValidationMessages!
                .AddErrorIf(lesson.StudentGroup.SemesterNumber != academicDiscipline.SemesterNumber,
                    payload, LessonValidationCode.MismatchedSemesterNumber);
            affectedLessonValidationMessages!
                .AddErrorIf(
                    !academicDiscipline.AllowedLessonTypes.Contains(lesson.AcademicDisciplineType!.Value),
                    new LessonValidationPayload { AffectedByAcademicDisciplineId = academicDiscipline.Id! },
                    LessonValidationCode.MismatchedAcademicDisciplineType);
        }

        var lessonsWithConflictById = lessons
            .Where(x => affectedLessonNewValidationMessagesByLessonId.ContainsKey(x.Id!.Value))
            .ToDictionary(x => x.Id!.Value);
        foreach (var (lessonId, affectedLessonValidationMessages) in affectedLessonNewValidationMessagesByLessonId)
        {
            lessonsWithConflictById[lessonId].ValidationMessages = lessonsWithConflictById[lessonId].ValidationMessages
                .Concat(affectedLessonValidationMessages!).ToArray();
        }

        await lessonRepository.SaveAllAsync(lessonsWithConflictById.Select(x => x.Value).ToArray());
    }

    public async Task UpdateAcademicDisciplineLessons(AcademicDiscipline academicDiscipline)
    {
        var previousAcademicDisciplineVersion = academicDiscipline.Id.HasValue
            ? await academicDisciplineRepository.GetAsync(academicDiscipline.Id!.Value)
            : null;
        var lessonTypesToDelete = previousAcademicDisciplineVersion != null
            ? previousAcademicDisciplineVersion.AllowedLessonTypes
                .Except(academicDiscipline.AllowedLessonTypes)
                .ToArray()
            : [];
        var lessonTypesToAdd = academicDiscipline.AllowedLessonTypes
            .Except(previousAcademicDisciplineVersion?.AllowedLessonTypes ?? []);
        var lessonTypesToUpdate = previousAcademicDisciplineVersion != null
            ? previousAcademicDisciplineVersion.AllowedLessonTypes
                .Intersect(academicDiscipline.AllowedLessonTypes)
                .ToArray()
            : [];

        if (academicDiscipline.Id.HasValue)
        {
            var lessonsToDelete = await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = academicDiscipline.ScheduleId,
                AcademicDisciplineId = academicDiscipline.Id,
                Types = lessonTypesToDelete.Concat(lessonTypesToUpdate).Distinct().ToArray(),
                CreatedFromDiscipline = true,
            });

            await lessonRepository.DeleteAsync(lessonsToDelete.Select(x => x.Id!.Value).ToArray());
        }

        var lessonsToSave = new List<Lesson>();
        foreach (var lessonTypeToSave in lessonTypesToAdd.Concat(lessonTypesToUpdate).Distinct().ToArray())
        {
            switch (lessonTypeToSave)
            {
                case AcademicDisciplineType.Lecture:
                    lessonsToSave.AddRange(GetBatchLessonsToAdd(academicDiscipline.LecturePayload!.LessonBatchInfo!,
                        AcademicDisciplineType.Lecture));
                    break;
                case AcademicDisciplineType.Lab:
                    lessonsToSave.AddRange(GetBatchLessonsToAdd(academicDiscipline.LabPayload!.LessonBatchInfo!,
                        AcademicDisciplineType.Lab));
                    break;
                case AcademicDisciplineType.Practice:
                    lessonsToSave.AddRange(GetBatchLessonsToAdd(academicDiscipline.PracticePayload!.LessonBatchInfo!,
                        AcademicDisciplineType.Practice));
                    break;
            }
        }

        await SaveLessonBatchAsync(lessonsToSave.ToArray());

        return;

        Lesson[] GetBatchLessonsToAdd(AcademicDisciplineLessonBatchInfo lessonBatchInfo, AcademicDisciplineType type)
        {
            var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                lessonBatchInfo.DateInterval,
                lessonBatchInfo.DayOfWeekTimeIntervals.Select(x => x.DayOfWeek).ToArray(),
                lessonBatchInfo.RepeatType);
            var timeIntervalsByDayOfWeek =
                lessonBatchInfo.DayOfWeekTimeIntervals.ToDictionary(x => x.DayOfWeek);
            return dates
                .Select(date => new Lesson
                {
                    ScheduleId = academicDiscipline.ScheduleId,
                    AcademicDisciplineId = academicDiscipline.Id,
                    AcademicDisciplineType = type,
                    StudentGroupId = lessonBatchInfo.StudentGroupId,
                    TeacherId = lessonBatchInfo.TeacherId,
                    RoomId = lessonBatchInfo.RoomId,
                    DateWithTimeInterval = new DateWithTimeInterval(date,
                        timeIntervalsByDayOfWeek[date.DayOfWeek].TimeInterval),
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AllowCombining = lessonBatchInfo.AllowCombining,
                    HoursCost = lessonBatchInfo.HoursCost,
                    CreatedFromDiscipline = true,
                    ValidationMessages = [],
                })
                .ToArray();
        }
    }

    public async Task RecalculateConflictsForNewTeacherPreferences(TeacherPreference[] preferences)
    {
        var affectedLessonNewValidationMessagesByLessonId = new Dictionary<Guid, List<LessonValidationMessage>?>();
        var scheduleId = preferences.First().ScheduleId;
        var teacherId = preferences.First().TeacherId;

        var timeBoundPreferences = preferences.Where(x => x.DayOfWeekTimeInterval != null).ToArray();
        var conflictingByTimeLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            TeacherId = teacherId,
            DayOfWeekTimeIntervals = timeBoundPreferences
                .Select(x => x.DayOfWeekTimeInterval!)
                .ToArray(),
        });

        var roomBoundPreferences = preferences.Where(x => x.RoomId.HasValue).ToArray();
        var conflictingByRoomLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            TeacherId = teacherId,
            RoomIds = roomBoundPreferences.Select(x => x.RoomId!.Value).ToArray(),
        });

        foreach (var conflictingLesson in conflictingByTimeLessons.Concat(conflictingByRoomLessons)
                     .DistinctBy(x => x.Id!.Value))
        {
            var conflictingTimeTeacherPreferences = timeBoundPreferences.Where(preference =>
                preference.DayOfWeekTimeInterval!.HasIntersection(conflictingLesson.DateWithTimeInterval));
            var conflictingRoomTeacherPreferences = roomBoundPreferences.Where(preference =>
                conflictingLesson.RoomId == preference.RoomId);
            foreach (var conflictingTeacherPreference in conflictingTimeTeacherPreferences.Concat(
                         conflictingRoomTeacherPreferences))
            {
                var payload = new LessonValidationPayload
                {
                    AffectedByTeacherPreferenceId = conflictingTeacherPreference.Id,
                    AffectedByTeacherId = teacherId,
                };
                if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingLesson.Id!.Value,
                        out var affectedLessonValidationMessages))
                {
                    affectedLessonValidationMessages = [];
                    affectedLessonNewValidationMessagesByLessonId[conflictingLesson.Id!.Value] =
                        affectedLessonValidationMessages;
                }

                affectedLessonValidationMessages!
                    .AddWarningIf(
                        conflictingTeacherPreference is
                        {
                            DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable
                        },
                        payload,
                        LessonValidationCode.UndesirableTimeTeacherPreferenceTypeConflict);
                affectedLessonValidationMessages!
                    .AddErrorIf(
                        conflictingTeacherPreference is
                        {
                            DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted
                        },
                        payload,
                        LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict);
                affectedLessonValidationMessages!
                    .AddWarningIf(
                        conflictingTeacherPreference is
                            { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                        payload,
                        LessonValidationCode.UndesirableRoomTeacherPreferenceTypeConflict);
                affectedLessonValidationMessages!
                    .AddErrorIf(
                        conflictingTeacherPreference is
                            { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                        payload,
                        LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict);
            }
        }

        var lessonsWithConflictById = conflictingByTimeLessons.Concat(conflictingByRoomLessons).DistinctBy(x => x.Id)
            .ToDictionary(x => x.Id!.Value);
        foreach (var (lessonId, affectedLessonValidationMessages) in affectedLessonNewValidationMessagesByLessonId)
        {
            lessonsWithConflictById[lessonId].ValidationMessages = lessonsWithConflictById[lessonId].ValidationMessages
                .Concat(affectedLessonValidationMessages!).ToArray();
        }

        await lessonRepository.SaveAllAsync(lessonsWithConflictById.Select(x => x.Value).ToArray());
    }

    public async Task RecalculateConflictsForNewStudentGroup(StudentGroup studentGroup)
    {
        var previousStudentGroupLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = studentGroup.ScheduleId,
            StudentGroupIds = [studentGroup.Id!.Value],
        });
        await lessonValidationService.RemoveValidationMessages(
            previousStudentGroupLessons.Select(x => x.Id!.Value).ToArray(),
            [LessonValidationCode.FlexibleLessonTypeConflictByGroup, LessonValidationCode.FixedLessonTypeConflictByGroup]);

        var studentGroupHierarchyIds = await studentGroupRepository.GetStudentGroupTreeIdsAsync(studentGroup.Id!.Value);
        var studentGroupHierarchyAttachmentLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = studentGroup.ScheduleId,
            StudentGroupIds = studentGroupHierarchyIds,
        });

        var affectedLessonNewValidationMessagesByLessonId = new Dictionary<Guid, List<LessonValidationMessage>?>();

        foreach (var mismatchedDisciplineLesson in studentGroupHierarchyAttachmentLessons.Where(x =>
                     x.AcademicDiscipline != null
                     && (x.AcademicDiscipline.Cypher != studentGroup.Cypher ||
                         x.AcademicDiscipline.SemesterNumber != studentGroup.SemesterNumber)))
        {
            var payload = new LessonValidationPayload
            {
                AffectedByAcademicDisciplineId = mismatchedDisciplineLesson.AcademicDiscipline!.Id,
                AffectedByStudentGroupId = studentGroup.Id,
            };
            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(mismatchedDisciplineLesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[mismatchedDisciplineLesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            affectedLessonValidationMessages!
                .AddErrorIf(mismatchedDisciplineLesson.AcademicDiscipline.Cypher != studentGroup.Cypher,
                    payload, LessonValidationCode.MismatchedCyphers);
            affectedLessonValidationMessages!
                .AddErrorIf(mismatchedDisciplineLesson.AcademicDiscipline.SemesterNumber != studentGroup.SemesterNumber,
                    payload, LessonValidationCode.MismatchedSemesterNumber);
        }

        // для новой иерархии пересчитаем, есть ли пересечения занятий по группе в иерархии
        var timeAttachmentLessons = studentGroupHierarchyAttachmentLessons
            .Where(x => x.DateWithTimeInterval != null);
        var lessonsGroupedByDate = timeAttachmentLessons
            .GroupBy(x => x.DateWithTimeInterval!.Date);
        foreach (var lessonsGroup in lessonsGroupedByDate)
        {
            var lessonsOnDate = lessonsGroup.ToArray();
            foreach (var newStudentGroupLesson in lessonsOnDate.Where(x => x.StudentGroupId == studentGroup.Id))
            {
                foreach (var conflictingLesson in lessonsOnDate
                             .Where(x => x.Id != newStudentGroupLesson.Id
                                         && x.DateWithTimeInterval!.HasIntersection(newStudentGroupLesson.DateWithTimeInterval!)))
                {
                    var editedLessonPayload = new LessonValidationPayload { AffectedByLessonId = conflictingLesson.Id };
                    if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(newStudentGroupLesson.Id!.Value,
                            out var lessonValidationMessages))
                    {
                        lessonValidationMessages = [];
                        affectedLessonNewValidationMessagesByLessonId[newStudentGroupLesson.Id!.Value] = lessonValidationMessages;
                    }

                    lessonValidationMessages!
                        .AddWarningIf(conflictingLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                            editedLessonPayload,
                            LessonValidationCode.FlexibleLessonTypeConflictByGroup);
                    lessonValidationMessages!
                        .AddErrorIf(conflictingLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                            editedLessonPayload,
                            LessonValidationCode.FixedLessonTypeConflictByGroup);

                    var existedLessonPayload = new LessonValidationPayload { AffectedByLessonId = newStudentGroupLesson.Id };
                    if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingLesson.Id!.Value,
                            out var affectedLessonValidationMessages))
                    {
                        affectedLessonValidationMessages = [];
                        affectedLessonNewValidationMessagesByLessonId[conflictingLesson.Id!.Value] =
                            affectedLessonValidationMessages;
                    }

                    affectedLessonValidationMessages!
                        .AddErrorIf(newStudentGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                            existedLessonPayload,
                            LessonValidationCode.FixedLessonTypeConflictByGroup);
                    affectedLessonValidationMessages!
                        .AddWarningIf(newStudentGroupLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                            existedLessonPayload,
                            LessonValidationCode.FlexibleLessonTypeConflictByGroup);
                }
            }
        }

        var lessonsWithConflictById =
            studentGroupHierarchyAttachmentLessons.DistinctBy(x => x.Id).ToDictionary(x => x.Id!.Value);
        foreach (var (lessonId, affectedLessonValidationMessages) in affectedLessonNewValidationMessagesByLessonId)
        {
            lessonsWithConflictById[lessonId].ValidationMessages = lessonsWithConflictById[lessonId].ValidationMessages
                .Concat(affectedLessonValidationMessages!).ToArray();
        }

        await lessonRepository.SaveAllAsync(lessonsWithConflictById.Select(x => x.Value).ToArray());
    }

    public async Task<LessonWeekConflictDto[]> GetLessonWeekConflictsAsync(Guid lessonId, DateInterval dateInterval)
    {
        var lesson = await lessonRepository.GetAsync(lessonId);
        var studentGroupHierarchyIds =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(lesson.StudentGroupId);
        var conflictingByGroupLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = lesson.ScheduleId,
            StudentGroupIds = studentGroupHierarchyIds,
            DateFrom = dateInterval.DateFrom,
            DateTo = dateInterval.DateTo,
        });

        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchAsync(
            new TeacherPreferenceSearchModel
            {
                ScheduleId = lesson.ScheduleId,
                TeacherId = lesson.TeacherId,
                DaysOfWeek = dateInterval.ToDateSequence().Select(x => x.DayOfWeek).Distinct().ToArray(),
                TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable]
            });

        var conflictingByRoomLessons = lesson.RoomId.HasValue
            ? await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = lesson.ScheduleId,
                RoomIds = [lesson.RoomId!.Value],
                DateFrom = dateInterval.DateFrom,
                DateTo = dateInterval.DateTo,
            })
            : [];

        var validationMessages = await lessonValidationService.FillValidationMessages(conflictingByGroupLessons
            .Concat(conflictingByRoomLessons).DistinctBy(x => x.Id).ToArray());
        validationMessages = validationMessages.Concat(conflictingTeacherPreferences
                .Select(x => new LessonWeekConflictDto
                {
                    DayOfWeekTimeInterval = x.DayOfWeekTimeInterval!,
                    Message = x.TeacherPreferenceType switch
                    {
                        TeacherPreferenceType.Undesirable => string.Format(
                            LessonValidationMessageTemplates.UndesirableTimeTeacherPreferenceTypeConflictTemplate,
                            x.Teacher.Fullname),
                        TeacherPreferenceType.Restricted => string.Format(
                            LessonValidationMessageTemplates.RestrictedTimeTeacherPreferenceTypeConflictTemplate,
                            x.Teacher.Fullname),
                        TeacherPreferenceType.Preferred => throw new NotImplementedException(),
                        null => throw new NotImplementedException(),
                        _ => throw new NotImplementedException(),
                    }
                }))
            .ToArray();
        return validationMessages;
    }

    private async Task SaveLessonBatchAsync(Lesson[] lessons)
    {
        foreach (var lesson in lessons)
        {
            var validationResult = await lessonValidationService.ValidateAsync(lesson);
            lesson.ValidationMessages = validationResult.Messages.ToArray();

            await lessonRepository.SaveAllAsync(validationResult.LessonsWithConflictById.Select(x => x.Value)
                .Concat([lesson]).ToArray());
        }
    }
}