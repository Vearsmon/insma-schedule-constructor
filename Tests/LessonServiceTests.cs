using AutoFixture;
using Dal.RegistryRepositories.Lesson;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.LessonBatchInfo;
using Dal.Repositories.LessonPolicyViolations;
using Dal.Repositories.Lessons;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Constants;
using Domain.Dto.ShortDto;
using Domain.Helpers;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.SearchModels;
using Domain.Services;
using Moq;
using Services;

namespace Tests;

public class LessonServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<ILessonRepository> _lessonRepositoryMock = new();
    private readonly Mock<ILessonRegistryRepository> _lessonRegistryRepositoryMock = new();
    private ILessonValidationService _lessonValidationService = null!;
    private readonly Mock<ILessonBatchInfoRepository> _lessonBatchInfoRepositoryMock = new();
    private readonly Mock<IAcademicDisciplineRepository> _academicDisciplineRepositoryMock = new();
    private readonly Mock<IStudentGroupRepository> _studentGroupRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<ITeacherRepository> _teacherRepositoryMock = new();
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<ITeacherPreferenceRepository> _teacherPreferenceRepositoryMock = new();
    private readonly Mock<ILessonPolicyViolationRepository> _lessonPolicyViolationRepositoryMock = new();

    private LessonService CreateService()
    {
        _lessonValidationService = new LessonValidationService(
            _lessonRepositoryMock.Object,
            _lessonPolicyViolationRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _teacherRepositoryMock.Object,
            _academicDisciplineRepositoryMock.Object,
            _roomRepositoryMock.Object,
            _studentGroupRepositoryMock.Object,
            _teacherPreferenceRepositoryMock.Object);
        return new(
            _lessonRepositoryMock.Object,
            _lessonRegistryRepositoryMock.Object,
            _lessonValidationService,
            _lessonBatchInfoRepositoryMock.Object,
            _academicDisciplineRepositoryMock.Object,
            _studentGroupRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _teacherRepositoryMock.Object,
            _roomRepositoryMock.Object,
            _teacherPreferenceRepositoryMock.Object
        );
    }

    [Fact]
    public async Task SearchWeekAsync_Should_Return_Valid_Result()
    {
        // Arrange
        var affectedByTeacherId = Guid.NewGuid();
        var lessonIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var lessons = new[]
        {
            _fixture.Build<Lesson>()
                .With(x => x.Id, lessonIds[0])
                .Without(x => x.Schedule)
                .Without(x => x.AcademicDisciplineId)
                .Without(x => x.AcademicDiscipline)
                .Without(x => x.AcademicDisciplineType)
                .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid(), Name = _fixture.Create<string>() }])
                .With(x => x.Teachers, [new Teacher { Id = Guid.NewGuid(), Fullname = _fixture.Create<string>(), Contacts = _fixture.Create<string>() }])
                .With(x => x.Rooms, [new Room { Id = Guid.NewGuid(), Name = _fixture.Create<string>() }])
                .With(x => x.DateWithTimeInterval, _fixture.Create<DateWithTimeInterval>())
                .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
                .With(x => x.AllowCombining, true)
                .Without(x => x.HoursCost)
                .Without(x => x.LessonBatchInfoId)
                .Without(x => x.LessonBatchInfo)
                .With(x => x.Violations,
                [
                    new LessonPolicyViolation
                    {
                        Id = Guid.NewGuid(),
                        LessonId = lessonIds[0],
                        ErrorType = LessonValidationErrorType.Error,
                        Payload = new LessonValidationPayload { AffectedByTeacherId = affectedByTeacherId },
                        Code = LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict,
                    }
                ])
                .Create(),
            _fixture.Build<Lesson>()
                .With(x => x.Id, lessonIds[1])
                .Without(x => x.Schedule)
                .Without(x => x.AcademicDisciplineId)
                .With(x => x.AcademicDiscipline, new AcademicDiscipline { Name = _fixture.Create<string>() })
                .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
                .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
                .Without(x => x.Teachers)
                .With(x => x.Rooms, [new Room { Id = Guid.NewGuid() }])
                .Without(x => x.DateWithTimeInterval)
                .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
                .Without(x => x.AllowCombining)
                .Without(x => x.HoursCost)
                .Without(x => x.LessonBatchInfoId)
                .Without(x => x.LessonBatchInfo)
                .With(x => x.Violations, [])
                .Create(),
            _fixture.Build<Lesson>()
                .With(x => x.Id, lessonIds[2])
                .Without(x => x.Schedule)
                .Without(x => x.AcademicDisciplineId)
                .Without(x => x.AcademicDiscipline)
                .Without(x => x.AcademicDisciplineType)
                .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
                .Without(x => x.Teachers)
                .Without(x => x.Rooms)
                .Without(x => x.DateWithTimeInterval)
                .Without(x => x.FlexibilityType)
                .Without(x => x.AllowCombining)
                .Without(x => x.HoursCost)
                .With(x => x.LessonBatchInfoId, Guid.NewGuid())
                .With(x => x.LessonBatchInfo, new LessonBatchInfo())
                .With(x => x.Violations,
                    [
                        new LessonPolicyViolation
                        {
                            Id = Guid.NewGuid(),
                            LessonId = lessonIds[2],
                            ErrorType = LessonValidationErrorType.Warning,
                            Payload = new LessonValidationPayload { AffectedByTeacherId = affectedByTeacherId },
                            Code = LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict,
                        },
                        new LessonPolicyViolation
                        {
                            Id = Guid.NewGuid(),
                            LessonId = lessonIds[2],
                            ErrorType = LessonValidationErrorType.Warning,
                            Payload = new LessonValidationPayload { AffectedByTeacherId = affectedByTeacherId },
                            Code = LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict,
                        },
                    ])
                .Create(),
        };

        var expectedLessons = lessons
            .Select(lesson => _fixture.Build<LessonShortDto>()
                .With(x => x.Id, lesson.Id!.Value)
                .With(x => x.AcademicDisciplineId, lesson.AcademicDisciplineId)
                .With(x => x.AcademicDisciplineName, lesson.AcademicDiscipline?.Name)
                .With(x => x.AcademicDisciplineType, lesson.AcademicDisciplineType)
                .With(x => x.StudentGroups, lesson.StudentGroups.Select(x => new StudentGroupShortDto { Id = x.Id!.Value, Name = x.Name }).ToArray())
                .With(x => x.Teachers, lesson.Teachers.Select(x => new TeacherShortDto { Id = x.Id!.Value, Fullname = x.Fullname, Contacts = x.Contacts }).ToArray())
                .With(x => x.Rooms, lesson.Rooms.Select(x => new RoomShortDto { Id = x.Id!.Value, Name = x.Name }).ToArray())
                .With(x => x.DateWithTimeInterval, lesson.DateWithTimeInterval)
                .With(x => x.FlexibilityType, lesson.FlexibilityType)
                .With(x => x.AllowCombining, lesson.AllowCombining)
                .With(x => x.LessonPolicyViolationDescription, () => lesson.Violations.Length == 0
                    ? null
                    : lesson.Violations.Length == 1
                        ? lesson.Violations.First().Id.ToString()
                        : string.Format(LessonPolicyViolationTemplates.LessonPolicyViolationDefaultTemplate, lesson.Violations.Length))
                .With(x => x.CurrentErrorsMaxLevel, lesson.Violations.Length > 0 ? lesson.Violations.MaxBy(x => x.ErrorType)!.ErrorType : null)
                .Create())
            .ToArray();

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync(lessons);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync([new Teacher { Id = affectedByTeacherId, Fullname = _fixture.Create<string>() }]);

        var service = CreateService();

        // Act
        var actualLessons = await service.SearchWeekAsync(Guid.Empty, DateOnly.MinValue, DateOnly.MaxValue);

        // Assert
        Assert.Equal(expectedLessons.Length, actualLessons.Length);
        foreach (var expectedLesson in expectedLessons)
        {
            var actualLesson = actualLessons.Single(x => x.Id == expectedLesson.Id);
            if (Guid.TryParse(expectedLesson.LessonPolicyViolationDescription, out _))
            {
                expectedLesson.LessonPolicyViolationDescription = actualLesson.LessonPolicyViolationDescription = null;
            }
            Assert.Equivalent(expectedLesson, actualLesson);
        }
    }

    [Fact]
    public async Task GetLessonSeriesConflictsAsync_Should_Return_Valid_Result()
    {
        // Arrange
        var academicDisciplineId = Guid.NewGuid();
        var studentGroupId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var lessonBatchInfoId = Guid.NewGuid();
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, academicDisciplineId)
            .Without(x => x.AcademicDiscipline)
            .Without(x => x.AcademicDisciplineType)
            .With(x => x.StudentGroups, [new StudentGroup { Id = studentGroupId }])
            .With(x => x.Teachers, [new Teacher { Id = teacherId }])
            .Without(x => x.Rooms)
            .With(x => x.DateWithTimeInterval, _fixture.Create<DateWithTimeInterval>())
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .With(x => x.AllowCombining, true)
            .Without(x => x.HoursCost)
            .With(x => x.LessonBatchInfoId, lessonBatchInfoId)
            .With(x => x.LessonBatchInfo, _fixture.Build<LessonBatchInfo>()
                .With(x => x.Id, lessonBatchInfoId)
                .With(x => x.AcademicDisciplineId, academicDisciplineId)
                .Without(x => x.AcademicDiscipline)
                .With(x => x.StudentGroups, [new StudentGroup { Id = studentGroupId }])
                .With(x => x.Teachers, [new Teacher { Id = teacherId }])
                .Without(x => x.Rooms)
                .Without(x => x.LessonsPerWeekCount)
                .Without(x => x.DayOfWeekTimeIntervals)
                .Without(x => x.RepeatType)
                .With(x => x.DateInterval, new DateInterval
                {
                    DateFrom = DateTime.Today.AddDays(-7).ToDateOnly(),
                    DateTo = DateTime.Today.AddDays(7).ToDateOnly(),
                })
                .Without(x => x.AllowCombining)
                .Without(x => x.FlexibilityType)
                .Without(x => x.HoursCost)
                .Without(x => x.TotalHoursCount)
                .Create())
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(
                new[] { lesson.LessonBatchInfo!.StudentGroups.First().Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                {
                    lesson.LessonBatchInfo!.StudentGroups.First().Id!.Value,
                    [lesson.LessonBatchInfo.StudentGroups.First().Id!.Value]
                }
            });

        var conflictingLesson = new Lesson
        {
            Id = Guid.NewGuid(),
            StudentGroups = lesson.StudentGroups,
            DateWithTimeInterval = _fixture.Create<DateWithTimeInterval>(),
            FlexibilityType = LessonFlexibilityType.Fixed,
        };
        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync([conflictingLesson]);

        var conflictingTeacherPreference = new TeacherPreference
        {
            Id = Guid.NewGuid(),
            TeacherId = lesson.Teachers.First().Id!.Value,
            DayOfWeekTimeInterval = _fixture.Create<DayOfWeekTimeInterval>(),
            TeacherPreferenceType = TeacherPreferenceType.Undesirable,
        };
        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([conflictingTeacherPreference]);

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync(lesson.StudentGroups);

        _lessonRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync([conflictingLesson, lesson]);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync(lesson.Teachers);

        var service = CreateService();

        // Act
        var result = await service.GetLessonSeriesConflictsAsync(lesson);

        // Assert
        if (conflictingTeacherPreference.DayOfWeekTimeInterval.HasIntersection(new DayOfWeekTimeInterval
            {
                DayOfWeek = conflictingLesson.DateWithTimeInterval.Date.DayOfWeek,
                TimeInterval = conflictingLesson.DateWithTimeInterval.TimeInterval,
            }))
        {
            Assert.Single(result);
        }
        else
        {
            Assert.Equal(2, result.Length);
        }
    }

    [Fact]
    public async Task RecalculateConflictsForUpdatedAcademicDiscipline_Should_Produce_Validation_Messages()
    {
        // Arrange
        var academicDiscipline = _fixture.Build<AcademicDiscipline>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .Without(x => x.Name)
            .Without(x => x.AssociatedNames)
            .With(x => x.SemesterNumber, 5)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [AcademicDisciplineType.Lecture])
            .Without(x => x.LectureLessonBatchInfos)
            .Without(x => x.PracticeLessonBatchInfos)
            .Without(x => x.LabLessonBatchInfos)
            .Without(x => x.ExamLessonBatchInfos)
            .Without(x => x.TestLessonBatchInfos)
            .Without(x => x.Comment)
            .Create();

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(x =>
                x.ScheduleId == academicDiscipline.ScheduleId &&
                x.AcademicDisciplineId == academicDiscipline.Id!.Value)))
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroups = [new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = 6 }],
                    AcademicDisciplineType = AcademicDisciplineType.Lecture,
                },
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroups = [new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = academicDiscipline.SemesterNumber }],
                    AcademicDisciplineType = AcademicDisciplineType.Practice,
                },
            ]);

        var service = CreateService();

        var actualViolations = new List<LessonPolicyViolation>();
        _lessonPolicyViolationRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<LessonPolicyViolation[]>(), CancellationToken.None))
            .Callback<LessonPolicyViolation[], CancellationToken>((violations, _) => actualViolations.AddRange(violations));

        // Act
        await service.RecalculateConflictsForUpdatedAcademicDiscipline(academicDiscipline);

        // Assert
        Assert.Equal(2, actualViolations.Count);
        Assert.Contains(actualViolations,
            x => x.Code == LessonPolicyViolationCode.MismatchedSemesterNumber);
        Assert.Contains(actualViolations,
            x => x.Code == LessonPolicyViolationCode.MismatchedAcademicDisciplineType);
    }

    [Fact]
    public async Task UpdateAcademicDisciplineLessons_Should_Produce_New_Lessons_When_Discipline_Primary_Lessons_Save()
    {
        // Arrange
        var academicDisciplineId = Guid.NewGuid();
        var firstTimeInterval = new TimeInterval { TimeFrom = new TimeOnly(9, 0), TimeTo = new TimeOnly(10, 30) };
        var secondTimeInterval = new TimeInterval { TimeFrom = new TimeOnly(10, 0), TimeTo = new TimeOnly(11, 30) };
        var academicDiscipline = _fixture.Build<AcademicDiscipline>()
            .With(x => x.Id, academicDisciplineId)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.Name, _fixture.Create<string>())
            .With(x => x.SemesterNumber, 5)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [AcademicDisciplineType.Lecture])
            .With(x => x.LectureLessonBatchInfos, [_fixture
                .Build<LessonBatchInfo>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.AcademicDisciplineId, academicDisciplineId)
                .Without(x => x.AcademicDiscipline)
                .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
                .Without(x => x.Teachers)
                .Without(x => x.Rooms)
                .With(x => x.DayOfWeekTimeIntervals,
                [
                    new DayOfWeekTimeInterval { DayOfWeek = DayOfWeek.Tuesday, TimeInterval = firstTimeInterval },
                    new DayOfWeekTimeInterval { DayOfWeek = DayOfWeek.Thursday, TimeInterval = secondTimeInterval },
                ])
                .With(x => x.RepeatType, DisciplineLessonRepeatType.Weekly)
                .With(x => x.DateInterval,
                    // 4 недели занятий
                    new DateInterval
                    {
                        DateFrom = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(-7).ToDateOnly(),
                        DateTo = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(21).ToDateOnly(),
                    })
                .Without(x => x.AllowCombining)
                .With(x => x.HoursCost, _fixture.Create<int>())
                .With(x => x.TotalHoursCount, _fixture.Create<int>())
                .Create()])
            .Without(x => x.PracticeLessonBatchInfos)
            .Without(x => x.LabLessonBatchInfos)
            .Without(x => x.ExamLessonBatchInfos)
            .Without(x => x.TestLessonBatchInfos)
            .Without(x => x.Comment)
            .Create();

        _academicDisciplineRepositoryMock.Setup(r => r.GetAsync(academicDiscipline.Id!.Value, CancellationToken.None))
            .ReturnsAsync(academicDiscipline);

        _scheduleRepositoryMock.Setup(r => r.GetAsync(academicDiscipline.ScheduleId, CancellationToken.None))
            .ReturnsAsync(new Schedule
            {
                DateInterval = new DateInterval
                {
                    DateFrom = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(-7).ToDateOnly(),
                    DateTo = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(21).ToDateOnly(),
                }
            });

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(
                new[] { academicDiscipline.LectureLessonBatchInfos.First().StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync(academicDiscipline.LectureLessonBatchInfos.First().StudentGroups);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var actualLessons = new List<Lesson>();
        _lessonRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .Callback<Lesson[], CancellationToken>((lessons, _) =>
            {
                foreach (var lesson in lessons) lesson.Id = Guid.NewGuid();
                actualLessons.AddRange(lessons);
            });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync(() => actualLessons.ToArray());

        var actualViolations = new List<LessonPolicyViolation>();
        _lessonPolicyViolationRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<LessonPolicyViolation[]>(), CancellationToken.None))
            .Callback<LessonPolicyViolation[], CancellationToken>((violations, _) => actualViolations.AddRange(violations));

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(
                new[] { academicDiscipline.LectureLessonBatchInfos.First().StudentGroups.First().Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                {
                    academicDiscipline.LectureLessonBatchInfos.First().StudentGroups.First().Id!.Value,
                    [academicDiscipline.LectureLessonBatchInfos.First().StudentGroups.First().Id!.Value]
                }
            });

        _academicDisciplineRepositoryMock.Setup(r => r.SelectAsync(new[] { academicDiscipline.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([academicDiscipline]);

        _lessonRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<LessonConflictsSearchModel>()))
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroups = [new StudentGroup { Id = academicDiscipline.LectureLessonBatchInfos.First().StudentGroups.First().Id!.Value }],
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = DateTime.Today.ToDateOnly(),
                        TimeInterval = new TimeInterval
                        {
                            TimeFrom = new TimeOnly(11, 0),
                            TimeTo = new TimeOnly(15, 0),
                        },
                    },
                    Violations = [],
                }
            ]);

        var service = CreateService();

        // Act
        await service.UpdateLessonsByBatches(academicDiscipline.ScheduleId,
            academicDiscipline.LectureLessonBatchInfos,
            [academicDiscipline.LectureLessonBatchInfos.First().Id!.Value]);

        // Assert
        Assert.Equal(8, actualLessons.Count);
        Assert.Equal(4, actualLessons.Count(x => x.DateWithTimeInterval!.Date.DayOfWeek == DayOfWeek.Tuesday));
        Assert.Empty(actualLessons.Where(x => x.DateWithTimeInterval!.Date.DayOfWeek == DayOfWeek.Tuesday)
            .Select(x => x.DateWithTimeInterval!.TimeInterval)
            .ToHashSet()
            .ToArray()
            .Except([firstTimeInterval]));
        Assert.Equal(4, actualLessons.Count(x => x.DateWithTimeInterval!.Date.DayOfWeek == DayOfWeek.Thursday));
        Assert.Empty(actualLessons.Where(x => x.DateWithTimeInterval!.Date.DayOfWeek == DayOfWeek.Thursday)
            .Select(x => x.DateWithTimeInterval!.TimeInterval)
            .ToHashSet()
            .ToArray()
            .Except([secondTimeInterval]));
    }

    [Fact]
    public async Task UpdateAcademicDisciplineLessons_Should_Update_Previous_Academic_Discipline_Version_Lessons()
    {
        // Arrange
        var academicDisciplineId = Guid.NewGuid();
        var firstTimeInterval = new TimeInterval { TimeFrom = new TimeOnly(9, 0), TimeTo = new TimeOnly(10, 30) };
        var secondTimeInterval = new TimeInterval { TimeFrom = new TimeOnly(10, 0), TimeTo = new TimeOnly(11, 30) };
        var payloadFixture = new[]
        {
            _fixture
                .Build<LessonBatchInfo>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.AcademicDisciplineId, academicDisciplineId)
                .Without(x => x.AcademicDiscipline)
                .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
                .Without(x => x.Teachers)
                .Without(x => x.Rooms)
                .With(x => x.DayOfWeekTimeIntervals,
                [
                    new DayOfWeekTimeInterval { DayOfWeek = DayOfWeek.Tuesday, TimeInterval = firstTimeInterval },
                    new DayOfWeekTimeInterval { DayOfWeek = DayOfWeek.Thursday, TimeInterval = secondTimeInterval },
                ])
                .With(x => x.RepeatType, DisciplineLessonRepeatType.Weekly)
                .With(x => x.DateInterval,
                    // 4 недели занятий
                    new DateInterval
                    {
                        DateFrom = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(-7).ToDateOnly(),
                        DateTo = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(21).ToDateOnly(),
                    })
                .Without(x => x.AllowCombining)
                .With(x => x.HoursCost, _fixture.Create<int>())
                .With(x => x.TotalHoursCount, _fixture.Create<int>())
                .Create()
        };

        var academicDiscipline = _fixture.Build<AcademicDiscipline>()
            .With(x => x.Id, academicDisciplineId)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.Name, _fixture.Create<string>())
            .With(x => x.SemesterNumber, 5)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [AcademicDisciplineType.Practice, AcademicDisciplineType.Lab])
            .Without(x => x.LectureLessonBatchInfos)
            .With(x => x.PracticeLessonBatchInfos, payloadFixture)
            .With(x => x.LabLessonBatchInfos, payloadFixture)
            .Without(x => x.ExamLessonBatchInfos)
            .Without(x => x.TestLessonBatchInfos)
            .Without(x => x.Comment)
            .Create();

        _academicDisciplineRepositoryMock.Setup(r => r.GetAsync(academicDiscipline.Id!.Value, CancellationToken.None))
            .ReturnsAsync(academicDiscipline);

        _scheduleRepositoryMock.Setup(r => r.GetAsync(academicDiscipline.ScheduleId, CancellationToken.None))
            .ReturnsAsync(new Schedule
            {
                DateInterval = new DateInterval
                {
                    DateFrom = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(-7).ToDateOnly(),
                    DateTo = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(21).ToDateOnly(),
                }
            });

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(
                new[] { payloadFixture.First().StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync(payloadFixture.First().StudentGroups);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var actualLessons = new List<Lesson>();
        _lessonRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .Callback<Lesson[], CancellationToken>((lessons, _) =>
            {
                foreach (var lesson in lessons) lesson.Id = Guid.NewGuid();
                actualLessons.AddRange(lessons);
            });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(It.IsAny<Guid[]>(), CancellationToken.None))
            .ReturnsAsync(() => actualLessons.ToArray());

        var actualViolations = new List<LessonPolicyViolation>();
        _lessonPolicyViolationRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<LessonPolicyViolation[]>(), CancellationToken.None))
            .Callback<LessonPolicyViolation[], CancellationToken>((violations, _) => actualViolations.AddRange(violations));

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(
                new[] { payloadFixture.First().StudentGroups.First().Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                {
                    payloadFixture.First().StudentGroups.First().Id!.Value,
                    [payloadFixture.First().StudentGroups.First().Id!.Value]
                }
            });

        _academicDisciplineRepositoryMock.Setup(r => r.SelectAsync(new[] { academicDiscipline.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([academicDiscipline]);

        _lessonRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<LessonConflictsSearchModel>()))
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroups = [new StudentGroup { Id = payloadFixture.First().StudentGroups.First().Id!.Value }],
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = DateTime.Today.ToDateOnly(),
                        TimeInterval = new TimeInterval
                        {
                            TimeFrom = new TimeOnly(11, 0),
                            TimeTo = new TimeOnly(15, 0),
                        },
                    },
                    Violations = [],
                }
            ]);

        var service = CreateService();

        // Act
        await service.UpdateLessonsByBatches(academicDiscipline.ScheduleId,
            academicDiscipline.PracticeLessonBatchInfos.Concat(academicDiscipline.LabLessonBatchInfos).ToArray(),
            academicDiscipline.PracticeLessonBatchInfos.Concat(academicDiscipline.LabLessonBatchInfos).Select(x => x.Id!.Value).ToArray());

        // Assert
        Assert.Equal(16, actualLessons.Count);
        Assert.Equal(8, actualLessons.Count(x => x.AcademicDisciplineType == AcademicDisciplineType.Lab));
        Assert.Equal(8, actualLessons.Count(x => x.AcademicDisciplineType == AcademicDisciplineType.Practice));
    }

    [Fact]
    public async Task RecalculateConflictsForNewTeacherPreferences_Should_Produce_Validation_Messages()
    {
        // Arrange
        var teacherPreferences = new[]
        {
            _fixture.Build<TeacherPreference>()
                .Without(x => x.Id)
                .With(x => x.ScheduleId, Guid.NewGuid())
                .Without(x => x.Schedule)
                .With(x => x.TeacherId, Guid.NewGuid())
                .Without(x => x.Teacher)
                .Without(x => x.RoomId)
                .Without(x => x.Room)
                .With(x => x.DayOfWeekTimeInterval, new DayOfWeekTimeInterval
                {
                    DayOfWeek = DayOfWeek.Monday,
                    TimeInterval = new TimeInterval
                    {
                        TimeFrom = new TimeOnly(9, 0),
                        TimeTo = new TimeOnly(10, 30),
                    },
                })
                .With(x => x.TeacherPreferenceType, TeacherPreferenceType.Restricted)
                .Without(x => x.Comment)
                .Create(),
            _fixture.Build<TeacherPreference>()
                .Without(x => x.Id)
                .With(x => x.ScheduleId, Guid.NewGuid())
                .Without(x => x.Schedule)
                .With(x => x.TeacherId, Guid.NewGuid())
                .Without(x => x.Teacher)
                .With(x => x.RoomId, Guid.NewGuid())
                .Without(x => x.Room)
                .Without(x => x.DayOfWeekTimeInterval)
                .With(x => x.TeacherPreferenceType, TeacherPreferenceType.Restricted)
                .Without(x => x.Comment)
                .Create(),
        };

        _lessonRepositoryMock
            .SetupSequence(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync([
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = new DateOnly(2026, 9, 7),
                        TimeInterval = teacherPreferences.First().DayOfWeekTimeInterval!.TimeInterval,
                    },
                }
            ])
            .ReturnsAsync([new Lesson
            {
                Id = Guid.NewGuid(),
                Rooms = [new Room { Id = teacherPreferences.Last().RoomId}],
            }]);

        var service = CreateService();

        var actualViolations = new List<LessonPolicyViolation>();
        _lessonPolicyViolationRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<LessonPolicyViolation[]>(), CancellationToken.None))
            .Callback<LessonPolicyViolation[], CancellationToken>((violations, _) => actualViolations.AddRange(violations));

        // Act
        await service.RecalculateConflictsForNewTeacherPreferences(teacherPreferences);

        // Assert
        Assert.Equal(2, actualViolations.Count);
        Assert.Contains(actualViolations,
            x => x.Code == LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict);
        Assert.Contains(actualViolations,
            x => x.Code == LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict);
    }

    [Fact]
    public async Task RecalculateConflictsForNewStudentGroup_Should_Produce_Validation_Messages()
    {
        // Arrange
        var firstStudentGroupId = Guid.NewGuid();
        var secondStudentGroupId = Guid.NewGuid();

        var firstExpectedLessonId = Guid.NewGuid();
        var secondExpectedLessonId = Guid.NewGuid();
        var thirdExpectedLessonId = Guid.NewGuid();

        var studentGroup = _fixture.Build<StudentGroup>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .Without(x => x.Name)
            .With(x => x.SemesterNumber, 5)
            .Without(x => x.StudentGroupType)
            .Without(x => x.Parents)
            .Without(x => x.Children)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(new[] { studentGroup.Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { studentGroup.Id!.Value, [studentGroup.Id!.Value, firstStudentGroupId, secondStudentGroupId] }
            });

        _lessonRepositoryMock.SetupSequence(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync([])
            .ReturnsAsync([
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    AcademicDiscipline = new AcademicDiscipline { Id = Guid.NewGuid(), SemesterNumber = 6 },
                },
                new Lesson
                {
                    Id = firstExpectedLessonId,
                    StudentGroups = [new StudentGroup { Id = firstStudentGroupId }],
                    DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = new DateOnly(2026, 9, 7),
                        TimeInterval = new TimeInterval
                        {
                            TimeFrom = new TimeOnly(9, 0),
                            TimeTo = new TimeOnly(10, 30),
                        },
                    },
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AcademicDiscipline = new AcademicDiscipline
                    {
                        Id = Guid.NewGuid(),
                        SemesterNumber = studentGroup.SemesterNumber,
                    }
                },
                new Lesson
                {
                    Id = secondExpectedLessonId,
                    StudentGroups = [new StudentGroup { Id = studentGroup.Id!.Value }],
                    DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = new DateOnly(2026, 9, 7),
                        TimeInterval = new TimeInterval
                        {
                            TimeFrom = new TimeOnly(10, 0),
                            TimeTo = new TimeOnly(11, 30),
                        },
                    },
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AcademicDiscipline = new AcademicDiscipline
                    {
                        Id = Guid.NewGuid(),
                        SemesterNumber = studentGroup.SemesterNumber,
                    }
                },
                new Lesson
                {
                    Id = thirdExpectedLessonId,
                    StudentGroups = [new StudentGroup { Id = secondStudentGroupId }],
                    DateWithTimeInterval = new DateWithTimeInterval
                    {
                        Date = new DateOnly(2026, 9, 7),
                        TimeInterval = new TimeInterval
                        {
                            TimeFrom = new TimeOnly(11, 0),
                            TimeTo = new TimeOnly(12, 30),
                        },
                    },
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AcademicDiscipline = new AcademicDiscipline
                    {
                        Id = Guid.NewGuid(),
                        SemesterNumber = studentGroup.SemesterNumber,
                    }
                },
            ]);

        var service = CreateService();

        var actualViolations = new List<LessonPolicyViolation>();
        _lessonPolicyViolationRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<LessonPolicyViolation[]>(), CancellationToken.None))
            .Callback<LessonPolicyViolation[], CancellationToken>((violations, _) => actualViolations.AddRange(violations));

        // Act
        await service.RecalculateConflictsForNewStudentGroup(studentGroup);

        // Assert
        Assert.Equal(5, actualViolations.Count);

        Assert.Contains(actualViolations,
            x => x.Code == LessonPolicyViolationCode.MismatchedSemesterNumber);
        Assert.Contains(actualViolations, x =>
            x.Code == LessonPolicyViolationCode.FixedLessonTypeConflictByGroup &&
            x.Payload.AffectedByLessonId == secondExpectedLessonId);
        Assert.Contains(actualViolations, x =>
            x.Code == LessonPolicyViolationCode.FixedLessonTypeConflictByGroup &&
            x.Payload.AffectedByLessonId == firstExpectedLessonId);
        Assert.Contains(actualViolations, x =>
            x.Code == LessonPolicyViolationCode.FixedLessonTypeConflictByGroup &&
            x.Payload.AffectedByLessonId == thirdExpectedLessonId);
        Assert.Contains(actualViolations, x =>
            x.Code == LessonPolicyViolationCode.FixedLessonTypeConflictByGroup &&
            x.Payload.AffectedByLessonId == secondExpectedLessonId);
    }
}