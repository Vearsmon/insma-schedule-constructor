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
        var violations = await lessonValidationService.ValidateAsync(lesson);
        lesson.Violations = lesson.Violations.Concat(violations.Where(x => x.LessonId == (lesson.Id ?? Guid.Empty))).ToArray();
        var conflictingLessons = await lessonRepository.SelectAsync(violations.Select(x => x.LessonId).ToArray());

        var lessonsWithConflict = UpdateLessonsPolicyViolations(violations.ToList(), conflictingLessons);
        await lessonRepository.SaveAllAsync(lessonsWithConflict.Concat([lesson]).ToArray());
    }

    public async Task RecalculateConflictsForUpdatedAcademicDiscipline(AcademicDiscipline academicDiscipline)
    {
        var lessonPolicyViolations = new List<LessonPolicyViolation>();
        var lessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = academicDiscipline.ScheduleId,
            AcademicDisciplineId = academicDiscipline.Id,
        });
        foreach (var lesson in lessons)
        {
            lessonValidationService.ValidateAcademicDisciplineStudentGroupMatch(lesson, lessonPolicyViolations,
                academicDiscipline, lesson.StudentGroups);
            lessonValidationService.ValidateAcademicDisciplineTypeMatch(lesson, lessonPolicyViolations,
                academicDiscipline, lesson.AcademicDisciplineType!.Value);
        }

        var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations,
            lessons.DistinctBy(x => x.Id!.Value).ToArray());

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
                        Violations = [],
                    }));
            }

            return result.ToArray();
        }
    }

    public async Task RecalculateConflictsForNewTeacherPreferences(TeacherPreference[] preferences)
    {
        var lessonPolicyViolations = new List<LessonPolicyViolation>();
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

            lessonValidationService.ValidateTeacherPreferenceConflict(conflictingLesson, conflictingTeacherPreferences,
                lessonPolicyViolations);
        }

        var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations,
            conflictingByTimeLessons.Concat(conflictingByRoomLessons).DistinctBy(x => x.Id).ToArray());
        await lessonRepository.SaveAllAsync(lessonsWithConflict);
    }

    public async Task RecalculateConflictsForNewStudentGroup(StudentGroup studentGroup)
    {
        var previousStudentGroupLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = studentGroup.ScheduleId,
            StudentGroupIds = [studentGroup.Id!.Value],
        });
        await lessonValidationService.RemovePolicyViolations(
            previousStudentGroupLessons.Select(x => x.Id!.Value).ToArray(),
            [
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup,
                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup
            ]);

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync([studentGroup.Id!.Value]);
        var hierarchyIds = studentGroupHierarchyIdsByStudentGroupId.Values.Single().ToArray();
        var studentGroupHierarchyAttachmentLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = studentGroup.ScheduleId,
            StudentGroupIds = hierarchyIds,
        });

        var lessonPolicyViolations = new List<LessonPolicyViolation>();

        if (studentGroup.SemesterNumber != null)
        {
            foreach (var mismatchedDisciplineLesson in studentGroupHierarchyAttachmentLessons
                         .Where(x => x.AcademicDiscipline != null))
            {
                lessonValidationService.ValidateAcademicDisciplineStudentGroupMatch(mismatchedDisciplineLesson,
                    lessonPolicyViolations, mismatchedDisciplineLesson.AcademicDiscipline!, [studentGroup]);
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
                    .Where(x =>
                        x.Id != newStudentGroupLesson.Id
                        && x.DateWithTimeInterval!.HasIntersection(newStudentGroupLesson.DateWithTimeInterval!))
                    .ToArray();

                lessonValidationService.ValidateLessonConflictByGroup(newStudentGroupLesson,
                    conflictingByGroupLessons, lessonPolicyViolations, hierarchyIds);
            }
        }

        var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations, studentGroupHierarchyAttachmentLessons);
        await lessonRepository.SaveAllAsync(lessonsWithConflict);
    }

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(LessonBatchInfo lessonBatchInfo, Guid scheduleId)
    {
        var lessonPolicyViolations = new List<LessonPolicyViolation>();
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

        lessonValidationService.BuildPolicyViolations(lessonPolicyViolations,
            studentGroupHierarchyIdsByStudentGroupId,
            conflictingLessons, null, teacherIds, roomIds, conflictingTeacherPreferences, includeTiming: true);

        var messages = await lessonValidationService.GetValidationResultMessageAsync(lessonPolicyViolations.ToArray());
        return lessonPolicyViolations.Select((validationMessage, index) => new LessonSeriesConflictDto
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
                Message = messages[index],
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
            var lessonPolicyViolations = await lessonValidationService.ValidateAsync(lesson);
            var conflictingLessons = await lessonRepository.SelectAsync(lessonPolicyViolations.Select(x => x.LessonId).ToArray());

            var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations.ToList(), conflictingLessons);
            await lessonRepository.SaveAllAsync(lessonsWithConflict);
        }
    }

    private Lesson[] UpdateLessonsPolicyViolations(List<LessonPolicyViolation> lessonPolicyViolations, Lesson[] lessonsWithConflict)
    {
        var lessonsWithConflictById = lessonsWithConflict.DistinctBy(x => x.Id).ToDictionary(x => x.Id!.Value);
        var affectedLessonNewViolationsByLessonId = lessonPolicyViolations
            .GroupBy(x => x.LessonId)
            .ToDictionary(x => x.Key);
        foreach (var (lessonId, affectedLessonPolicyViolations) in affectedLessonNewViolationsByLessonId)
        {
            lessonsWithConflictById[lessonId].Violations = lessonsWithConflictById[lessonId].Violations
                .Concat(affectedLessonPolicyViolations).ToArray();
        }

        return lessonsWithConflictById.Select(x => x.Value).ToArray();
    }
}