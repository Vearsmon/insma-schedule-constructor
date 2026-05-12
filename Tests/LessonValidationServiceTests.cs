using AutoFixture;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonPolicyViolations;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Constants;
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

        _lessonRepositoryMock.Setup(r => r.GetAsync(lessonToSave.Id!.Value, CancellationToken.None))
            .ReturnsAsync(lessonToSaveFixture
                .With(x => x.ScheduleId, Guid.NewGuid())
                .Create());

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
            .Without(x => x.Id)
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
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = _fixture.Create<int>() }]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = _fixture.Create<int>(),
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

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
            .Without(x => x.Id)
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
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline { Id = Guid.NewGuid(), SemesterNumber = semesterNumber });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.MismatchedAcademicDisciplineType, violations.First().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_FixedLessonTypeConflictByGroup_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
                {
                    Date = DateTime.Today.ToDateOnly(),
                    TimeInterval = new TimeInterval
                    {
                        TimeFrom = new TimeOnly(9, 0),
                        TimeTo = new TimeOnly(12, 0),
                    },
                })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([
            new Lesson
            {
                Id = Guid.NewGuid(),
                StudentGroups = [new StudentGroup { Id = lessonToSave.StudentGroups.First().Id!.Value }],
                FlexibilityType = LessonFlexibilityType.Fixed,
                Violations = [],
            }
        ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations= await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Equal(2, violations.Length);
        Assert.Equal(LessonPolicyViolationCode.FixedLessonTypeConflictByGroup, violations.First().Code);
        Assert.Equal(LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup, violations.Last().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_FlexibleLessonTypeConflictByGroup_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
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

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([
            new Lesson
            {
                Id = Guid.NewGuid(),
                StudentGroups = [new StudentGroup { Id = lessonToSave.StudentGroups.First().Id!.Value }],
                FlexibilityType = LessonFlexibilityType.Flexible,
                Violations = [],
            }
        ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Equal(2, violations.Length);
        Assert.Equal(LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup, violations.First().Code);
        Assert.Equal(LessonPolicyViolationCode.FixedLessonTypeConflictByGroup, violations.Last().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_RestrictedTimeTeacherPreferenceTypeConflict_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
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

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x =>
                    x.TimeInterval == lessonToSave.DateWithTimeInterval!.TimeInterval)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval
                    {
                        DayOfWeek = lessonToSave.DateWithTimeInterval!.Date.DayOfWeek,
                        TimeInterval = lessonToSave.DateWithTimeInterval.TimeInterval,
                    },
                    TeacherPreferenceType = TeacherPreferenceType.Restricted,
                }
            ]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict, violations.First().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_UndesirableTimeTeacherPreferenceTypeConflict_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
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

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x =>
                    x.TimeInterval == lessonToSave.DateWithTimeInterval!.TimeInterval)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval
                    {
                        DayOfWeek = lessonToSave.DateWithTimeInterval!.Date.DayOfWeek,
                        TimeInterval = lessonToSave.DateWithTimeInterval.TimeInterval,
                    },
                    TeacherPreferenceType = TeacherPreferenceType.Undesirable,
                }
            ]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict, violations.First().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_RestrictedRoomTeacherPreferenceTypeConflict_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
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

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Teachers.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Teacher()]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Rooms.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Room()]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x =>
                    x.RoomIds.Length == 1)))
            .ReturnsAsync([new TeacherPreference
            {
                Id = Guid.NewGuid(),
                RoomId = lessonToSave.Rooms.First().Id!.Value,
                TeacherPreferenceType = TeacherPreferenceType.Restricted,
            }]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict, violations.First().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_UndesirableRoomTeacherPreferenceTypeConflict_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
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

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Teachers.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Teacher()]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Rooms.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Room()]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x =>
                    x.RoomIds.Length == 1)))
            .ReturnsAsync([new TeacherPreference
            {
                Id = Guid.NewGuid(),
                RoomId = lessonToSave.Rooms.First().Id!.Value,
                TeacherPreferenceType = TeacherPreferenceType.Undesirable,
            }]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict, violations.First().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_FixedLessonTypeConflictByRoom_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
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

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        _roomRepositoryMock.Setup(r =>
                r.SelectAsync(new[] { lessonToSave.Rooms.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Room()]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.SetupSequence(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    Rooms = [new Room { Id = lessonToSave.Rooms.First().Id!.Value }],
                    FlexibilityType = LessonFlexibilityType.Flexible,
                    Violations = [],
                }
            ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Equal(2, violations.Length);
        Assert.Equal(LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom, violations.First().Code);
        Assert.Equal(LessonPolicyViolationCode.FixedLessonTypeConflictByRoom, violations.Last().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_FlexibleLessonTypeConflictByRoom_Validation_Code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.Teachers)
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
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.LessonBatchInfoId)
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup { Id = Guid.NewGuid(), SemesterNumber = semesterNumber }]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Rooms.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new Room()]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.SetupSequence(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    Rooms = [new Room { Id = lessonToSave.Rooms.First().Id!.Value }],
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    Violations = [],
                }
            ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Equal(2, violations.Length);
        Assert.Equal(LessonPolicyViolationCode.FixedLessonTypeConflictByRoom, violations.First().Code);
        Assert.Equal(LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom, violations.Last().Code);
    }

    [Fact]
    public async Task FillValidationMessages_Should_Produce_MismatchedSemesterNumber_Validation_Code_Message()
    {
        // Arrange
        var academicDiscipline = new AcademicDiscipline
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
            SemesterNumber = _fixture.Create<int>(),
        };
        var studentGroup = new StudentGroup
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
            SemesterNumber = _fixture.Create<int>(),
        };
        var lesson = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, academicDiscipline.Id)
            .Without(x => x.AcademicDiscipline)
            .Without(x => x.AcademicDisciplineType)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
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
            .With(x => x.Violations,
            [
                new LessonPolicyViolation
                {
                    Code = LessonPolicyViolationCode.MismatchedSemesterNumber,
                    Payload = new LessonValidationPayload
                    {
                        AffectedByAcademicDisciplineId = academicDiscipline.Id,
                        AffectedByStudentGroupId = studentGroup.Id,
                    }
                }
            ])
            .Create();

        _academicDisciplineRepositoryMock
            .Setup(x => x.SelectAsync(new[] { lesson.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([academicDiscipline]);

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { studentGroup.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([studentGroup]);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(LessonPolicyViolationTemplates.MismatchedSemesterNumberTemplate, academicDiscipline.Name,
                studentGroup.Name, studentGroup.SemesterNumber, academicDiscipline.SemesterNumber),
            actualMessages.First().Messages.First().Message);
    }

    [Fact]
    public async Task FillValidationMessages_Should_Produce_MismatchedAcademicDisciplineType_Validation_Code_Message()
    {
        // Arrange
        var academicDiscipline = new AcademicDiscipline
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
        };
        var lesson = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, academicDiscipline.Id)
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType)
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
            .With(x => x.Violations,
            [
                new LessonPolicyViolation
                {
                    Code = LessonPolicyViolationCode.MismatchedAcademicDisciplineType,
                    Payload = new LessonValidationPayload { AffectedByAcademicDisciplineId = academicDiscipline.Id }
                }
            ])
            .Create();

        _academicDisciplineRepositoryMock
            .Setup(x => x.SelectAsync(new[] { lesson.AcademicDisciplineId!.Value }, CancellationToken.None))
            .ReturnsAsync([academicDiscipline]);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(LessonPolicyViolationTemplates.MismatchedAcademicDisciplineTypeTemplate,
                academicDiscipline.Name,
                lesson.AcademicDisciplineType!.Value.GetDescription()),
            actualMessages.First().Messages.First().Message);
    }

    [Theory]
    [InlineData(false, LessonPolicyViolationCode.FixedLessonTypeConflictByGroup)]
    [InlineData(true, LessonPolicyViolationCode.FixedLessonTypeConflictByGroup)]
    [InlineData(false, LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup)]
    [InlineData(true, LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup)]
    [InlineData(false, LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher)]
    [InlineData(true, LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher)]
    [InlineData(false, LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher)]
    [InlineData(true, LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher)]
    [InlineData(false, LessonPolicyViolationCode.FixedLessonTypeConflictByRoom)]
    [InlineData(true, LessonPolicyViolationCode.FixedLessonTypeConflictByRoom)]
    [InlineData(false, LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom)]
    [InlineData(true, LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom)]
    public async Task FillValidationMessages_Should_Produce_LessonTypeConflict_Validation_Code_Message(
        bool withAcademicDiscipline, LessonPolicyViolationCode policyViolationCode)
    {
        // Arrange
        var studentGroupName = _fixture.Create<string>();
        var linkedEntityName = _fixture.Create<string>();
        var academicDiscipline = withAcademicDiscipline
            ? new AcademicDiscipline { Name = _fixture.Create<string>() }
            : null;
        var lesson = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .Without(x => x.ScheduleId)
            .Without(x => x.Schedule)
            .Without(x => x.AcademicDisciplineId)
            .Without(x => x.AcademicDiscipline)
            .Without(x => x.AcademicDisciplineType)
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
            .With(x => x.Violations,
            [
                new LessonPolicyViolation
                {
                    Code = policyViolationCode,
                    Payload = new LessonValidationPayload
                    {
                        AffectedByLessonId = Guid.NewGuid(),
                        AffectedByStudentGroupId = Guid.NewGuid(),
                        AffectedByRoomId = Guid.NewGuid(),
                        AffectedByTeacherId = Guid.NewGuid(),
                    }
                }
            ])
            .Create();

        _lessonRepositoryMock
            .Setup(x => x.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByLessonId!.Value },
                CancellationToken.None))
            .ReturnsAsync([new Lesson
            {
                Id = lesson.Violations.First().Payload.AffectedByLessonId!.Value,
                AcademicDiscipline = withAcademicDiscipline ? academicDiscipline : null,
            }]);

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByStudentGroupId!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = lesson.Violations.First().Payload.AffectedByStudentGroupId!.Value,
                Name = policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByGroup
                    or LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup
                    ? linkedEntityName
                    : studentGroupName
            }]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByRoomId!.Value }, CancellationToken.None))
            .ReturnsAsync([new Room
            {
                Id = lesson.Violations.First().Payload.AffectedByRoomId!.Value,
                Name = linkedEntityName,
            }]);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByTeacherId!.Value }, CancellationToken.None))
            .ReturnsAsync([new Teacher
            {
                Id = lesson.Violations.First().Payload.AffectedByTeacherId!.Value,
                Fullname = linkedEntityName,
            }]);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(GetMessageTemplate(policyViolationCode),
                academicDiscipline != null ? academicDiscipline.Name : string.Empty,
                linkedEntityName),
            actualMessages.First().Messages.First().Message);

        return;

        string GetMessageTemplate(LessonPolicyViolationCode code)
        {
            return code switch
            {
                LessonPolicyViolationCode.FixedLessonTypeConflictByGroup => LessonPolicyViolationTemplates
                    .FixedLessonTypeConflictByGroupTemplate,
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup => LessonPolicyViolationTemplates
                    .FlexibleLessonTypeConflictByGroupTemplate,
                LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher => LessonPolicyViolationTemplates
                    .FixedLessonTypeConflictByTeacherTemplate,
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher => LessonPolicyViolationTemplates
                    .FlexibleLessonTypeConflictByTeacherTemplate,
                LessonPolicyViolationCode.FixedLessonTypeConflictByRoom => LessonPolicyViolationTemplates
                    .FixedLessonTypeConflictByRoomTemplate,
                LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom => LessonPolicyViolationTemplates
                    .FlexibleLessonTypeConflictByRoomTemplate,
                _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
            };
        }
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
        var teacherFullname = _fixture.Create<string>();
        var lesson = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .Without(x => x.ScheduleId)
            .Without(x => x.Schedule)
            .Without(x => x.AcademicDisciplineId)
            .Without(x => x.AcademicDiscipline)
            .Without(x => x.AcademicDisciplineType)
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
            .With(x => x.Violations,
            [
                new LessonPolicyViolation
                {
                    Code = policyViolationCode,
                    Payload = new LessonValidationPayload { AffectedByTeacherId = Guid.NewGuid() }
                }
            ])
            .Create();

        _teacherRepositoryMock
            .Setup(x => x.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByTeacherId!.Value },
                CancellationToken.None))
            .ReturnsAsync([new Teacher
            {
                Id = lesson.Violations.First().Payload.AffectedByTeacherId!.Value,
                Fullname = teacherFullname,
            }]);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(GetMessageTemplate(policyViolationCode), teacherFullname),
            actualMessages.First().Messages.First().Message);

        return;

        string GetMessageTemplate(LessonPolicyViolationCode code)
        {
            return code switch
            {
                LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict => LessonPolicyViolationTemplates
                    .RestrictedTimeTeacherPreferenceTypeConflictTemplate,
                LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict => LessonPolicyViolationTemplates
                    .UndesirableTimeTeacherPreferenceTypeConflictTemplate,
                LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict => LessonPolicyViolationTemplates
                    .RestrictedRoomTeacherPreferenceTypeConflictTemplate,
                LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict => LessonPolicyViolationTemplates
                    .UndesirableRoomTeacherPreferenceTypeConflictTemplate,
                _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
            };
        }
    }

    [Fact]
    public async Task FillValidationMessages_Should_Produce_MismatchedAcademicDisciplineTypeTotalHoursCount_Validation_Code_Message()
    {
        // Arrange
        var expectedTotalHoursCount = _fixture.Create<int>();
        var actualTotalHoursCount = _fixture.Create<int>();
        var academicDisciplineName = _fixture.Create<string>();
        var studentGroupId = Guid.NewGuid();
        var lesson = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .Without(x => x.AcademicDisciplineType)
            .With(x => x.StudentGroups,
                [new StudentGroup { Id = studentGroupId, Name = _fixture.Create<string>() }])
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
            .With(x => x.Violations,
            [
                new LessonPolicyViolation
                {
                    Code = LessonPolicyViolationCode.MismatchedAcademicDisciplineTypeTotalHoursCount,
                    Payload = new LessonValidationPayload
                    {
                        AffectedByAcademicDisciplineId = Guid.NewGuid(),
                        AffectedByAcademicDisciplineType = AcademicDisciplineType.Lecture,
                        AffectedByStudentGroupId = studentGroupId,
                        AffectedByStudentGroup = new StudentGroup { Name = _fixture.Create<string>() },
                    }
                }
            ])
            .Create();

        _lessonRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync([
                new Lesson
                {
                    StudentGroups = lesson.StudentGroups,
                    AcademicDisciplineType = AcademicDisciplineType.Lecture,
                    HoursCost = actualTotalHoursCount,
                }
            ]);

        _academicDisciplineRepositoryMock
            .Setup(x => x.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByAcademicDisciplineId!.Value },
                CancellationToken.None))
            .ReturnsAsync([new AcademicDiscipline
            {
                Id = lesson.Violations.First().Payload.AffectedByAcademicDisciplineId!.Value,
                Name = academicDisciplineName,
                LectureLessonBatchInfos = [new LessonBatchInfo { TotalHoursCount = expectedTotalHoursCount }],
            }]);

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Violations.First().Payload.AffectedByStudentGroupId!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson.StudentGroups.First()]);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(LessonPolicyViolationTemplates.MismatchedAcademicDisciplineTypeTotalHoursCountTemplate,
                AcademicDisciplineType.Lecture.GetDescription(), academicDisciplineName, actualTotalHoursCount,
                expectedTotalHoursCount, lesson.StudentGroups.First().Name),
            actualMessages.First().Messages.First().Message);
    }
}