using Dal.RegistryRepositories.Lesson;
using Dal.Repositories.DayOfWeekTimeIntervalAssignments;
using Dal.Repositories.LessonBatchInfo;
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
    ILessonBatchInfoRepository lessonBatchInfoRepository,
    IStudentGroupRepository studentGroupRepository,
    IScheduleRepository scheduleRepository,
    ITeacherRepository teacherRepository,
    IRoomRepository roomRepository,
    ITeacherPreferenceRepository teacherPreferenceRepository,
    IDayOfWeekTimeIntervalAssignmentRepository dayOfWeekTimeIntervalAssignmentRepository) : ILessonService
{
    public async Task<LessonShortDto[]> SearchWeekAsync(Guid scheduleId, DateOnly dateFrom, DateOnly dateTo)
    {
        var schedule = await scheduleRepository.GetAsync(scheduleId);
        if (schedule.DateInterval.DateTo < dateFrom || schedule.DateInterval.DateFrom > dateTo)
        {
            return [];
        }
        var lessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            DateFrom = dateFrom,
            DateTo = dateTo,
        });
        var lessonsCountByBatchId = lessons
            .GroupBy(x => x.LessonBatchInfoId)
            .ToDictionary(x => x.Key, x => x.Count());
        var lessonBatchInfos = await lessonBatchInfoRepository.SearchAsync(new LessonBatchInfoSearchModel
        {
            ScheduleId = scheduleId,
            DateFrom = dateFrom,
            DateTo = dateTo,
        });

        var notFullyPresentedLessonBatchInfos = lessonBatchInfos
            .Where(x => !lessonsCountByBatchId.ContainsKey(x.Id!.Value)
                        || lessonsCountByBatchId[x.Id!.Value] != x.LessonsPerWeekCount)
            .ToArray();
        var batchLessons = notFullyPresentedLessonBatchInfos.Length > 0
            ? await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = scheduleId,
                HasNoTimeAssignment = true,
                LessonBatchInfoIds = notFullyPresentedLessonBatchInfos.Select(x => x.Id!.Value).ToArray(),
            }) : [];
        var isEvenWeek = dateFrom.IntersectsEvenWeek(schedule.DateInterval);
        var noTimeAssignmentLessons = batchLessons
            .Where(x => (isEvenWeek && x.LessonBatchInfo.RepeatType != DisciplineLessonRepeatType.OddWeeks)
                        || (!isEvenWeek && x.LessonBatchInfo.RepeatType != DisciplineLessonRepeatType.EvenWeeks))
            .GroupBy(x => x.LessonBatchInfoId)
            .SelectMany(x => x.Take(x.First().LessonBatchInfo.LessonsPerWeekCount - (lessonsCountByBatchId.GetValueOrDefault(x.Key, 0))))
            .ToArray();

        var lessonsToReturn = lessons
            .Concat(noTimeAssignmentLessons)
            .DistinctBy(x => x.Id!.Value)
            .ToArray();
        var messagesByLessonId = (await lessonValidationService.FillValidationMessages(lessonsToReturn
            .Where(x => x.Violations.Length == 1)
            .ToArray()))
            .ToDictionary(x => x.LessonIds.Single());
        return lessonsToReturn
            .Select(x =>
            {
                var shortDto = LessonDtoMappingRegister.MapModelToShortDto(x);
                shortDto!.LessonPolicyViolationDescription = x.Violations.Length switch
                {
                    0 => null,
                    1 => messagesByLessonId[x.Id!.Value].Messages.Single().Message,
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
        var lesson = await lessonRepository.GetAsync(lessonSaveDto.Id!.Value);
        Validate(lessonSaveDto, lesson);

        var toSave = new List<Lesson>();
        var toDeleteIds = new List<Guid>();
        LessonBatchInfo? updatedLessonBatchInfoToSave = null;

        if (lessonSaveDto.UpdateBatch)
        {
            var batchLessons = await lessonRepository.SearchAsync(new LessonSearchModel
            {
                LessonBatchInfoIds = [lesson.LessonBatchInfoId],
            });

            if (lessonSaveDto.DateWithTimeInterval == null && lesson.DayOfWeekTimeIntervalAssignmentId.HasValue)
            {
                lesson.LessonBatchInfo.DayOfWeekTimeIntervals = lesson.LessonBatchInfo.DayOfWeekTimeIntervals
                    .Where(x => x.Id != lesson.DayOfWeekTimeIntervalAssignmentId!.Value)
                    .ToArray();
                toDeleteIds.AddRange(batchLessons
                    .Where(x => x.DayOfWeekTimeIntervalAssignmentId == lesson.DayOfWeekTimeIntervalAssignmentId!.Value)
                    .Select(x => x.Id!.Value));
            }

            var batch = lesson.LessonBatchInfo;
            var schedule = batch.AcademicDiscipline.Schedule;
            var rootGroups = lessonSaveDto.StudentGroupIds.Select(x => new StudentGroup { Id = x }).ToArray();
            var teachers = lessonSaveDto.TeacherIds.Select(x => new Teacher { Id = x }).ToArray();
            var rooms = lessonSaveDto.RoomIds.Select(x => new Room { Id = x }).ToArray();

            if (lessonSaveDto.DateWithTimeInterval != null)
            {
                var batchTimeAssignment = new DayOfWeekTimeIntervalAssignment
                {
                    Id = lesson.DayOfWeekTimeIntervalAssignmentId,
                    LessonBatchInfoId = batch.Id!.Value,
                    DayOfWeekTimeInterval = lessonSaveDto.DateWithTimeInterval.ToDayOfWeekTimeInterval(),
                };
                var id = await dayOfWeekTimeIntervalAssignmentRepository.SaveAsync(batchTimeAssignment);

                batchTimeAssignment.Id = id;
                batch.DayOfWeekTimeIntervals = batch.DayOfWeekTimeIntervals
                    .Where(x => x.Id != lesson.DayOfWeekTimeIntervalAssignmentId!.Value)
                    .Concat([batchTimeAssignment])
                    .ToArray();

                Update(batchLessons,
                    batchTimeAssignment,
                    batch,
                    schedule,
                    toSave,
                    toDeleteIds,
                    rootGroups,
                    teachers,
                    rooms);
            }

            AddNewMissingLessons(batch,
                schedule,
                toSave,
                rootGroups,
                teachers,
                rooms);

            var toSaveArray = toSave.ToArray();
            UpdateBatchLessons(lessonSaveDto, toSaveArray, lesson.LessonBatchInfo);
            toSave = toSaveArray.ToList();
            updatedLessonBatchInfoToSave = lesson.LessonBatchInfo;
        }
        else
        {
            LessonDtoMappingRegister.UpdateModelWithSaveDto(lessonSaveDto, lesson);
            lesson.DetachedFromBatch = true;
            toSave.Add(lesson);
        }

        await lessonRepository.DeleteAsync(toDeleteIds.ToArray());

        var ids = await lessonRepository.SaveAllAsync(toSave.ToArray());
        var savedLessons = await lessonRepository.SelectAsync(ids);
        if (savedLessons.Length > 0)
        {
            var lessonPolicyViolations = await lessonValidationService.ValidateAsync(savedLessons);
            await lessonValidationService.SaveAllAsync(lessonPolicyViolations);
        }

        if (updatedLessonBatchInfoToSave != null)
        {
            await lessonBatchInfoRepository.SaveAsync(updatedLessonBatchInfoToSave);
        }
    }

    public async Task RecalculateConflictsForUpdatedAcademicDiscipline(AcademicDiscipline academicDiscipline)
    {
        await lessonValidationService.RemovePolicyViolations(academicDiscipline.Id!.Value);
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
                academicDiscipline, lesson.LessonBatchInfo.Type);
        }

        await lessonValidationService.SaveAllAsync(lessonPolicyViolations.ToArray());
    }

    public async Task UpdateLessonsByBatches(Guid scheduleId, LessonBatchInfo[] lessonBatchInfos)
    {
        var schedule = await scheduleRepository.GetAsync(scheduleId);

        var lessonBatchInfoStudentGroupsById = (await studentGroupRepository.SelectAsync(lessonBatchInfos
            .SelectMany(x => x.StudentGroups.Select(y => y.Id!.Value))
            .Distinct()
            .ToArray())).ToDictionary(x => x.Id!.Value);
        var lessonBatchInfoTeachersById = (await teacherRepository.SelectAsync(lessonBatchInfos
            .SelectMany(x => x.Teachers.Select(y => y.Id!.Value))
            .Distinct()
            .ToArray())).ToDictionary(x => x.Id!.Value);
        var lessonBatchInfoRoomsById = (await roomRepository.SelectAsync(lessonBatchInfos
            .SelectMany(x => x.Rooms.Select(y => y.Id!.Value))
            .Distinct()
            .ToArray())).ToDictionary(x => x.Id!.Value);

        var existingLessonsByLessonBatchInfoId = (await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = scheduleId,
                AcademicDisciplineId = lessonBatchInfos.First().AcademicDisciplineId,
                LessonBatchInfoIds = lessonBatchInfos.Select(x => x.Id!.Value).ToArray(),
            }))
            .GroupBy(x => x.LessonBatchInfoId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var toSave = new List<Lesson>();
        var toDeleteIds = new List<Guid>();

        foreach (var batch in lessonBatchInfos)
        {
            var toSaveForBatch = new List<Lesson>();

            var groups = batch.StudentGroups.Select(studentGroup => lessonBatchInfoStudentGroupsById[studentGroup.Id!.Value]).ToArray();
            var rootGroups = groups
                .Where(l => groups
                    .Where(x => x.Id != l.Id)
                    .All(r => l.Parents
                        .All(x => x.Id!.Value != r.Id!.Value)))
                .ToArray();
            var teachers = batch.Teachers.Select(teacher => lessonBatchInfoTeachersById[teacher.Id!.Value]).ToArray();
            var rooms = batch.Rooms.Select(room => lessonBatchInfoRoomsById[room.Id!.Value]).ToArray();

            var existingLessons = existingLessonsByLessonBatchInfoId.TryGetValue(batch.Id!.Value, out var batchLessons) ? batchLessons : [];

            toDeleteIds.AddRange(existingLessons
                .Where(x => x.DayOfWeekTimeIntervalAssignmentId.HasValue
                            && batch.DayOfWeekTimeIntervals.All(y => y.Id != x.DayOfWeekTimeIntervalAssignmentId!.Value))
                .Select(x => x.Id!.Value));

            foreach (var batchTimeAssignment in batch.DayOfWeekTimeIntervals)
            {
                Update(existingLessons,
                    batchTimeAssignment,
                    batch,
                    schedule,
                    toSaveForBatch,
                    toDeleteIds,
                    rootGroups,
                    teachers,
                    rooms);
            }

            AddNewMissingLessons(batch,
                schedule,
                toSaveForBatch,
                rootGroups,
                teachers,
                rooms);

            var toSaveForBatchArray = toSaveForBatch.ToArray();
            UpdateBatchLessons(null, toSaveForBatchArray, batch);
            toSave.AddRange(toSaveForBatchArray);
        }

        await lessonRepository.DeleteAsync(toDeleteIds.ToArray());

        var ids = await lessonRepository.SaveAllAsync(toSave.ToArray());
        var savedLessons = await lessonRepository.SelectAsync(ids);
        if (savedLessons.Length > 0)
        {
            var lessonPolicyViolations = await lessonValidationService.ValidateAsync(savedLessons);
            await lessonValidationService.SaveAllAsync(lessonPolicyViolations);
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

        foreach (var conflictingLesson in conflictingByTimeLessons)
        {
            var conflictingTimeTeacherPreferences = timeBoundPreferences
                .Where(preference => preference.DayOfWeekTimeInterval!.HasIntersection(conflictingLesson.DateWithTimeInterval))
                .ToArray();
            lessonValidationService.ValidateTeacherPreferenceConflict(conflictingLesson, conflictingTimeTeacherPreferences,
                lessonPolicyViolations);
        }

        foreach (var conflictingLesson in conflictingByRoomLessons)
        {
            var conflictingRoomTeacherPreferences = roomBoundPreferences
                .Where(preference => conflictingLesson.Rooms.Any(x => x.Id == preference.RoomId))
                .ToArray();
            lessonValidationService.ValidateTeacherPreferenceConflict(conflictingLesson, conflictingRoomTeacherPreferences,
                lessonPolicyViolations);
        }

        await lessonValidationService.SaveAllAsync(lessonPolicyViolations.ToArray());
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
                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup,
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
            foreach (var mismatchedDisciplineLesson in studentGroupHierarchyAttachmentLessons)
            {
                lessonValidationService.ValidateAcademicDisciplineStudentGroupMatch(mismatchedDisciplineLesson,
                    lessonPolicyViolations, mismatchedDisciplineLesson.LessonBatchInfo.AcademicDiscipline, [studentGroup]);
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

        await lessonValidationService.SaveAllAsync(lessonPolicyViolations.ToArray());
    }

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Lesson lesson)
    {
        var studentGroupIds = lesson.LessonBatchInfo.StudentGroups.Select(x => x.Id!.Value).ToArray();
        var teacherIds = lesson.LessonBatchInfo.Teachers.Select(x => x.Id!.Value).ToArray();
        var roomIds = lesson.LessonBatchInfo.Rooms.Select(x => x.Id!.Value).ToArray();

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(studentGroupIds);
        var hierarchyIdsFlat = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray();

        var conflictingLessonsFromOtherBatches = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = lesson.LessonBatchInfo.AcademicDiscipline.ScheduleId,
            StudentGroupIds = hierarchyIdsFlat,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DateFrom = lesson.LessonBatchInfo.DateInterval.DateFrom,
            DateTo = lesson.LessonBatchInfo.DateInterval.DateTo,
            ExcludeLessonBatchInfoId = lesson.LessonBatchInfoId,
            ExcludeAllowCombining = true,
            SearchForConflicts = true,
        });

        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = lesson.LessonBatchInfo.AcademicDiscipline.ScheduleId,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
        });

        var lessonPolicyViolations = new List<LessonPolicyViolation>();
        lessonValidationService.BuildPolicyViolations(lessonPolicyViolations,
            studentGroupHierarchyIdsByStudentGroupId,
            conflictingLessonsFromOtherBatches, lesson, teacherIds, roomIds, conflictingTeacherPreferences, includeTiming: true);
        lessonPolicyViolations = lessonPolicyViolations.Where(violation => violation.LessonId == lesson.Id).ToList();

        foreach (var violation in lessonPolicyViolations)
        {
            violation.Id ??= Guid.NewGuid();
        }
        var messagesByLessonId =
            (await lessonValidationService.GetValidationResultMessageAsync(lessonPolicyViolations.ToArray()))
            .ToDictionary(x => x.LessonId, x => x.MessagesByViolationId);

        var includedKeys = new List<(DayOfWeekTimeInterval, string)>();

        var lessonSeriesConflicts = lessonPolicyViolations
            .GroupBy(x => x.LessonId)
            .SelectMany(group => group
                .Where(violation => violation.DayOfWeekTimeInterval != null)
                .Select(violation =>
                {
                    var key = (violation.DayOfWeekTimeInterval!,
                        messagesByLessonId[violation.LessonId][violation.Id!.Value]);
                    if (includedKeys.Contains(key))
                    {
                        return null;
                    }
                    includedKeys.Add(key);
                    return new LessonSeriesConflictDto
                    {
                        DayOfWeekTimeInterval = violation.DayOfWeekTimeInterval!,
                        Messages =
                        [
                            new LessonSeriesConflictMessageDto
                            {
                                TimeInterval = violation.DayOfWeekTimeInterval!.TimeInterval,
                                Message = messagesByLessonId[violation.LessonId][violation.Id!.Value],
                                ErrorType = violation.ErrorType,
                            }
                        ],
                        MaxErrorType = violation.ErrorType,
                    };
                }))
            .Where(x => x != null)
            .ToArray();

        return lessonSeriesConflicts!.MergeIntersections();
    }

    public async Task DeleteAsync(Guid lessonId)
    {
        await lessonRepository.DeleteAsync(lessonId);
    }

    private void Validate(LessonSaveDto lessonSaveDto, Lesson lesson)
    {
        var validationMessages = new List<ValidationMessage>();

        if (lessonSaveDto.DateWithTimeInterval != null)
        {
            if (!lesson.LessonBatchInfo.AcademicDiscipline.Schedule.DateInterval.HasIntersection(lessonSaveDto.DateWithTimeInterval!.Date)
                || (!lesson.DetachedFromBatch &&
                    !lesson.LessonBatchInfo.DateInterval.HasIntersection(lessonSaveDto.DateWithTimeInterval.Date)))
            {
                validationMessages.Add(new ValidationMessage("Дата сохраняемого занятия не входит в отрезок дат расписания или своего шаблона"));
            }
            else
            {
                var intersectsEvenWeek = lessonSaveDto.DateWithTimeInterval.Date.IntersectsEvenWeek(lesson.LessonBatchInfo.AcademicDiscipline.Schedule.DateInterval);
                switch (lesson.LessonBatchInfo.RepeatType)
                {
                    case DisciplineLessonRepeatType.EvenWeeks when !intersectsEvenWeek:
                        validationMessages.Add(new ValidationMessage("Занятие может быть сохранено только в четные недели согласно шаблону дисциплины"));
                        break;
                    case DisciplineLessonRepeatType.OddWeeks when intersectsEvenWeek:
                        validationMessages.Add(new ValidationMessage("Занятие может быть сохранено только в нечетные недели согласно шаблону дисциплины"));
                        break;
                }
            }

            if (lesson.DateWithTimeInterval != null
                && lesson.LessonBatchInfo.DayOfWeekTimeIntervals.Length > lesson.LessonBatchInfo.LessonsPerWeekCount)
            {
                validationMessages.Add(new ValidationMessage("Количество занятий в неделю превышает допустимое для шаблона занятий данного вида и дисциплины"));
            }
        }

        if (lessonSaveDto.UpdateBatch && lesson.DetachedFromBatch)
        {
            validationMessages.Add(new ValidationMessage("Занятие было откреплено от своего шаблона и не может быть изменено вместе с другими занятиями шаблона"));
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }
    }

    private void Update(Lesson[] batchLessons,
        DayOfWeekTimeIntervalAssignment batchTimeAssignment,
        LessonBatchInfo batch,
        Schedule schedule,
        List<Lesson> toSave,
        List<Guid> toDeleteIds,
        StudentGroup[] rootGroups,
        Teacher[] teachers,
        Room[] rooms)
    {
        var existingLessonsWithTimeAssignment = batchLessons
            .Where(x => x.DayOfWeekTimeIntervalAssignmentId == batchTimeAssignment.Id!.Value)
            .OrderBy(x => x.DateWithTimeInterval?.Date ?? DateOnly.MaxValue)
            .ThenBy(x => x.DateWithTimeInterval?.TimeInterval)
            .ToArray();
        var existingLessonsWithoutTimeAssignment = batchLessons
            .Where(x => !x.DayOfWeekTimeIntervalAssignmentId.HasValue)
            .ToArray();

        var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
            batch.DateInterval,
            [batchTimeAssignment.DayOfWeekTimeInterval.DayOfWeek],
            batch.RepeatType,
            schedule.DateInterval);

        for (var i = 0; i < dates.Length; i++)
        {
            var newDateWithTimeInterval = new DateWithTimeInterval
            {
                Date = dates[i],
                TimeInterval = batchTimeAssignment.DayOfWeekTimeInterval.TimeInterval,
            };

            if (i < existingLessonsWithTimeAssignment.Length)
            {
                existingLessonsWithTimeAssignment[i].DetachedFromBatch = false;
                existingLessonsWithTimeAssignment[i].DateWithTimeInterval = newDateWithTimeInterval;
                toSave.Add(existingLessonsWithTimeAssignment[i]);
            }
            else if (i - existingLessonsWithTimeAssignment.Length < existingLessonsWithoutTimeAssignment.Length)
            {
                existingLessonsWithoutTimeAssignment[i - existingLessonsWithTimeAssignment.Length].DetachedFromBatch = false;
                existingLessonsWithoutTimeAssignment[i - existingLessonsWithTimeAssignment.Length].DayOfWeekTimeIntervalAssignmentId = batchTimeAssignment.Id;
                existingLessonsWithoutTimeAssignment[i - existingLessonsWithTimeAssignment.Length].DateWithTimeInterval = newDateWithTimeInterval;
                toSave.Add(existingLessonsWithoutTimeAssignment[i - existingLessonsWithTimeAssignment.Length]);
            }
            else
            {
                toSave.Add(new Lesson
                {
                    StudentGroups = rootGroups,
                    Teachers = teachers,
                    Rooms = rooms,
                    DayOfWeekTimeIntervalAssignmentId = batchTimeAssignment.Id!.Value,
                    DateWithTimeInterval = newDateWithTimeInterval,
                    FlexibilityType = batch.FlexibilityType,
                    HoursCost = batch.HoursCost,
                    AllowCombining = batch.AllowCombining,
                    LessonBatchInfoId = batch.Id!.Value,
                });
            }
        }

        if (dates.Length >= existingLessonsWithTimeAssignment.Length)
        {
            return;
        }
        toDeleteIds.AddRange(Enumerable.Range(dates.Length, existingLessonsWithTimeAssignment.Length - dates.Length)
            .Select(i => existingLessonsWithTimeAssignment[i].Id!.Value));
    }

    private void AddNewMissingLessons(LessonBatchInfo batch,
        Schedule schedule,
        List<Lesson> toSave,
        StudentGroup[] rootGroups,
        Teacher[] teachers,
        Room[] rooms)
    {
        if (batch.LessonsPerWeekCount - batch.DayOfWeekTimeIntervals.Length <= 0)
        {
            return;
        }

        var lessonsWithoutTimeAssignmentTotalCount = DateOnlyHelper.GetDaysInDateIntervalCount(
            batch.DateInterval,
            batch.LessonsPerWeekCount - batch.DayOfWeekTimeIntervals.Length,
            batch.RepeatType,
            schedule.DateInterval) - toSave.Count(x => x.DayOfWeekTimeIntervalAssignmentId == null);

        if (lessonsWithoutTimeAssignmentTotalCount > 0)
        {
            toSave.AddRange(Enumerable.Range(0, lessonsWithoutTimeAssignmentTotalCount)
                .Select(_ => new Lesson
                {
                    StudentGroups = rootGroups,
                    Teachers = teachers,
                    Rooms = rooms,
                    FlexibilityType = batch.FlexibilityType,
                    HoursCost = batch.HoursCost,
                    AllowCombining = batch.AllowCombining,
                    LessonBatchInfoId = batch.Id!.Value,
                }));
        }
    }

    private void UpdateBatchLessons(LessonSaveDto? lessonSaveDto, Lesson[] lessons, LessonBatchInfo batchInfo)
    {
        foreach (var lessonToUpdate in lessons)
        {
            lessonToUpdate.StudentGroups = batchInfo.StudentGroups = lessonSaveDto?.StudentGroupIds.Select(x => new StudentGroup { Id = x }).ToArray() ?? batchInfo.StudentGroups;
            lessonToUpdate.Teachers = batchInfo.Teachers = lessonSaveDto?.TeacherIds.Select(x => new Teacher { Id = x }).ToArray() ?? batchInfo.Teachers;
            lessonToUpdate.Rooms = batchInfo.Rooms = lessonSaveDto?.RoomIds.Select(x => new Room { Id = x }).ToArray() ?? batchInfo.Rooms;
            lessonToUpdate.FlexibilityType = batchInfo.FlexibilityType = lessonSaveDto?.FlexibilityType ?? batchInfo.FlexibilityType;
            lessonToUpdate.HoursCost = batchInfo.HoursCost = lessonSaveDto?.HoursCost ?? batchInfo.HoursCost;
            lessonToUpdate.AllowCombining = batchInfo.AllowCombining = lessonSaveDto?.AllowCombining ?? batchInfo.AllowCombining;
        }
    }
}