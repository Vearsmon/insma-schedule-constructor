using Dal.RegistryRepositories.Lesson;
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
        var lessonBatchInfos = await lessonBatchInfoRepository.SearchAsync(new LessonBatchInfoSearchModel
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
        });

        var notFullyPresentedLessonBatchInfos = lessonBatchInfos
            .Where(x => lessons.All(y => y.LessonBatchInfoId != x.Id)
                        || lessons.Count(y => y.LessonBatchInfoId == x.Id) != x.LessonsPerWeekCount);
        var batchLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            HasNoTimeAssignment = true,
            LessonBatchInfoIds = notFullyPresentedLessonBatchInfos.Select(x => x.Id!.Value).ToArray(),
        });
        var noTimeAssignmentLessons = batchLessons
            .GroupBy(x => x.LessonBatchInfoId!.Value)
            .SelectMany(x => x.Take(x.First().LessonBatchInfo!.LessonsPerWeekCount - lessons.Count(y => y.LessonBatchInfoId == x.Key)))
            .ToArray();

        var messages = await lessonValidationService.FillValidationMessages(
            lessons.Concat(noTimeAssignmentLessons).DistinctBy(x => x.Id!.Value).Where(x => x.Violations.Length == 1).ToArray());
        return lessons.Concat(noTimeAssignmentLessons).DistinctBy(x => x.Id!.Value).Select(x =>
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
        var validationMessages = new List<ValidationMessage>();
        if (!lessonSaveDto.Id.HasValue)
        {
            validationMessages.Add(new ValidationMessage("Не допускается сохранение занятий без создания академической дисциплины"));
        }

        var lesson = await lessonRepository.GetAsync(lessonSaveDto.Id!.Value);
        var schedule = await scheduleRepository.GetAsync(lesson.ScheduleId);

        if (lessonSaveDto.DateWithTimeInterval != null)
        {
            if (!schedule.DateInterval.HasIntersection(lessonSaveDto.DateWithTimeInterval!.Date)
                || (!lesson.DetachedFromBatch &&
                    !lesson.LessonBatchInfo!.DateInterval.HasIntersection(lessonSaveDto.DateWithTimeInterval.Date)))
            {
                validationMessages.Add(new ValidationMessage("Дата сохраняемого занятия не входит в отрезок дат расписания или своего шаблона"));
            }
            else
            {
                var intersectsEvenWeek = lessonSaveDto.DateWithTimeInterval.Date.IntersectsEvenWeek(schedule.DateInterval);
                switch (lesson.LessonBatchInfo!.RepeatType)
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
                && lesson.LessonBatchInfo!.DayOfWeekTimeIntervals.Length == lesson.LessonBatchInfo.LessonsPerWeekCount)
            {
                validationMessages.Add(new ValidationMessage("Количество занятий в неделю превышает допустимое для шаблона занятий данного вида и дисциплины"));
            }
        }

        if (lessonSaveDto.UpdateBatch)
        {
            if (lesson.DetachedFromBatch)
            {
                validationMessages.Add(new ValidationMessage("Занятие было откреплено от своего шаблона и не может быть изменено вместе с другими занятиями шаблона"));
            }
            if (lessonSaveDto.DateWithTimeInterval != null && !lessonSaveDto.DateWithTimeInterval.Equals(lesson.DateWithTimeInterval))
            {
                var matchedAnyBatchInfoDayOfWeekIntervals = lesson.LessonBatchInfo!.DayOfWeekTimeIntervals
                    .Any(dayOfWeekTimeInterval => dayOfWeekTimeInterval.Equals(lessonSaveDto.DateWithTimeInterval.ToDayOfWeekTimeInterval()));
                if (matchedAnyBatchInfoDayOfWeekIntervals)
                {
                    validationMessages.Add(new ValidationMessage("Серия занятий не может быть поставлена в это время, так как оно уже занято другой серией этого шаблона"));
                }
            }
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var updatedLessonsToSave = new List<Lesson>();
        LessonBatchInfo? updatedLessonBatchInfoToSave = null;

        var batchLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = lesson.ScheduleId,
            AcademicDisciplineId = lesson.AcademicDisciplineId,
            LessonBatchInfoIds = [lesson.LessonBatchInfoId!.Value],
        });

        if (lessonSaveDto.DateWithTimeInterval != null)
        {
            if (lesson.DateWithTimeInterval == null)
            {
                var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                    lesson.LessonBatchInfo!.DateInterval,
                    [lessonSaveDto.DateWithTimeInterval.Date.DayOfWeek],
                    lesson.LessonBatchInfo.RepeatType,
                    schedule.DateInterval);

                UpdateBatchLessons(lessonSaveDto, batchLessons, lesson.LessonBatchInfo);

                var batchLessonsWithoutTimeAssignment = batchLessons
                    .Where(x => x.DateWithTimeInterval == null)
                    .Take(dates.Length)
                    .ToArray();

                for (var i = 0; i < dates.Length; i++)
                {
                    batchLessonsWithoutTimeAssignment[i].DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = dates[i],
                        TimeInterval = lessonSaveDto.DateWithTimeInterval.TimeInterval,
                    };
                }

                lesson.LessonBatchInfo!.DayOfWeekTimeIntervals = lesson.LessonBatchInfo.DayOfWeekTimeIntervals
                    .Concat([lessonSaveDto.DateWithTimeInterval.ToDayOfWeekTimeInterval()])
                    .ToArray();

                updatedLessonsToSave.AddRange(batchLessons
                    .Where(x => batchLessonsWithoutTimeAssignment.All(y => y.Id!.Value != x.Id!.Value) && x.Id!.Value != lesson.Id!.Value)
                    .Concat(batchLessonsWithoutTimeAssignment));
                updatedLessonBatchInfoToSave = lesson.LessonBatchInfo;
            }
            else if (lessonSaveDto.UpdateBatch)
            {
                var daysOffset = lessonSaveDto.DateWithTimeInterval.Date.DayNumber - lesson.DateWithTimeInterval!.Date.DayNumber;
                var batchLessonsToUpdate = batchLessons
                    .Where(x => x.DateWithTimeInterval != null
                                && x.DateWithTimeInterval.ToDayOfWeekTimeInterval().Equals(
                                    lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval())
                                && x.Id!.Value != lesson.Id!.Value)
                    .ToArray();

                if (daysOffset != 0 && batchLessonsToUpdate.Any(batchLesson =>
                    {
                        var dateWithOffset = batchLesson.DateWithTimeInterval!.Date.AddDays(daysOffset);
                        return !schedule.DateInterval.HasIntersection(dateWithOffset)
                               || !batchLesson.LessonBatchInfo!.DateInterval.HasIntersection(dateWithOffset);
                    }))
                {
                    throw new ServiceException(new ValidationMessage("Дата одного из сохраняемых занятий шаблона не входит в отрезок дат расписания или своего шаблона"));
                }

                UpdateBatchLessons(lessonSaveDto, batchLessons, lesson.LessonBatchInfo!);

                foreach (var batchLessonToUpdate in batchLessonsToUpdate)
                {
                    batchLessonToUpdate.DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = batchLessonToUpdate.DateWithTimeInterval!.Date.AddDays(daysOffset),
                        TimeInterval = lessonSaveDto.DateWithTimeInterval.TimeInterval,
                    };
                }

                lesson.LessonBatchInfo!.DayOfWeekTimeIntervals = lesson.LessonBatchInfo.DayOfWeekTimeIntervals
                    .Where(dayOfWeekTimeInterval => !dayOfWeekTimeInterval.Equals(lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval()))
                    .Concat([lessonSaveDto.DateWithTimeInterval.ToDayOfWeekTimeInterval()])
                    .ToArray();

                updatedLessonsToSave.AddRange(batchLessons
                    .Where(x => batchLessonsToUpdate.All(y => y.Id!.Value != x.Id!.Value) && x.Id!.Value != lesson.Id!.Value)
                    .Concat(batchLessonsToUpdate));
                updatedLessonBatchInfoToSave = lesson.LessonBatchInfo;
            }
            else if (!lessonSaveDto.DateWithTimeInterval.Equals(lesson.DateWithTimeInterval))
            {
                lesson.DetachedFromBatch = true;
            }
        }
        else if (lessonSaveDto.UpdateBatch)
        {
            var batchLessonsToUpdate = batchLessons
                .Where(x => x.DateWithTimeInterval != null
                            && x.DateWithTimeInterval.ToDayOfWeekTimeInterval().Equals(
                                lesson.DateWithTimeInterval?.ToDayOfWeekTimeInterval())
                            && x.Id!.Value != lesson.Id!.Value)
                .ToArray();

            UpdateBatchLessons(lessonSaveDto, batchLessons, lesson.LessonBatchInfo!);

            foreach (var batchLessonToUpdate in batchLessonsToUpdate)
            {
                batchLessonToUpdate.DateWithTimeInterval = null;
            }

            if (lesson.DateWithTimeInterval != null)
            {
                lesson.LessonBatchInfo!.DayOfWeekTimeIntervals = lesson.LessonBatchInfo.DayOfWeekTimeIntervals
                    .Where(dayOfWeekTimeInterval => !dayOfWeekTimeInterval.Equals(lesson.DateWithTimeInterval.ToDayOfWeekTimeInterval()))
                    .ToArray();
                updatedLessonBatchInfoToSave = lesson.LessonBatchInfo!;
            }

            updatedLessonsToSave.AddRange(batchLessons
                .Where(x => batchLessonsToUpdate.All(y => y.Id!.Value != x.Id!.Value) && x.Id!.Value != lesson.Id!.Value)
                .Concat(batchLessonsToUpdate));
        }

        LessonDtoMappingRegister.UpdateModelWithSaveDto(lessonSaveDto, lesson);
        var ids = await lessonRepository.SaveAllAsync(updatedLessonsToSave.Concat([lesson]).ToArray());
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
                academicDiscipline, lesson.AcademicDisciplineType!.Value);
        }

        await lessonValidationService.SaveAllAsync(lessonPolicyViolations.ToArray());
    }

    public async Task UpdateLessonsByBatches(Guid scheduleId, LessonBatchInfo[] lessonBatchInfos, Guid[] newLessonBatchInfoIds)
    {
        var lessonBatchInfoLinkedEntityIds = lessonBatchInfos
            .Select(lessonBatchInfo => new
            {
                StudentGroupIds = lessonBatchInfo.StudentGroups.Select(studentGroup => studentGroup.Id!.Value),
                TeacherIds = lessonBatchInfo.Teachers.Select(teacher => teacher.Id!.Value),
                RoomIds = lessonBatchInfo.Rooms.Select(room => room.Id!.Value),
            })
            .ToArray();

        var schedule = await scheduleRepository.GetAsync(scheduleId);

        var lessonBatchInfoStudentGroups = await studentGroupRepository.SelectAsync(lessonBatchInfoLinkedEntityIds.SelectMany(x => x.StudentGroupIds).Distinct().ToArray());
        var lessonBatchInfoTeachers = await teacherRepository.SelectAsync(lessonBatchInfoLinkedEntityIds.SelectMany(x => x.TeacherIds).Distinct().ToArray());
        var lessonBatchInfoRooms = await roomRepository.SelectAsync(lessonBatchInfoLinkedEntityIds.SelectMany(x => x.RoomIds).Distinct().ToArray());

        var lessonBatchInfoStudentGroupsById = lessonBatchInfoStudentGroups.ToDictionary(x => x.Id!.Value);
        var lessonBatchInfoTeachersById = lessonBatchInfoTeachers.ToDictionary(x => x.Id!.Value);
        var lessonBatchInfoRoomsById = lessonBatchInfoRooms.ToDictionary(x => x.Id!.Value);

        var lessonsToSave = lessonBatchInfos
            .Where(lessonBatchInfo => newLessonBatchInfoIds.Contains(lessonBatchInfo.Id!.Value))
            .SelectMany(GetBatchLessonsToAdd)
            .ToList();

        var existingLessonBatchInfosById = lessonBatchInfos
            .Where(lessonBatchInfo => !newLessonBatchInfoIds.Contains(lessonBatchInfo.Id!.Value))
            .ToDictionary(lessonBatchInfo => lessonBatchInfo.Id!.Value);
        if (existingLessonBatchInfosById.Keys.Count > 0)
        {
            var existingLessonsToUpdateByLessonBatchInfoId = (await lessonRepository.SearchAsync(new LessonSearchModel
                {
                    ScheduleId = scheduleId,
                    AcademicDisciplineId = lessonBatchInfos.First().AcademicDisciplineId,
                    LessonBatchInfoIds = existingLessonBatchInfosById.Keys.ToArray(),
                }))
                .GroupBy(x => x.LessonBatchInfoId!.Value)
                .ToDictionary(x => x.Key, x => x.ToArray());
            foreach (var (lessonBatchInfoId, lessons) in existingLessonsToUpdateByLessonBatchInfoId)
            {
                UpdateBatchLessons(null, lessons, existingLessonBatchInfosById[lessonBatchInfoId]);
                lessonsToSave.AddRange(lessons);
            }
        }

        var ids = await lessonRepository.SaveAllAsync(lessonsToSave.ToArray());
        var savedLessons = await lessonRepository.SelectAsync(ids);
        if (savedLessons.Length > 0)
        {
            var lessonPolicyViolations = await lessonValidationService.ValidateAsync(savedLessons);
            await lessonValidationService.SaveAllAsync(lessonPolicyViolations);
        }

        return;

        Lesson[] GetBatchLessonsToAdd(LessonBatchInfo lessonBatchInfo)
        {
            var result = new List<Lesson>();

            var groups = lessonBatchInfo.StudentGroups.Select(studentGroup => lessonBatchInfoStudentGroupsById[studentGroup.Id!.Value]).ToArray();
            var rootGroups = groups
                .Where(l => groups
                    .Where(x => x.Id != l.Id)
                    .All(r => l.Parents
                        .All(x => x.Id!.Value != r.Id!.Value)))
                .ToArray();
            var teachers = lessonBatchInfo.Teachers.Select(teacher => lessonBatchInfoTeachersById[teacher.Id!.Value]).ToArray();
            var rooms = lessonBatchInfo.Rooms.Select(room => lessonBatchInfoRoomsById[room.Id!.Value]).ToArray();

            var lessonsWithoutTimeAssignmentPerWeek = lessonBatchInfo.LessonsPerWeekCount - lessonBatchInfo.DayOfWeekTimeIntervals.Length;
            var lessonsWithoutTimeAssignmentTotalCount = DateOnlyHelper.GetDaysInDateIntervalCount(
                lessonBatchInfo.DateInterval,
                lessonsWithoutTimeAssignmentPerWeek,
                lessonBatchInfo.RepeatType,
                schedule.DateInterval);
            result.AddRange(Enumerable.Range(0, lessonsWithoutTimeAssignmentTotalCount)
                .Select(_ => new Lesson
                {
                    ScheduleId = scheduleId,
                    AcademicDisciplineId = lessonBatchInfo.AcademicDisciplineId,
                    AcademicDisciplineType = lessonBatchInfo.Type,
                    StudentGroups = rootGroups,
                    Teachers = teachers,
                    Rooms = rooms,
                    FlexibilityType = lessonBatchInfo.FlexibilityType,
                    HoursCost = lessonBatchInfo.HoursCost,
                    AllowCombining = lessonBatchInfo.AllowCombining,
                    LessonBatchInfoId = lessonBatchInfo.Id!.Value,
                }));

            var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                lessonBatchInfo.DateInterval,
                lessonBatchInfo.DayOfWeekTimeIntervals.Select(x => x.DayOfWeek).ToArray(),
                lessonBatchInfo.RepeatType,
                schedule.DateInterval);
            var timeIntervalsByDayOfWeek = lessonBatchInfo.DayOfWeekTimeIntervals
                .GroupBy(x => x.DayOfWeek)
                .ToDictionary(x => x.Key, x => x.ToArray());
            result.AddRange(dates
                .SelectMany(date => timeIntervalsByDayOfWeek[date.DayOfWeek]
                    .Select(dayOfWeekTimeInterval => new Lesson
                    {
                        ScheduleId = scheduleId,
                        AcademicDisciplineId = lessonBatchInfo.AcademicDisciplineId,
                        AcademicDisciplineType = lessonBatchInfo.Type,
                        StudentGroups = rootGroups,
                        Teachers = teachers,
                        Rooms = rooms,
                        DateWithTimeInterval = new DateWithTimeInterval
                        {
                            Date = date,
                            TimeInterval = dayOfWeekTimeInterval.TimeInterval,
                        },
                        FlexibilityType = lessonBatchInfo.FlexibilityType,
                        HoursCost = lessonBatchInfo.HoursCost,
                        AllowCombining = lessonBatchInfo.AllowCombining,
                        LessonBatchInfoId = lessonBatchInfo.Id!.Value,
                    })));

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

        await lessonValidationService.SaveAllAsync(lessonPolicyViolations.ToArray());
    }

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(Lesson lesson)
    {
        var studentGroupIds = lesson.LessonBatchInfo!.StudentGroups.Select(x => x.Id!.Value).ToArray();
        var teacherIds = lesson.LessonBatchInfo.Teachers.Select(x => x.Id!.Value).ToArray();
        var roomIds = lesson.LessonBatchInfo.Rooms.Select(x => x.Id!.Value).ToArray();

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(studentGroupIds);
        var hierarchyIdsFlat = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray();

        var conflictingLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = lesson.ScheduleId,
            StudentGroupIds = hierarchyIdsFlat,
            TeacherIds = teacherIds,
            RoomIds = roomIds,
            DateFrom = lesson.LessonBatchInfo.DateInterval.DateFrom,
            DateTo = lesson.LessonBatchInfo.DateInterval.DateTo,
            ExcludeAllowCombining = true,
            SearchForConflicts = true,
        });
        var conflictingLessonsFromOtherBatches = conflictingLessons
            .Where(conflictingLesson => conflictingLesson.LessonBatchInfoId != lesson.LessonBatchInfoId!.Value)
            .ToArray();

        var conflictingTeacherPreferences = await teacherPreferenceRepository.SearchAsync(new TeacherPreferenceSearchModel
        {
            ScheduleId = lesson.ScheduleId,
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
        var lessonSeriesConflicts = lessonPolicyViolations
            .GroupBy(x => x.LessonId)
            .SelectMany(group => group
                .Select(violation => new LessonSeriesConflictDto
                {
                    DayOfWeekTimeInterval = violation.Payload.DayOfWeekTimeInterval!,
                    Messages = [new LessonSeriesConflictMessageDto
                    {
                        TimeInterval = violation.Payload.DayOfWeekTimeInterval!.TimeInterval,
                        Message = messagesByLessonId[violation.LessonId][violation.Id!.Value],
                        ErrorType = violation.ErrorType,
                    }],
                    MaxErrorType = violation.ErrorType,
                }))
            .ToArray();

        return lessonSeriesConflicts.MergeIntersections();
    }

    public async Task DeleteAsync(Guid lessonId)
    {
        await lessonRepository.DeleteAsync(lessonId);
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