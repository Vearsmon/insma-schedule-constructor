using AutoFixture;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonPolicyViolations;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Exceptions;
using Domain.Helpers;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.SearchModels;
using Moq;
using Services;

namespace Tests;

public class LessonValidationServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IStudentGroupRepository> _studentGroupRepositoryMock = new();
    private readonly Mock<ITeacherRepository> _teacherRepositoryMock = new();
    private readonly Mock<ITeacherPreferenceRepository> _teacherPreferenceRepositoryMock = new();
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<IAcademicDisciplineRepository> _academicDisciplineRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<ILessonRepository> _lessonRepositoryMock = new();
    private readonly Mock<ILessonPolicyViolationRepository> _lessonPolicyViolationRepositoryMock = new();

    private LessonValidationService CreateService() => new(
        _lessonRepositoryMock.Object,
        _lessonPolicyViolationRepositoryMock.Object,
        _scheduleRepositoryMock.Object,
        _teacherRepositoryMock.Object,
        _academicDisciplineRepositoryMock.Object,
        _roomRepositoryMock.Object,
        _studentGroupRepositoryMock.Object,
        _teacherPreferenceRepositoryMock.Object
    );

    [Fact]
    public async Task ValidateAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var lessonToSaveFixture = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .Without(x => x.Schedule)
            .Without(x => x.AcademicDisciplineId)
            .Without(x => x.AcademicDiscipline)
            .Without(x => x.AcademicDisciplineType)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .With(x => x.Teachers, [new Teacher { Id = Guid.NewGuid() }])
            .With(x => x.Rooms, [new Room { Id = Guid.NewGuid() }])
            .Without(x => x.DateWithTimeInterval)
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations);
        var lessonToSave = lessonToSaveFixture
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray(), CancellationToken.None))
            .ReturnsAsync([new StudentGroup()]);

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lessonToSaveFixture
                .With(x => x.ScheduleId, Guid.NewGuid())
                .Create()]);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(false);

        var service = CreateService();
        var serviceFunc = () => service.ValidateAsync([lessonToSave]);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(5, actualException.ValidationMessages.Length);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_MismatchedSemesterNumber_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = lessonToSave.StudentGroups.First().Id!.Value, SemesterNumber = _fixture.Create<int>() }]);

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { lessonToSave.StudentGroups.First().Id!.Value, [lessonToSave.StudentGroups.First().Id!.Value] }
            });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lessonToSave]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.SelectAsync(new[] { lessonToSave.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([new AcademicDiscipline
            {
                Id = lessonToSave.AcademicDisciplineId!.Value,
                SemesterNumber = _fixture.Create<int>(),
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            }]);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.MismatchedSemesterNumber, violations.First().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_MismatchedAcademicDisciplineType_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lab)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = lessonToSave.StudentGroups.First().Id!.Value, SemesterNumber = semesterNumber }]);

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { lessonToSave.StudentGroups.First().Id!.Value, [lessonToSave.StudentGroups.First().Id!.Value] }
            });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lessonToSave]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.SelectAsync(new[] { lessonToSave.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([new AcademicDiscipline { Id = lessonToSave.AcademicDisciplineId!.Value, SemesterNumber = semesterNumber }]);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.MismatchedAcademicDisciplineType, violations.First().Code);
    }

    [Fact]
    public async Task FillValidationMessages_Should_Produce_MismatchedSemesterNumber_Validation_Code()
    {
        // Arrange
        var academicDiscipline = new AcademicDiscipline
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
            SemesterNumber = _fixture.Create<int>(),
            AllowedLessonTypes = [AcademicDisciplineType.Lecture],
        };
        var studentGroup = new StudentGroup
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
            SemesterNumber = _fixture.Create<int>(),
        };
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, academicDiscipline.Id)
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, academicDiscipline.AllowedLessonTypes.First())
            .With(x => x.StudentGroups, [studentGroup])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval
                {
                    Date = new DateOnly(2026, 9, 7),
                    TimeInterval = new TimeInterval
                    {
                        TimeFrom = new TimeOnly(9, 0),
                        TimeTo = new TimeOnly(10, 30),
                    },
                })
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        var studentGroupIds = lesson.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lesson.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson]);

        _academicDisciplineRepositoryMock
            .Setup(x => x.SelectAsync(new[] { lesson.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([academicDiscipline]);

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { studentGroup.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([studentGroup]);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lesson.ScheduleId))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lesson]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.MismatchedSemesterNumber, violations.First().Code);
    }

    [Fact]
    public async Task FillValidationMessages_Should_Produce_MismatchedAcademicDisciplineType_Validation_Code()
    {
        // Arrange
        var academicDiscipline = new AcademicDiscipline
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
            AllowedLessonTypes = [AcademicDisciplineType.Lecture],
        };
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, academicDiscipline.Id)
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Practice)
            .Without(x => x.StudentGroups)
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval
                {
                    Date = new DateOnly(2026, 9, 7),
                    TimeInterval = new TimeInterval
                    {
                        TimeFrom = new TimeOnly(9, 0),
                        TimeTo = new TimeOnly(10, 30),
                    },
                })
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>());

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson]);

        _academicDisciplineRepositoryMock
            .Setup(x => x.SelectAsync(new[] { lesson.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([academicDiscipline]);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lesson.ScheduleId))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lesson]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.MismatchedAcademicDisciplineType, violations.First().Code);
    }

    [Theory]
    [InlineData(LessonPolicyViolationCode.FixedLessonTypeConflictByGroup)]
    [InlineData(LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup)]
    [InlineData(LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher)]
    [InlineData(LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher)]
    [InlineData(LessonPolicyViolationCode.FixedLessonTypeConflictByRoom)]
    [InlineData(LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom)]
    public async Task FillValidationMessages_Should_Produce_LessonTypeConflict_Validation_Code(
        LessonPolicyViolationCode policyViolationCode)
    {
        // Arrange
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .With(x => x.Teachers,
                policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher
                    or LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher
                    ? [new Teacher { Id = Guid.NewGuid() }] : [])
            .With(x => x.Rooms,
                policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByRoom
                    or LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom
                    ? [new Room { Id = Guid.NewGuid() }] : [])
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval
                {
                    Date = DateTime.Today.ToDateOnly(),
                    TimeInterval = new TimeInterval
                    {
                        TimeFrom = new TimeOnly(9, 0),
                        TimeTo = new TimeOnly(12, 00),
                    },
                })
            .With(x => x.FlexibilityType,
                policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByGroup
                or LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher
                or LessonPolicyViolationCode.FixedLessonTypeConflictByRoom
                ? LessonFlexibilityType.Fixed : LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = lesson.StudentGroups.First().Id!.Value, SemesterNumber = semesterNumber }]);

        if (lesson.Rooms.Length > 0)
        {
            _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Rooms.First().Id!.Value }, CancellationToken.None))
                .ReturnsAsync([new Room { Id = lesson.Rooms.First().Id!.Value }]);
        }

        if (lesson.Teachers.Length > 0)
        {
            _teacherRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Teachers.First().Id!.Value }, CancellationToken.None))
                .ReturnsAsync([new Teacher { Id = lesson.Teachers.First().Id!.Value }]);
        }

        var studentGroupIds = lesson.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lesson.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.SelectAsync(new[] { lesson.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([new AcademicDiscipline
            {
                Id = lesson.AcademicDisciplineId!.Value,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            }]);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lesson.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<LessonConflictsSearchModel>()))
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroups = policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByGroup
                        or LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup
                        ? [new StudentGroup { Id = lesson.StudentGroups.First().Id!.Value }] : [],
                    Teachers = policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher
                        or LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher
                        ? [new Teacher { Id = lesson.Teachers.First().Id!.Value }] : [],
                    Rooms = policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByRoom
                        or LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom
                        ? [new Room { Id = lesson.Rooms.First().Id!.Value }] : [],
                    FlexibilityType = policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByGroup
                        or LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher
                        or LessonPolicyViolationCode.FixedLessonTypeConflictByRoom
                        ? LessonFlexibilityType.Flexible : LessonFlexibilityType.Fixed,
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

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lesson]);

        // Assert
        Assert.Equal(2, violations.Length);
        Assert.Equal(policyViolationCode, violations.Last().Code);
    }

    [Theory]
    [InlineData(LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict)]
    [InlineData(LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict)]
    [InlineData(LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict)]
    [InlineData(LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict)]
    public async Task FillValidationMessages_Should_Produce_TeacherPreferenceTypeConflict_Validation_Code_Message(
        LessonPolicyViolationCode policyViolationCode)
    {
        // Arrange
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .With(x => x.Teachers, [new Teacher { Id = Guid.NewGuid() }])
            .With(x => x.Rooms, [new Room { Id = Guid.NewGuid() }])
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
                {
                    Date = DateTime.Today.ToDateOnly(),
                    TimeInterval = new TimeInterval
                    {
                        TimeFrom = new TimeOnly(9, 0),
                        TimeTo = new TimeOnly(12, 0),
                    },
                })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = lesson.StudentGroups.First().Id!.Value, SemesterNumber = semesterNumber }]);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Teachers.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Teacher { Id = lesson.Teachers.First().Id!.Value }]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Rooms.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Room { Id = lesson.Rooms.First().Id!.Value }]);

        var studentGroupIds = lesson.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lesson.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.SelectAsync(new[] { lesson.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([new AcademicDiscipline
            {
                Id = lesson.AcademicDisciplineId!.Value,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            }]);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lesson.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<LessonConflictsSearchModel>()))
            .ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<TeacherPreferenceConflictsSearchModel>()))
            .ReturnsAsync([new TeacherPreference
            {
                Id = Guid.NewGuid(),
                TeacherId = lesson.Teachers.First().Id!.Value,
                RoomId = policyViolationCode is LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict
                    or LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict
                    ? lesson.Rooms.First().Id!.Value : null,
                DayOfWeekTimeInterval = policyViolationCode is LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict
                    or LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict
                    ? new DayOfWeekTimeInterval
                    {
                        DayOfWeek = DateTime.Today.ToDateOnly().DayOfWeek,
                        TimeInterval = new TimeInterval
                        {
                            TimeFrom = new TimeOnly(11, 0),
                            TimeTo = new TimeOnly(15, 0),
                        },
                    }: null,
                TeacherPreferenceType = policyViolationCode is LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict
                    or LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict
                    ? TeacherPreferenceType.Undesirable : TeacherPreferenceType.Restricted,
            }]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lesson]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(policyViolationCode, violations.First().Code);
    }

    // [Fact]
    // public async Task FillValidationMessages_Should_Produce_MismatchedAcademicDisciplineTypeTotalHoursCount_Validation_Code_Message()
    // {
    //     // Arrange
    //     var expectedTotalHoursCount = _fixture.Create<int>();
    //     var actualTotalHoursCount = _fixture.Create<int>();
    //     var academicDisciplineName = _fixture.Create<string>();
    //     var lesson = _fixture.Build<Lesson>()
    //         .With(x => x.Id, Guid.NewGuid())
    //         .With(x => x.ScheduleId, Guid.NewGuid())
    //         .Without(x => x.Schedule)
    //         .With(x => x.AcademicDisciplineId, Guid.NewGuid())
    //         .Without(x => x.AcademicDiscipline)
    //         .Without(x => x.AcademicDisciplineType)
    //         .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
    //         .Without(x => x.Teachers)
    //         .Without(x => x.Rooms)
    //         .With(x => x.DateWithTimeInterval,
    //             new DateWithTimeInterval
    //             {
    //                 Date = new DateOnly(2026, 9, 7),
    //                 TimeInterval = new TimeInterval
    //                 {
    //                     TimeFrom = new TimeOnly(9, 0),
    //                     TimeTo = new TimeOnly(10, 30),
    //                 },
    //             })
    //         .Without(x => x.FlexibilityType)
    //         .Without(x => x.AllowCombining)
    //         .Without(x => x.HoursCost)
    //         .Without(x => x.LessonBatchInfoId)
    //         .Without(x => x.LessonBatchInfo)
    //         .Without(x => x.Violations)
    //         .Create();
    //
    //     _lessonRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<LessonSearchModel>()))
    //         .ReturnsAsync([
    //             new Lesson
    //             {
    //                 StudentGroups = lesson.StudentGroups,
    //                 AcademicDisciplineType = AcademicDisciplineType.Lecture,
    //                 HoursCost = actualTotalHoursCount,
    //             }
    //         ]);
    //
    //     _academicDisciplineRepositoryMock
    //         .Setup(x => x.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByAcademicDisciplineId!.Value },
    //             CancellationToken.None))
    //         .ReturnsAsync([new AcademicDiscipline
    //         {
    //             Id = lesson.Violations.First().Payload.AffectedByAcademicDisciplineId!.Value,
    //             Name = academicDisciplineName,
    //             LectureLessonBatchInfos = [new LessonBatchInfo { TotalHoursCount = expectedTotalHoursCount }],
    //         }]);
    //
    //     _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByStudentGroupId!.Value }, CancellationToken.None))
    //         .ReturnsAsync([lesson.StudentGroups.First()]);
    //
    //     var service = CreateService();
    //
    //     // Act
    //     var actualMessages = await service.FillValidationMessages([lesson]);
    //
    //     // Assert
    //     Assert.Single(actualMessages);
    //     Assert.Equal(
    //         string.Format(LessonPolicyViolationTemplates.MismatchedAcademicDisciplineTypeTotalHoursCountTemplate,
    //             AcademicDisciplineType.Lecture.GetDescription(), academicDisciplineName, actualTotalHoursCount,
    //             expectedTotalHoursCount, lesson.StudentGroups.First().Name),
    //         actualMessages.First().Messages.First().Message);
    // }
}