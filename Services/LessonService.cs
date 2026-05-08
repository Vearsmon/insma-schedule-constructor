using Dal.RegistryRepositories.Lesson;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Mapping;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class LessonService(
    ILessonRepository lessonRepository,
    ILessonRegistryRepository lessonRegistryRepository,
    ILessonValidationService lessonValidationService,
    IAcademicDisciplineRepository academicDisciplineRepository,
    IStudentGroupRepository studentGroupRepository,
    IScheduleRepository scheduleRepository,
    ITeacherRepository teacherRepository,
    IRoomRepository roomRepository,
    ITeacherPreferenceRepository teacherPreferenceRepository) : ILessonService
{
    public async Task<LessonShortDto[]> SearchWeekAsync(Guid scheduleId, DateOnly dateFrom, DateOnly dateTo)
    {
        var lessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            DateFrom = dateFrom,
            DateTo = dateTo,
        });
        return lessons.Select(LessonDtoMappingRegister.MapModelToShortDto).ToArray()!;
    }

    public async Task<RegistryDto<LessonRegistryItemDto>> SearchAsync(LessonRegistrySearchModel searchModel)
    {
        var registryEntries =
            await lessonRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<LessonRegistryItemDto>
        {
            Items = registryEntries.Items.Select(LessonDtoMappingRegister.MapItemToItemDto).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<LessonViewDto> GetViewAsync(Guid lessonId)
    {
        var lesson = await lessonRepository.GetAsync(lessonId);
        return LessonDtoMappingRegister.MapModelToViewDto(lesson)!;
    }

    public async Task SaveAsync(LessonSaveDto lessonSaveDto)
    {
        var lesson = LessonDtoMappingRegister.MapSaveDtoToModel(lessonSaveDto)!;
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
        foreach (var lesson in lessons)
        {
            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(lesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[lesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            lessonValidationService.ValidateAcademicDisciplineStudentGroupMatch(affectedLessonValidationMessages!,
                academicDiscipline, lesson.StudentGroups);
            lessonValidationService.ValidateAcademicDisciplineTypeMatch(affectedLessonValidationMessages!,
                academicDiscipline, lesson.AcademicDisciplineType!.Value);
        }

        var lessonsWithConflictById = lessons
            .Where(x => affectedLessonNewValidationMessagesByLessonId.TryGetValue(x.Id!.Value, out var messages) && messages!.Count > 0)
            .ToDictionary(x => x.Id!.Value);
        foreach (var (lessonId, affectedLessonValidationMessages) in affectedLessonNewValidationMessagesByLessonId.Where(x => x.Value!.Count > 0))
        {
            lessonsWithConflictById[lessonId].ValidationMessages = lessonsWithConflictById[lessonId].ValidationMessages
                .Concat(affectedLessonValidationMessages!).ToArray();
        }

        var lessonsWithConflict = lessonsWithConflictById.Select(x => x.Value).ToArray();
        foreach (var lesson in lessonsWithConflict)
        {
            lesson.AcademicDiscipline = null;
        }
        await lessonRepository.SaveAllAsync(lessonsWithConflict);
    }

    public async Task UpdateAcademicDisciplineLessons(AcademicDiscipline academicDiscipline)
    {
        var previousAcademicDisciplineVersion = academicDiscipline.Id.HasValue
            ? await academicDisciplineRepository.GetAsync(academicDiscipline.Id!.Value)
            : null;
        var lessonTypesToDelete = previousAcademicDisciplineVersion?.AllowedLessonTypes
            .Except(academicDiscipline.AllowedLessonTypes)
            .ToArray() ?? [];
        var lessonTypesToAdd = academicDiscipline.AllowedLessonTypes
            .Except(previousAcademicDisciplineVersion?.AllowedLessonTypes ?? []);
        var lessonTypesToUpdate = previousAcademicDisciplineVersion?.AllowedLessonTypes
            .Intersect(academicDiscipline.AllowedLessonTypes)
            .ToArray() ?? [];

        if (academicDiscipline.Id.HasValue && (lessonTypesToDelete.Length > 0 || lessonTypesToUpdate.Length > 0))
        {
            var lessonsToDelete = await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = academicDiscipline.ScheduleId,
                AcademicDisciplineId = academicDiscipline.Id,
                Types = lessonTypesToDelete.Concat(lessonTypesToUpdate).Distinct().ToArray(),
            });

            await lessonRepository.DeleteAsync(lessonsToDelete.Select(x => x.Id!.Value).ToArray());
        }

        var lessonsToSave = new List<Lesson>();
        var schedule = await scheduleRepository.GetAsync(academicDiscipline.ScheduleId);
        foreach (var lessonTypeToSave in lessonTypesToAdd.Concat(lessonTypesToUpdate).Distinct().ToArray())
        {
            var payload = academicDiscipline.GetPayloadByType(lessonTypeToSave);
            if (payload != null && (payload.LessonBatchInfos.Length > 0 || payload.TotalHoursCount != 0))
            {
                lessonsToSave.AddRange(await GetBatchLessonsToAdd(payload.LessonBatchInfos, lessonTypeToSave));
            }
        }

        await SaveLessonBatchAsync(lessonsToSave.ToArray());

        return;

        async Task<Lesson[]> GetBatchLessonsToAdd(LessonBatchInfo[] lessonBatchInfos, AcademicDisciplineType type)
        {
            var result = new List<Lesson>();
            foreach (var lessonBatchInfo in lessonBatchInfos)
            {
                var groups = await studentGroupRepository.SelectAsync(lessonBatchInfo.StudentGroups.Select(x => x.Id!.Value).ToArray());
                var rootGroups = groups
                    .Where(l => groups
                        .Where(x => x.Id != l.Id)
                        .All(r => l.Parents
                            .All(x => x.Id!.Value != r.Id!.Value)))
                    .ToArray();
                var teachers = await teacherRepository.SelectAsync(lessonBatchInfo.Teachers.Select(x => x.Id!.Value).ToArray());
                var rooms = await roomRepository.SelectAsync(lessonBatchInfo.Rooms.Select(x => x.Id!.Value).ToArray());
                var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                    lessonBatchInfo.DateInterval,
                    lessonBatchInfo.DayOfWeekTimeIntervals.Select(x => x.DayOfWeek).ToArray(),
                    lessonBatchInfo.RepeatType,
                    schedule.DateInterval);
                var timeIntervalsByDayOfWeek =
                    lessonBatchInfo.DayOfWeekTimeIntervals.ToDictionary(x => x.DayOfWeek);
                result.AddRange(dates
                    .Select(date => new Lesson
                    {
                        ScheduleId = academicDiscipline.ScheduleId,
                        AcademicDisciplineId = academicDiscipline.Id,
                        AcademicDisciplineType = type,
                        StudentGroups = rootGroups,
                        Teachers = teachers,
                        Rooms = rooms,
                        DateWithTimeInterval = new DateWithTimeInterval
                        {
                            Date = date,
                            TimeInterval = timeIntervalsByDayOfWeek[date.DayOfWeek].TimeInterval,
                        },
                        FlexibilityType = LessonFlexibilityType.Fixed,
                        AllowCombining = lessonBatchInfo.AllowCombining,
                        HoursCost = lessonBatchInfo.HoursCost,
                        ValidationMessages = [],
                    }));
            }

            return result.ToArray();
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
            TeacherIds = [teacherId],
            DayOfWeekTimeIntervals = timeBoundPreferences
                .Select(x => x.DayOfWeekTimeInterval!)
                .ToArray(),
        });

        var roomBoundPreferences = preferences.Where(x => x.RoomId.HasValue).ToArray();
        var conflictingByRoomLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            TeacherIds = [teacherId],
            RoomIds = roomBoundPreferences.Select(x => x.RoomId!.Value).ToArray(),
        });

        foreach (var conflictingLesson in conflictingByTimeLessons.Concat(conflictingByRoomLessons)
                     .DistinctBy(x => x.Id!.Value))
        {
            var conflictingTimeTeacherPreferences = timeBoundPreferences.Where(preference =>
                preference.DayOfWeekTimeInterval!.HasIntersection(conflictingLesson.DateWithTimeInterval));
            var conflictingRoomTeacherPreferences = roomBoundPreferences.Where(preference =>
                conflictingLesson.Rooms.Any(x => x.Id == preference.RoomId));
            var conflictingTeacherPreferences =
                conflictingTimeTeacherPreferences.Concat(conflictingRoomTeacherPreferences).ToArray();

            if (conflictingTeacherPreferences.Length == 0)
            {
                continue;
            }

            if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(conflictingLesson.Id!.Value,
                    out var affectedLessonValidationMessages))
            {
                affectedLessonValidationMessages = [];
                affectedLessonNewValidationMessagesByLessonId[conflictingLesson.Id!.Value] =
                    affectedLessonValidationMessages;
            }

            lessonValidationService.ValidateTeacherPreferenceConflict(conflictingTeacherPreferences, affectedLessonValidationMessages!);
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
            [
                LessonValidationCode.FlexibleLessonTypeConflictByGroup,
                LessonValidationCode.FixedLessonTypeConflictByGroup
            ]);

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync([studentGroup.Id!.Value]);
        var hierarchyIds = studentGroupHierarchyIdsByStudentGroupId.Values.First().ToArray();
        var studentGroupHierarchyAttachmentLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = studentGroup.ScheduleId,
            StudentGroupIds = hierarchyIds,
        });

        var affectedLessonNewValidationMessagesByLessonId = new Dictionary<Guid, List<LessonValidationMessage>?>();

        if (studentGroup.SemesterNumber != null)
        {
            foreach (var mismatchedDisciplineLesson in studentGroupHierarchyAttachmentLessons.Where(x =>
                         x.AcademicDiscipline is { SemesterNumber: not null }
                         && x.AcademicDiscipline.SemesterNumber != studentGroup.SemesterNumber))
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

                affectedLessonValidationMessages!.AddError(payload, LessonValidationCode.MismatchedSemesterNumber);
            }
        }

        // для новой иерархии пересчитаем, есть ли пересечения занятий по группе в иерархии
        var lessonsGroupedByDate = studentGroupHierarchyAttachmentLessons
            .Where(x => x.DateWithTimeInterval != null)
            .GroupBy(x => x.DateWithTimeInterval!.Date);
        foreach (var lessonsGroup in lessonsGroupedByDate)
        {
            var lessonsOnDate = lessonsGroup.ToArray();
            foreach (var newStudentGroupLesson in lessonsOnDate
                         .Where(x => x.StudentGroups.Any(y => y.Id == studentGroup.Id)))
            {
                var conflictingByGroupLessons = lessonsOnDate
                    .Where(x => x.Id != newStudentGroupLesson.Id
                                && x.DateWithTimeInterval!.HasIntersection(newStudentGroupLesson
                                    .DateWithTimeInterval!))
                    .ToArray();

                if (conflictingByGroupLessons.Length == 0)
                {
                    continue;
                }

                if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(newStudentGroupLesson.Id!.Value,
                        out var lessonValidationMessages))
                {
                    lessonValidationMessages = [];
                    affectedLessonNewValidationMessagesByLessonId[newStudentGroupLesson.Id!.Value] =
                        lessonValidationMessages;
                }

                lessonValidationService.ValidateLessonConflictByGroup(newStudentGroupLesson, conflictingByGroupLessons,
                    lessonValidationMessages!, affectedLessonNewValidationMessagesByLessonId, hierarchyIds);
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

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(LessonBatchInfo lessonBatchInfo, Guid scheduleId)
    {
        var validationMessages = new List<LessonValidationMessage>();
        var studentGroupIds = lessonBatchInfo.StudentGroups.Select(x => x.Id!.Value).ToArray();
        var teacherIds = lessonBatchInfo.Teachers.Select(x => x.Id!.Value).ToArray();
        var roomIds = lessonBatchInfo.Rooms.Select(x => x.Id!.Value).ToArray();

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(studentGroupIds);
        var hierarchyIdsFlat = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray();

        var conflictingLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            StudentGroupIds = hierarchyIdsFlat,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DateFrom = lessonBatchInfo.DateInterval.DateFrom,
            DateTo = lessonBatchInfo.DateInterval.DateTo,
            ExcludeAllowCombining = true,
            SearchForConflicts = true,
        });

        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = scheduleId,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
        });

        lessonValidationService.BuildValidationMessages(validationMessages,
            studentGroupHierarchyIdsByStudentGroupId,
            conflictingLessons, null, teacherIds, roomIds, conflictingTeacherPreferences, null, includeTiming: true);

        var messages = await lessonValidationService.GetValidationResultMessageAsync(validationMessages.ToArray());
        return validationMessages.Select((validationMessage, i) => new LessonSeriesConflictDto
        {
            DayOfWeekTimeInterval = validationMessage.Payload.DateWithTimeInterval != null
                ? new DayOfWeekTimeInterval
                {
                    DayOfWeek = validationMessage.Payload.DateWithTimeInterval!.Date.DayOfWeek,
                    TimeInterval = validationMessage.Payload.DateWithTimeInterval.TimeInterval,
                }
                : validationMessage.Payload.DayOfWeekTimeInterval!,
            Messages = [new LessonSeriesConflictMessageDto
            {
                TimeInterval = validationMessage.Payload.DateWithTimeInterval?.TimeInterval
                    ?? validationMessage.Payload.DayOfWeekTimeInterval!.TimeInterval,
                Message = messages[i],
            }],
            ErrorType = validationMessage.ErrorType,
        }).ToArray();
    }

    public async Task DeleteAsync(Guid scheduleId, Guid lessonId)
    {
        var lesson = await lessonRepository.GetAsync(lessonId);
        if (lesson.ScheduleId != scheduleId)
        {
            throw new ServiceException(new ValidationMessage("Не найден проект расписания для удаления занятия"));
        }

        await lessonRepository.DeleteAsync(lessonId);
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