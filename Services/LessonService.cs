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
        return lessons.Select(DtoMappingRegister.MapShort).ToArray()!;
    }

    public async Task<RegistryDto<LessonRegistryItemDto>> SearchAsync(LessonRegistrySearchModel searchModel)
    {
        var registryEntries =
            await lessonRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<LessonRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray()!,
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
        foreach (var lesson in lessons)
        {
            foreach (var studentGroup in lesson.StudentGroups)
            {
                if (studentGroup.SemesterNumber == academicDiscipline.SemesterNumber
                    && academicDiscipline.AllowedLessonTypes.Contains(lesson.AcademicDisciplineType!.Value))
                {
                    continue;
                }

                var payload = new LessonValidationPayload
                {
                    AffectedByAcademicDisciplineId = academicDiscipline.Id,
                    AffectedByStudentGroupId = studentGroup.Id,
                };
                if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(lesson.Id!.Value,
                        out var affectedLessonValidationMessages))
                {
                    affectedLessonValidationMessages = [];
                    affectedLessonNewValidationMessagesByLessonId[lesson.Id!.Value] =
                        affectedLessonValidationMessages;
                }

                affectedLessonValidationMessages!
                    .AddErrorIf(studentGroup.SemesterNumber != null
                                && academicDiscipline.SemesterNumber != null
                                && studentGroup.SemesterNumber != academicDiscipline.SemesterNumber,
                        payload, LessonValidationCode.MismatchedSemesterNumber);
                affectedLessonValidationMessages!
                    .AddErrorIf(
                        !academicDiscipline.AllowedLessonTypes.Contains(lesson.AcademicDisciplineType!.Value),
                        new LessonValidationPayload { AffectedByAcademicDisciplineId = academicDiscipline.Id! },
                        LessonValidationCode.MismatchedAcademicDisciplineType);
            }
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
            });

            await lessonRepository.DeleteAsync(lessonsToDelete.Select(x => x.Id!.Value).ToArray());
        }

        var lessonsToSave = new List<Lesson>();
        var schedule = await scheduleRepository.GetAsync(academicDiscipline.ScheduleId);
        foreach (var lessonTypeToSave in lessonTypesToAdd.Concat(lessonTypesToUpdate).Distinct().ToArray())
        {
            switch (lessonTypeToSave)
            {
                case AcademicDisciplineType.Lecture when academicDiscipline.LecturePayload != null
                                                         && (academicDiscipline.LecturePayload.LessonBatchInfos.Length > 0
                                                             || academicDiscipline.LecturePayload.TotalHoursCount != 0):
                    lessonsToSave.AddRange(await GetBatchLessonsToAdd(academicDiscipline.LecturePayload.LessonBatchInfos,
                        AcademicDisciplineType.Lecture));
                    break;
                case AcademicDisciplineType.Lab when academicDiscipline.LabPayload != null
                                                     && (academicDiscipline.LabPayload.LessonBatchInfos.Length > 0
                                                         || academicDiscipline.LabPayload.TotalHoursCount != 0):
                    lessonsToSave.AddRange(await GetBatchLessonsToAdd(academicDiscipline.LabPayload.LessonBatchInfos,
                        AcademicDisciplineType.Lab));
                    break;
                case AcademicDisciplineType.Practice when academicDiscipline.PracticePayload != null
                                                          && (academicDiscipline.PracticePayload.LessonBatchInfos.Length > 0
                                                              || academicDiscipline.PracticePayload.TotalHoursCount != 0):
                    lessonsToSave.AddRange(await GetBatchLessonsToAdd(academicDiscipline.PracticePayload.LessonBatchInfos,
                        AcademicDisciplineType.Practice));
                    break;
                case AcademicDisciplineType.Exam when academicDiscipline.ExamPayload != null
                                                      && (academicDiscipline.ExamPayload.LessonBatchInfos.Length > 0
                                                          || academicDiscipline.ExamPayload.TotalHoursCount != 0):
                    lessonsToSave.AddRange(await GetBatchLessonsToAdd(academicDiscipline.ExamPayload.LessonBatchInfos,
                        AcademicDisciplineType.Exam));
                    break;
                case AcademicDisciplineType.Test when academicDiscipline.TestPayload != null
                                                      && (academicDiscipline.TestPayload.LessonBatchInfos.Length > 0
                                                          || academicDiscipline.TestPayload.TotalHoursCount != 0):
                    lessonsToSave.AddRange(await GetBatchLessonsToAdd(academicDiscipline.TestPayload.LessonBatchInfos,
                        AcademicDisciplineType.Test));
                    break;
            }
        }

        await SaveLessonBatchAsync(lessonsToSave.ToArray());

        return;

        async Task<Lesson[]> GetBatchLessonsToAdd(LessonBatchInfo[] lessonBatchInfos, AcademicDisciplineType type)
        {
            if (lessonBatchInfos.Length == 0)
            {
                return [];
            }

            var result = new List<Lesson>();
            foreach (var lessonBatchInfo in lessonBatchInfos)
            {
                var groups = await studentGroupRepository.SelectAsync(lessonBatchInfo.StudentGroups.Select(x => x.Id!.Value).ToArray());
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
                        StudentGroups = groups,
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
            [
                LessonValidationCode.FlexibleLessonTypeConflictByGroup,
                LessonValidationCode.FixedLessonTypeConflictByGroup
            ]);

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync([studentGroup.Id!.Value]);
        var hierarchyIds = studentGroupHierarchyIdsByStudentGroupId[studentGroup.Id!.Value].ToArray();
        var studentGroupHierarchyAttachmentLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = studentGroup.ScheduleId,
            StudentGroupIds = hierarchyIds,
        });

        var affectedLessonNewValidationMessagesByLessonId = new Dictionary<Guid, List<LessonValidationMessage>?>();

        foreach (var mismatchedDisciplineLesson in studentGroupHierarchyAttachmentLessons.Where(x =>
                     x.AcademicDiscipline != null
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

            affectedLessonValidationMessages!
                .AddErrorIf(mismatchedDisciplineLesson.AcademicDiscipline.SemesterNumber != null
                            && studentGroup.SemesterNumber != null
                            && mismatchedDisciplineLesson.AcademicDiscipline.SemesterNumber != studentGroup.SemesterNumber,
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
            foreach (var newStudentGroupLesson in lessonsOnDate
                         .Where(x => x.StudentGroups.Any(y => y.Id == studentGroup.Id)))
            {
                foreach (var conflictingLesson in lessonsOnDate
                             .Where(x => x.Id != newStudentGroupLesson.Id
                                         && x.DateWithTimeInterval!.HasIntersection(newStudentGroupLesson
                                             .DateWithTimeInterval!)))
                {
                    var editedLessonPayload = new LessonValidationPayload
                    {
                        AffectedByLessonId = conflictingLesson.Id,
                        AffectedByStudentGroupId =
                            conflictingLesson.StudentGroups.Single(x => hierarchyIds.Contains(x.Id!.Value)).Id!.Value,
                    };
                    if (!affectedLessonNewValidationMessagesByLessonId.TryGetValue(newStudentGroupLesson.Id!.Value,
                            out var lessonValidationMessages))
                    {
                        lessonValidationMessages = [];
                        affectedLessonNewValidationMessagesByLessonId[newStudentGroupLesson.Id!.Value] =
                            lessonValidationMessages;
                    }

                    lessonValidationMessages!
                        .AddWarningIf(conflictingLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                            editedLessonPayload,
                            LessonValidationCode.FlexibleLessonTypeConflictByGroup);
                    lessonValidationMessages!
                        .AddErrorIf(conflictingLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                            editedLessonPayload,
                            LessonValidationCode.FixedLessonTypeConflictByGroup);

                    var existedLessonPayload = new LessonValidationPayload
                    {
                        AffectedByLessonId = newStudentGroupLesson.Id,
                        AffectedByStudentGroupId = newStudentGroupLesson.StudentGroups
                            .Single(x => hierarchyIds.Contains(x.Id!.Value)).Id!.Value,
                    };
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

    public async Task<LessonSeriesConflictDto[]> GetLessonSeriesConflictsAsync(
        Guid academicDisciplineId,
        LessonBatchInfo lessonBatchInfo,
        AcademicDisciplineType academicDisciplineType, Guid scheduleId)
    {
        var validationMessages = new List<LessonValidationMessage>();

        var studentGroupHierarchyIdsByStudentGroupId =
            await studentGroupRepository.GetStudentGroupTreeIdsAsync(lessonBatchInfo.StudentGroups
                .Select(x => x.Id!.Value).ToArray());
        var conflictingByGroupLessons = await lessonRepository.SearchAsync(new LessonSearchModel
        {
            ScheduleId = scheduleId,
            StudentGroupIds = studentGroupHierarchyIdsByStudentGroupId.SelectMany(x => x.Value).ToArray(),
            DateFrom = lessonBatchInfo.DateInterval.DateFrom,
            DateTo = lessonBatchInfo.DateInterval.DateTo,
        });

        foreach (var studentGroupIdKey in studentGroupHierarchyIdsByStudentGroupId.Keys)
        {
            var hierarchyIds = studentGroupHierarchyIdsByStudentGroupId[studentGroupIdKey]
                .ToArray();
            var conflictingByGroupHierarchyLessons = conflictingByGroupLessons
                .Where(x => x.StudentGroups.Any(y => hierarchyIds.Contains(y.Id!.Value)))
                .ToArray();

            foreach (var conflictingByGroupLesson in conflictingByGroupHierarchyLessons)
            {
                var editedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = conflictingByGroupLesson.Id,
                    AffectedByStudentGroupId = conflictingByGroupLesson.StudentGroups
                        .Single(x => hierarchyIds.Contains(x.Id!.Value)).Id!.Value,
                    DateWithTimeInterval = conflictingByGroupLesson.DateWithTimeInterval,
                };
                validationMessages
                    .AddWarningIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonValidationCode.FlexibleLessonTypeConflictByGroup);
                validationMessages
                    .AddErrorIf(conflictingByGroupLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonValidationCode.FixedLessonTypeConflictByGroup);
            }
        }

        var conflictingByTeacherLessons = lessonBatchInfo.Teachers.Length > 0
            ? await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = scheduleId,
                TeacherIds = lessonBatchInfo.Teachers.Select(x => x.Id!.Value).ToArray(),
                DateFrom = lessonBatchInfo.DateInterval.DateFrom,
                DateTo = lessonBatchInfo.DateInterval.DateTo,
            })
            : [];

        foreach (var conflictingByTeacherLesson in conflictingByTeacherLessons)
        {
            foreach (var teacher in conflictingByTeacherLesson.Teachers
                         .Where(x => lessonBatchInfo.Teachers.Any(y => y.Id == x.Id)))
            {
                var editedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = conflictingByTeacherLesson.Id,
                    AffectedByTeacherId = teacher.Id,
                    DateWithTimeInterval = conflictingByTeacherLesson.DateWithTimeInterval,
                };
                validationMessages
                    .AddWarningIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonValidationCode.FlexibleLessonTypeConflictByTeacher);
                validationMessages
                    .AddErrorIf(conflictingByTeacherLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonValidationCode.FixedLessonTypeConflictByTeacher);
            }
        }

        var conflictingTimeTeacherPreferences = lessonBatchInfo.Teachers.Length > 0
            ? await teacherPreferenceRepository.SearchAsync(
                new TeacherPreferenceSearchModel
                {
                    ScheduleId = scheduleId,
                    TeacherIds = lessonBatchInfo.Teachers.Select(x => x.Id!.Value).ToArray(),
                    TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable]
                })
            : [];
        var conflictingRoomTeacherPreferences = lessonBatchInfo.Rooms.Length > 0
            ? await teacherPreferenceRepository.SearchAsync(
                new TeacherPreferenceSearchModel
                {
                    ScheduleId = scheduleId,
                    TeacherIds = lessonBatchInfo.Teachers.Select(x => x.Id!.Value).ToArray(),
                    RoomIds = lessonBatchInfo.Rooms.Select(x => x.Id!.Value).ToArray(),
                    TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
                })
            : [];

        foreach (var conflictingTeacherPreference in conflictingTimeTeacherPreferences.Concat(
                     conflictingRoomTeacherPreferences))
        {
            var payload = new LessonValidationPayload
            {
                AffectedByTeacherPreferenceId = conflictingTeacherPreference.Id,
                AffectedByTeacherId = conflictingTeacherPreference.Teacher.Id!.Value,
                DayOfWeekTimeInterval = conflictingTeacherPreference.DayOfWeekTimeInterval,
            };
            validationMessages
                .AddWarningIf(
                    conflictingTeacherPreference is
                        { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    payload,
                    LessonValidationCode.UndesirableTimeTeacherPreferenceTypeConflict);
            validationMessages
                .AddErrorIf(
                    conflictingTeacherPreference is
                        { DayOfWeekTimeInterval: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict);
            validationMessages
                .AddWarningIf(
                    conflictingTeacherPreference is
                        { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Undesirable },
                    payload,
                    LessonValidationCode.UndesirableRoomTeacherPreferenceTypeConflict);
            validationMessages
                .AddErrorIf(
                    conflictingTeacherPreference is
                        { RoomId: not null, TeacherPreferenceType: TeacherPreferenceType.Restricted },
                    payload,
                    LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict);
        }

        var conflictingByRoomLessons = lessonBatchInfo.Rooms.Length > 0
            ? await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = scheduleId,
                RoomIds = lessonBatchInfo.Rooms.Select(x => x.Id!.Value).ToArray(),
                DateFrom = lessonBatchInfo.DateInterval.DateFrom,
                DateTo = lessonBatchInfo.DateInterval.DateTo,
            })
            : [];

        foreach (var conflictingByRoomLesson in conflictingByRoomLessons)
        {
            foreach (var room in conflictingByRoomLesson.Rooms
                         .Where(x => lessonBatchInfo.Rooms.Any(y => y.Id == x.Id)))
            {
                var editedLessonPayload = new LessonValidationPayload
                {
                    AffectedByLessonId = conflictingByRoomLesson.Id,
                    AffectedByRoomId = room.Id,
                    DateWithTimeInterval = conflictingByRoomLesson.DateWithTimeInterval,
                };
                validationMessages
                    .AddWarningIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Flexible,
                        editedLessonPayload,
                        LessonValidationCode.FlexibleLessonTypeConflictByRoom);
                validationMessages
                    .AddErrorIf(conflictingByRoomLesson.FlexibilityType == LessonFlexibilityType.Fixed,
                        editedLessonPayload,
                        LessonValidationCode.FixedLessonTypeConflictByRoom);
            }
        }

        var lessonConflicts = new List<LessonSeriesConflictDto>();
        foreach (var validationMessage in validationMessages)
        {
            var studentGroup = validationMessage.Payload.AffectedByStudentGroupId.HasValue
                ? await studentGroupRepository.GetAsync(validationMessage.Payload.AffectedByStudentGroupId!.Value)
                : null;
            var affectedByLesson = validationMessage.Payload.AffectedByLessonId.HasValue
                ? await lessonRepository.GetAsync(validationMessage.Payload.AffectedByLessonId!.Value)
                : null;
            var teacher = validationMessage.Payload.AffectedByTeacherId.HasValue
                ? await teacherRepository.GetAsync(validationMessage.Payload.AffectedByTeacherId!.Value)
                : null;
            var room = validationMessage.Payload.AffectedByRoomId.HasValue
                ? await roomRepository.GetAsync(validationMessage.Payload.AffectedByRoomId!.Value)
                : null;
            var message = validationMessage.Code switch
            {
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
                _ => throw new NotSupportedException(),
            };
            lessonConflicts.Add(new LessonSeriesConflictDto
            {
                DayOfWeekTimeInterval = validationMessage.Payload.DateWithTimeInterval != null
                    ? new DayOfWeekTimeInterval
                    {
                        DayOfWeek = validationMessage.Payload.DateWithTimeInterval!.Date.DayOfWeek,
                        TimeInterval = validationMessage.Payload.DateWithTimeInterval.TimeInterval,
                    }
                    : validationMessage.Payload.DayOfWeekTimeInterval!,
                Messages = [message],
                ErrorType = validationMessage.ErrorType,
            });
        }

        return lessonConflicts.ToArray();
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