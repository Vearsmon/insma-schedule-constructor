using Dal.RegistryRepositories.Lesson;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Constants;
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
        var messages = await lessonValidationService.FillValidationMessages(
            lessons.Where(x => x.Violations.Length == 1).ToArray());
        return lessons.Select(x =>
        {
            var shortDto = LessonDtoMappingRegister.MapModelToShortDto(x);
            shortDto!.LessonPolicyViolationDescription = x.Violations.Length switch
            {
                0 => null,
                1 => messages.Single(y => y.LessonIds.Contains(x.Id!.Value)).Messages.Single().Message,
                _ => string.Format(LessonPolicyViolationTemplates.LessonPolicyViolationDefaultTemplate, x.Violations.Length)
            };
            return shortDto;
        }).ToArray();
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
        Lesson lesson;
        var id = lessonSaveDto.Id;
        if (id.HasValue)
        {
            lesson = await lessonRepository.GetAsync(id.Value);
            LessonDtoMappingRegister.UpdateModelWithSaveDto(lessonSaveDto, lesson);
            lesson.Violations = [];
            await lessonRepository.SaveAsync(lesson);
        }
        else
        {
            lesson = LessonDtoMappingRegister.MapSaveDtoToModel(lessonSaveDto)!;
            id = await lessonRepository.SaveAsync(lesson);
        }

        lesson.Id ??= id;
        var violations = await lessonValidationService.ValidateAsync([lesson]);
        var lessonToSaveViolations = violations
            .Where(x => x.LessonId == (lesson.Id ?? Guid.Empty))
            .ToArray();
        var otherLessonsViolations = violations
            .Where(x => x.LessonId != (lesson.Id ?? Guid.Empty))
            .ToArray();
        lesson.Violations = lesson.Violations.Concat(lessonToSaveViolations).ToArray();
        var conflictingLessons = await lessonRepository.SelectAsync(otherLessonsViolations
            .Select(x => x.LessonId)
            .Distinct()
            .ToArray());

        foreach (var conflictingLesson in conflictingLessons)
        {
            conflictingLesson.Violations = [];
        }
        var lessonsWithConflict = UpdateLessonsPolicyViolations(otherLessonsViolations, conflictingLessons);
        await lessonValidationService.DeleteViolationLinksAsync(lessonsWithConflict.SelectMany(x => x.Violations)
            .Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToArray());
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

        var conflictingLessons = lessons.DistinctBy(x => x.Id!.Value).ToArray();
        foreach (var conflictingLesson in conflictingLessons)
        {
            conflictingLesson.Violations = [];
        }
        var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations.ToArray(), conflictingLessons);

        await lessonRepository.SaveAllAsync(lessonsWithConflict);
    }

    public async Task UpdateAcademicDisciplineLessons(AcademicDiscipline academicDiscipline)
    {
        var previousAcademicDisciplineVersion = academicDiscipline.Id.HasValue
            ? await academicDisciplineRepository.GetAsync(academicDiscipline.Id!.Value)
            : null;
        var previousAllowedLessonTypes = previousAcademicDisciplineVersion?.AllowedLessonTypes ?? [];

        var lessonTypesToDelete = previousAllowedLessonTypes.Except(academicDiscipline.AllowedLessonTypes).ToArray();
        var lessonTypesToAdd = academicDiscipline.AllowedLessonTypes.Except(previousAllowedLessonTypes);
        var lessonTypesToUpdate = previousAllowedLessonTypes.Intersect(academicDiscipline.AllowedLessonTypes).ToArray();

        var lessonTypesToSave = lessonTypesToAdd.Concat(lessonTypesToUpdate).Distinct().ToArray();
        var lessonBatchInfoLinkedEntityIds = lessonTypesToSave
            .SelectMany(academicDiscipline.GetBatchInfosByType)
            .Select(lessonBatchInfo => new
            {
                StudentGroupIds = lessonBatchInfo.StudentGroups.Select(studentGroup => studentGroup.Id!.Value),
                TeacherIds = lessonBatchInfo.Teachers.Select(teacher => teacher.Id!.Value),
                RoomIds = lessonBatchInfo.Rooms.Select(room => room.Id!.Value),
            })
            .ToArray();

        var schedule = await scheduleRepository.GetAsync(academicDiscipline.ScheduleId);

        var lessonBatchInfoStudentGroups = await studentGroupRepository.SelectAsync(lessonBatchInfoLinkedEntityIds.SelectMany(x => x.StudentGroupIds).ToArray());
        var lessonBatchInfoTeachers = await teacherRepository.SelectAsync(lessonBatchInfoLinkedEntityIds.SelectMany(x => x.TeacherIds).ToArray());
        var lessonBatchInfoRooms = await roomRepository.SelectAsync(lessonBatchInfoLinkedEntityIds.SelectMany(x => x.RoomIds).ToArray());

        var lessonBatchInfoStudentGroupsById = lessonBatchInfoStudentGroups.ToDictionary(x => x.Id!.Value);
        var lessonBatchInfoTeachersById = lessonBatchInfoTeachers.ToDictionary(x => x.Id!.Value);
        var lessonBatchInfoRoomsById = lessonBatchInfoRooms.ToDictionary(x => x.Id!.Value);

        var lessonsToSave = lessonTypesToSave
            .SelectMany(lessonTypeToSave =>
            {
                var lessonBatchInfos = academicDiscipline.GetBatchInfosByType(lessonTypeToSave);
                return GetBatchLessonsToAdd( lessonBatchInfos, lessonTypeToSave);
            });

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
        await SaveLessonBatchAsync(lessonsToSave.ToArray());

        return;

        Lesson[] GetBatchLessonsToAdd(LessonBatchInfo[] lessonBatchInfos, AcademicDisciplineType type)
        {
            var result = new List<Lesson>();
            foreach (var lessonBatchInfo in lessonBatchInfos)
            {
                var groups = lessonBatchInfo.StudentGroups.Select(studentGroup => lessonBatchInfoStudentGroupsById[studentGroup.Id!.Value]).ToArray();
                var rootGroups = groups
                    .Where(l => groups
                        .Where(x => x.Id != l.Id)
                        .All(r => l.Parents
                            .All(x => x.Id!.Value != r.Id!.Value)))
                    .ToArray();
                var teachers = lessonBatchInfo.Teachers.Select(teacher => lessonBatchInfoTeachersById[teacher.Id!.Value]).ToArray();
                var rooms = lessonBatchInfo.Rooms.Select(room => lessonBatchInfoRoomsById[room.Id!.Value]).ToArray();
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
                        FlexibilityType = LessonFlexibilityType.Flexible,
                        HoursCost = lessonBatchInfo.HoursCost,
                        AllowCombining = lessonBatchInfo.AllowCombining,
                        LessonBatchInfoId = lessonBatchInfo.Id!.Value,
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

        var conflictingLessons = conflictingByTimeLessons.Concat(conflictingByRoomLessons).DistinctBy(x => x.Id).ToArray();
        foreach (var lesson in conflictingLessons)
        {
            lesson.Violations = [];
        }
        var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations.ToArray(), conflictingLessons);
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
                    .Where(x => x.Id != newStudentGroupLesson.Id
                                && x.DateWithTimeInterval!.HasIntersection(newStudentGroupLesson.DateWithTimeInterval!))
                    .ToArray();

                lessonValidationService.ValidateLessonConflictByGroup(newStudentGroupLesson,
                    conflictingByGroupLessons, lessonPolicyViolations, hierarchyIds);
            }
        }

        foreach (var lesson in studentGroupHierarchyAttachmentLessons)
        {
            lesson.Violations = [];
        }
        var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations.ToArray(), studentGroupHierarchyAttachmentLessons);
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
        var messagesFlat = messages.SelectMany(message => message.Messages).ToArray();
        return lessonPolicyViolations.Select((violation, index) => new LessonSeriesConflictDto
        {
            DayOfWeekTimeInterval = violation.Payload.DateWithTimeInterval != null
                ? new DayOfWeekTimeInterval
                {
                    DayOfWeek = violation.Payload.DateWithTimeInterval!.Date.DayOfWeek,
                    TimeInterval = violation.Payload.DateWithTimeInterval.TimeInterval,
                }
                : violation.Payload.DayOfWeekTimeInterval!,
            Messages = [new LessonSeriesConflictMessageDto
            {
                TimeInterval = violation.Payload.DateWithTimeInterval?.TimeInterval
                    ?? violation.Payload.DayOfWeekTimeInterval!.TimeInterval,
                Message = messagesFlat[index],
            }],
            ErrorType = violation.ErrorType,
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
        var ids = await lessonRepository.SaveAllAsync(lessons);
        lessons = await lessonRepository.SelectAsync(ids);
        var lessonPolicyViolations = await lessonValidationService.ValidateAsync(lessons);

        var conflictingLessons = await lessonRepository.SelectAsync(lessonPolicyViolations.Select(x => x.LessonId).ToArray());
        conflictingLessons = conflictingLessons.Concat(lessons).ToArray();
        var lessonsWithConflict = UpdateLessonsPolicyViolations(lessonPolicyViolations.ToArray(), conflictingLessons);
        await lessonRepository.SaveAllAsync(lessonsWithConflict);
    }

    private Lesson[] UpdateLessonsPolicyViolations(LessonPolicyViolation[] lessonPolicyViolations, Lesson[] lessonsWithConflict)
    {
        foreach (var lesson in lessonsWithConflict)
        {
            lesson.Id ??= Guid.Empty;
        }
        var affectedLessonNewViolationsByLessonId = lessonPolicyViolations
            .GroupBy(x => x.LessonId)
            .ToDictionary(x => x.Key);
        var lessonsWithConflictById = lessonsWithConflict
            .DistinctBy(x => x.Id)
            .Where(x => affectedLessonNewViolationsByLessonId.ContainsKey(x.Id!.Value))
            .ToDictionary(x => x.Id!.Value);
        foreach (var (lessonId, affectedLessonPolicyViolations) in affectedLessonNewViolationsByLessonId)
        {
            lessonsWithConflictById[lessonId].Violations = affectedLessonPolicyViolations.ToArray();
        }

        return lessonsWithConflictById.Select(x => x.Value).ToArray();
    }
}