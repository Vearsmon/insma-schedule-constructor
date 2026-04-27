using Dal.RegistryRepositories.Lesson;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
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
                if (studentGroup.Cypher == academicDiscipline.Cypher
                    && studentGroup.SemesterNumber == academicDiscipline.SemesterNumber
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

                // affectedLessonValidationMessages!
                //     .AddErrorIf(studentGroup.Cypher != academicDiscipline.Cypher,
                //         payload, LessonValidationCode.MismatchedCyphers);
                affectedLessonValidationMessages!
                    .AddErrorIf(studentGroup.SemesterNumber != academicDiscipline.SemesterNumber,
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
                case AcademicDisciplineType.Lecture:
                    lessonsToSave.AddRange(GetBatchLessonsToAdd(academicDiscipline.LecturePayload?.LessonBatchInfo,
                        AcademicDisciplineType.Lecture));
                    break;
                case AcademicDisciplineType.Lab:
                    lessonsToSave.AddRange(GetBatchLessonsToAdd(academicDiscipline.LabPayload?.LessonBatchInfo!,
                        AcademicDisciplineType.Lab));
                    break;
                case AcademicDisciplineType.Practice:
                    lessonsToSave.AddRange(GetBatchLessonsToAdd(academicDiscipline.PracticePayload?.LessonBatchInfo,
                        AcademicDisciplineType.Practice));
                    break;
            }
        }

        await SaveLessonBatchAsync(lessonsToSave.ToArray());

        return;

        Lesson[] GetBatchLessonsToAdd(AcademicDisciplineLessonBatchInfo? lessonBatchInfo, AcademicDisciplineType type)
        {
            if (lessonBatchInfo == null)
            {
                return [];
            }

            var dates = DateOnlyHelper.GetDatesInIntervalByDaysOfWeek(
                lessonBatchInfo.DateInterval,
                lessonBatchInfo.DayOfWeekTimeIntervals.Select(x => x.DayOfWeek).ToArray(),
                lessonBatchInfo.RepeatType,
                schedule.DateInterval);
            var timeIntervalsByDayOfWeek =
                lessonBatchInfo.DayOfWeekTimeIntervals.ToDictionary(x => x.DayOfWeek);
            return dates
                .Select(date => new Lesson
                {
                    ScheduleId = academicDiscipline.ScheduleId,
                    AcademicDisciplineId = academicDiscipline.Id,
                    AcademicDisciplineType = type,
                    StudentGroups = lessonBatchInfo.StudentGroups,
                    TeacherId = lessonBatchInfo.TeacherId,
                    RoomId = lessonBatchInfo.RoomId,
                    DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = date,
                        TimeInterval = timeIntervalsByDayOfWeek[date.DayOfWeek].TimeInterval,
                    },
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AllowCombining = lessonBatchInfo.AllowCombining,
                    HoursCost = lessonBatchInfo.HoursCost,
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

            // affectedLessonValidationMessages!
            //     .AddErrorIf(mismatchedDisciplineLesson.AcademicDiscipline.Cypher != studentGroup.Cypher,
            //         payload, LessonValidationCode.MismatchedCyphers);
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
        AcademicDisciplineLessonBatchInfo lessonBatchInfo,
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

        var conflictingByTeacherLessons = lessonBatchInfo.TeacherId.HasValue
            ? await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = scheduleId,
                TeacherId = lessonBatchInfo.TeacherId!.Value,
                DateFrom = lessonBatchInfo.DateInterval.DateFrom,
                DateTo = lessonBatchInfo.DateInterval.DateTo,
            })
            : [];

        foreach (var conflictingByTeacherLesson in conflictingByTeacherLessons)
        {
            var editedLessonPayload = new LessonValidationPayload
            {
                AffectedByLessonId = conflictingByTeacherLesson.Id,
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

        var conflictingTimeTeacherPreferences = lessonBatchInfo.TeacherId.HasValue
            ? await teacherPreferenceRepository.SearchAsync(
                new TeacherPreferenceSearchModel
                {
                    ScheduleId = scheduleId,
                    TeacherId = lessonBatchInfo.TeacherId!.Value,
                    TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable]
                })
            : [];
        var conflictingRoomTeacherPreferences = lessonBatchInfo.RoomId.HasValue
            ? await teacherPreferenceRepository.SearchAsync(
                new TeacherPreferenceSearchModel
                {
                    ScheduleId = scheduleId,
                    TeacherId = lessonBatchInfo.TeacherId,
                    RoomIds = [lessonBatchInfo.RoomId!.Value],
                    TeacherPreferenceTypes = [TeacherPreferenceType.Restricted, TeacherPreferenceType.Undesirable],
                })
            : [];

        foreach (var conflictingTeacherPreference in conflictingTimeTeacherPreferences.Concat(
                     conflictingRoomTeacherPreferences))
        {
            var payload = new LessonValidationPayload
            {
                AffectedByTeacherPreferenceId = conflictingTeacherPreference.Id,
                AffectedByTeacherId = lessonBatchInfo.TeacherId!.Value,
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

        var conflictingByRoomLessons = lessonBatchInfo.RoomId.HasValue
            ? await lessonRepository.SearchAsync(new LessonSearchModel
            {
                ScheduleId = scheduleId,
                RoomIds = [lessonBatchInfo.RoomId!.Value],
                DateFrom = lessonBatchInfo.DateInterval.DateFrom,
                DateTo = lessonBatchInfo.DateInterval.DateTo,
            })
            : [];

        foreach (var conflictingByRoomLesson in conflictingByRoomLessons)
        {
            var editedLessonPayload = new LessonValidationPayload
            {
                AffectedByLessonId = conflictingByRoomLesson.Id,
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
                    affectedByLesson.Teacher!.Fullname),
                LessonValidationCode.FlexibleLessonTypeConflictByTeacher => string.Format(
                    LessonValidationMessageTemplates.FlexibleLessonTypeConflictByTeacherTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    affectedByLesson.Teacher!.Fullname),
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
                    affectedByLesson.Room!.Name),
                LessonValidationCode.FlexibleLessonTypeConflictByRoom => string.Format(
                    LessonValidationMessageTemplates.FlexibleLessonTypeConflictByRoomTemplate,
                    affectedByLesson!.AcademicDiscipline == null
                        ? string.Empty
                        : affectedByLesson.AcademicDiscipline.Name,
                    affectedByLesson.Room!.Name),
                _ => throw new NotImplementedException(),
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
                Message = message,
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