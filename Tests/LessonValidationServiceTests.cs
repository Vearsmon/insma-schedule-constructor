using AutoFixture;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonValidationMessages;
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
    private readonly Mock<ILessonValidationMessageRepository> _lessonValidationMessageRepositoryMock = new();

    private LessonValidationService CreateService() => new(
        _lessonRepositoryMock.Object,
        _lessonValidationMessageRepositoryMock.Object,
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .With(x => x.RoomId, Guid.NewGuid())
            .Without(x => x.Room)
            .Without(x => x.DateWithTimeInterval)
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages);
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

        _roomRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.RoomId!.Value))
            .ReturnsAsync(false);

        var service = CreateService();
        var serviceFunc = () => service.ValidateAsync(lessonToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(4, actualException.ValidationMessages.Length);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_MismatchedCyphers_Validation_Code()
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = _fixture.Create<string>(),
            }]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = _fixture.Create<string>(),
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.MismatchedCyphers, validationResult.Messages.First().Code);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                SemesterNumber = _fixture.Create<int>(),
            }]);

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
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.MismatchedSemesterNumber, validationResult.Messages.First().Code);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.MismatchedAcademicDisciplineType, validationResult.Messages.First().Code);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
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
                ValidationMessages = [],
            }
        ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.FixedLessonTypeConflictByGroup, validationResult.Messages.First().Code);
        Assert.Single(validationResult.LessonsWithConflictById.Keys.ToArray());
        Assert.Single(validationResult.LessonsWithConflictById.First().Value.ValidationMessages);
        Assert.Equal(LessonValidationCode.FlexibleLessonTypeConflictByGroup,
            validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
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
                ValidationMessages = [],
            }
        ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.FlexibleLessonTypeConflictByGroup, validationResult.Messages.First().Code);
        Assert.Single(validationResult.LessonsWithConflictById.Keys.ToArray());
        Assert.Single(validationResult.LessonsWithConflictById.First().Value.ValidationMessages);
        Assert.Equal(LessonValidationCode.FixedLessonTypeConflictByGroup,
            validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
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
            .With(x => x.TeacherId, Guid.NewGuid())
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _teacherRepositoryMock.Setup(r => r.GetAsync(lessonToSave.TeacherId!.Value, CancellationToken.None))
            .ReturnsAsync(new Teacher { Id = Guid.NewGuid() });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
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
                    x.TeacherId == lessonToSave.TeacherId &&
                    x.TimeInterval == lessonToSave.DateWithTimeInterval!.TimeInterval)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval(lessonToSave.DateWithTimeInterval!.Date.DayOfWeek,
                        lessonToSave.DateWithTimeInterval.TimeInterval),
                    TeacherPreferenceType = TeacherPreferenceType.Restricted,
                }
            ]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict,
            validationResult.Messages.First().Code);
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
            .With(x => x.TeacherId, Guid.NewGuid())
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherRepositoryMock.Setup(r => r.GetAsync(lessonToSave.TeacherId!.Value, CancellationToken.None))
            .ReturnsAsync(new Teacher { Id = Guid.NewGuid() });

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x =>
                    x.TeacherId == lessonToSave.TeacherId &&
                    x.TimeInterval == lessonToSave.DateWithTimeInterval!.TimeInterval)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval(lessonToSave.DateWithTimeInterval!.Date.DayOfWeek,
                        lessonToSave.DateWithTimeInterval.TimeInterval),
                    TeacherPreferenceType = TeacherPreferenceType.Undesirable,
                }
            ]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.UndesirableTimeTeacherPreferenceTypeConflict,
            validationResult.Messages.First().Code);
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
            .With(x => x.TeacherId, Guid.NewGuid())
            .Without(x => x.Teacher)
            .With(x => x.RoomId, Guid.NewGuid())
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _teacherRepositoryMock.Setup(r => r.GetAsync(lessonToSave.TeacherId!.Value, CancellationToken.None))
            .ReturnsAsync(new Teacher { Id = Guid.NewGuid() });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _roomRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.RoomId!.Value))
            .ReturnsAsync(true);

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x =>
                    x.TeacherId == lessonToSave.TeacherId
                    && x.RoomIds.Length == 1
                    && x.RoomIds.First() == lessonToSave.RoomId!.Value)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
                    RoomId = lessonToSave.RoomId,
                    TeacherPreferenceType = TeacherPreferenceType.Restricted,
                }
            ]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict,
            validationResult.Messages.First().Code);
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
            .With(x => x.TeacherId, Guid.NewGuid())
            .Without(x => x.Teacher)
            .With(x => x.RoomId, Guid.NewGuid())
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherRepositoryMock.Setup(r => r.GetAsync(lessonToSave.TeacherId!.Value, CancellationToken.None))
            .ReturnsAsync(new Teacher { Id = Guid.NewGuid() });

        _roomRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.RoomId!.Value))
            .ReturnsAsync(true);

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x =>
                    x.TeacherId == lessonToSave.TeacherId
                    && x.RoomIds.Length == 1
                    && x.RoomIds.First() == lessonToSave.RoomId!.Value)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
                    RoomId = lessonToSave.RoomId,
                    TeacherPreferenceType = TeacherPreferenceType.Undesirable,
                }
            ]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.UndesirableRoomTeacherPreferenceTypeConflict,
            validationResult.Messages.First().Code);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .With(x => x.RoomId, Guid.NewGuid())
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _roomRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.RoomId!.Value))
            .ReturnsAsync(true);

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.SetupSequence(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync([])
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroups = [new StudentGroup { Id = lessonToSave.StudentGroups.First().Id!.Value }],
                    FlexibilityType = LessonFlexibilityType.Flexible,
                    ValidationMessages = [],
                }
            ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.FlexibleLessonTypeConflictByRoom, validationResult.Messages.First().Code);
        Assert.Single(validationResult.LessonsWithConflictById.Keys.ToArray());
        Assert.Single(validationResult.LessonsWithConflictById.First().Value.ValidationMessages);
        Assert.Equal(LessonValidationCode.FixedLessonTypeConflictByRoom,
            validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .With(x => x.RoomId, Guid.NewGuid())
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval(DateTime.Today.ToDateOnly(),
                new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))))
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.StudentGroups.First().Id!.Value }, CancellationToken.None))
            .ReturnsAsync([new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            }]);

        var studentGroupIds = lessonToSave.StudentGroups.Select(x => x.Id!.Value).ToArray();
        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroupIds))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>() { { lessonToSave.StudentGroups.First().Id!.Value, studentGroupIds.ToList() } });

        _roomRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.RoomId!.Value))
            .ReturnsAsync(true);

        _academicDisciplineRepositoryMock
            .Setup(r => r.GetAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes =
                    [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.SetupSequence(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync([])
            .ReturnsAsync(
            [
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroups = [new StudentGroup { Id = lessonToSave.StudentGroups.First().Id!.Value }],
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    ValidationMessages = [],
                }
            ]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([])
            .Verifiable();

        var service = CreateService();

        // Act
        var validationResult = await service.ValidateAsync(lessonToSave);

        // Assert
        Assert.Single(validationResult.Messages);
        Assert.Equal(LessonValidationCode.FixedLessonTypeConflictByRoom, validationResult.Messages.First().Code);
        Assert.Single(validationResult.LessonsWithConflictById.Keys.ToArray());
        Assert.Single(validationResult.LessonsWithConflictById.First().Value.ValidationMessages);
        Assert.Equal(LessonValidationCode.FlexibleLessonTypeConflictByRoom,
            validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
    }

    [Fact]
    public async Task FillValidationMessages_Should_Produce_MismatchedCyphers_Validation_Code_Message()
    {
        // Arrange
        var academicDiscipline = new AcademicDiscipline
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
            Cypher = _fixture.Create<string>(),
        };
        var studentGroup = new StudentGroup
        {
            Id = Guid.NewGuid(),
            Name = _fixture.Create<string>(),
            Cypher = _fixture.Create<string>(),
        };
        var lesson = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, academicDiscipline.Id)
            .Without(x => x.AcademicDiscipline)
            .Without(x => x.AcademicDisciplineType)
            .With(x => x.StudentGroups, [new StudentGroup { Id = Guid.NewGuid() }])
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                    new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .With(x => x.ValidationMessages,
            [
                new LessonValidationMessage
                {
                    Code = LessonValidationCode.MismatchedCyphers,
                    Payload = new LessonValidationPayload
                    {
                        AffectedByAcademicDisciplineId = academicDiscipline.Id,
                        AffectedByStudentGroupId = studentGroup.Id,
                    }
                }
            ])
            .Create();

        _academicDisciplineRepositoryMock
            .Setup(x => x.GetAsync(lesson.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(academicDiscipline);

        _studentGroupRepositoryMock.Setup(r => r.GetAsync(studentGroup.Id!.Value, CancellationToken.None))
            .ReturnsAsync(studentGroup);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(LessonValidationMessageTemplates.MismatchedCyphersTemplate, academicDiscipline.Name,
                studentGroup.Name, studentGroup.Cypher, academicDiscipline.Cypher), actualMessages.First().Message);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                    new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .With(x => x.ValidationMessages,
            [
                new LessonValidationMessage
                {
                    Code = LessonValidationCode.MismatchedSemesterNumber,
                    Payload = new LessonValidationPayload
                    {
                        AffectedByAcademicDisciplineId = academicDiscipline.Id,
                        AffectedByStudentGroupId = studentGroup.Id,
                    }
                }
            ])
            .Create();

        _academicDisciplineRepositoryMock
            .Setup(x => x.GetAsync(lesson.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(academicDiscipline);

        _studentGroupRepositoryMock.Setup(r => r.GetAsync(studentGroup.Id!.Value, CancellationToken.None))
            .ReturnsAsync(studentGroup);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(LessonValidationMessageTemplates.MismatchedSemesterNumberTemplate, academicDiscipline.Name,
                studentGroup.Name, studentGroup.SemesterNumber, academicDiscipline.SemesterNumber),
            actualMessages.First().Message);
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                    new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .With(x => x.ValidationMessages,
            [
                new LessonValidationMessage
                {
                    Code = LessonValidationCode.MismatchedAcademicDisciplineType,
                    Payload = new LessonValidationPayload { AffectedByAcademicDisciplineId = academicDiscipline.Id }
                }
            ])
            .Create();

        _academicDisciplineRepositoryMock
            .Setup(x => x.GetAsync(lesson.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(academicDiscipline);

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(LessonValidationMessageTemplates.MismatchedAcademicDisciplineTypeTemplate,
                academicDiscipline.Name,
                lesson.AcademicDisciplineType!.Value.GetDescription()),
            actualMessages.First().Message);
    }

    [Theory]
    [InlineData(false, LessonValidationCode.FixedLessonTypeConflictByGroup)]
    [InlineData(true, LessonValidationCode.FixedLessonTypeConflictByGroup)]
    [InlineData(false, LessonValidationCode.FlexibleLessonTypeConflictByGroup)]
    [InlineData(true, LessonValidationCode.FlexibleLessonTypeConflictByGroup)]
    [InlineData(false, LessonValidationCode.FixedLessonTypeConflictByTeacher)]
    [InlineData(true, LessonValidationCode.FixedLessonTypeConflictByTeacher)]
    [InlineData(false, LessonValidationCode.FlexibleLessonTypeConflictByTeacher)]
    [InlineData(true, LessonValidationCode.FlexibleLessonTypeConflictByTeacher)]
    [InlineData(false, LessonValidationCode.FixedLessonTypeConflictByRoom)]
    [InlineData(true, LessonValidationCode.FixedLessonTypeConflictByRoom)]
    [InlineData(false, LessonValidationCode.FlexibleLessonTypeConflictByRoom)]
    [InlineData(true, LessonValidationCode.FlexibleLessonTypeConflictByRoom)]
    public async Task FillValidationMessages_Should_Produce_LessonTypeConflict_Validation_Code_Message(
        bool withAcademicDiscipline, LessonValidationCode validationCode)
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                    new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .With(x => x.ValidationMessages,
            [
                new LessonValidationMessage
                {
                    Code = validationCode,
                    Payload = new LessonValidationPayload
                    {
                        AffectedByLessonId = Guid.NewGuid(),
                        AffectedByStudentGroupId = Guid.NewGuid(),
                    }
                }
            ])
            .Create();

        _lessonRepositoryMock
            .Setup(x => x.GetAsync(lesson.ValidationMessages.First().Payload.AffectedByLessonId!.Value,
                CancellationToken.None))
            .ReturnsAsync(new Lesson
            {
                AcademicDiscipline = withAcademicDiscipline ? academicDiscipline : null,
                Teacher = validationCode is LessonValidationCode.FixedLessonTypeConflictByTeacher
                    or LessonValidationCode.FlexibleLessonTypeConflictByTeacher
                    ? new Teacher { Fullname = linkedEntityName }
                    : null,
                Room = validationCode is LessonValidationCode.FixedLessonTypeConflictByRoom
                    or LessonValidationCode.FlexibleLessonTypeConflictByRoom
                    ? new Room { Name = linkedEntityName }
                    : null,
            });

        _studentGroupRepositoryMock.Setup(r => r.GetAsync(lesson.ValidationMessages.First().Payload.AffectedByStudentGroupId!.Value, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Name = validationCode is LessonValidationCode.FixedLessonTypeConflictByGroup
                    or LessonValidationCode.FlexibleLessonTypeConflictByGroup
                    ? linkedEntityName
                    : studentGroupName
            });

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(GetMessageTemplate(validationCode),
                academicDiscipline != null ? academicDiscipline.Name : string.Empty,
                linkedEntityName),
            actualMessages.First().Message);

        return;

        string GetMessageTemplate(LessonValidationCode code)
        {
            return code switch
            {
                LessonValidationCode.FixedLessonTypeConflictByGroup => LessonValidationMessageTemplates
                    .FixedLessonTypeConflictByGroupTemplate,
                LessonValidationCode.FlexibleLessonTypeConflictByGroup => LessonValidationMessageTemplates
                    .FlexibleLessonTypeConflictByGroupTemplate,
                LessonValidationCode.FixedLessonTypeConflictByTeacher => LessonValidationMessageTemplates
                    .FixedLessonTypeConflictByTeacherTemplate,
                LessonValidationCode.FlexibleLessonTypeConflictByTeacher => LessonValidationMessageTemplates
                    .FlexibleLessonTypeConflictByTeacherTemplate,
                LessonValidationCode.FixedLessonTypeConflictByRoom => LessonValidationMessageTemplates
                    .FixedLessonTypeConflictByRoomTemplate,
                LessonValidationCode.FlexibleLessonTypeConflictByRoom => LessonValidationMessageTemplates
                    .FlexibleLessonTypeConflictByRoomTemplate,
                _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
            };
        }
    }

    [Theory]
    [InlineData(LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict)]
    [InlineData(LessonValidationCode.UndesirableTimeTeacherPreferenceTypeConflict)]
    [InlineData(LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict)]
    [InlineData(LessonValidationCode.UndesirableRoomTeacherPreferenceTypeConflict)]
    public async Task FillValidationMessages_Should_Produce_TeacherPreferenceTypeConflict_Validation_Code_Message(
        LessonValidationCode validationCode)
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                    new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .With(x => x.ValidationMessages,
            [
                new LessonValidationMessage
                {
                    Code = validationCode,
                    Payload = new LessonValidationPayload { AffectedByTeacherId = Guid.NewGuid() }
                }
            ])
            .Create();

        _teacherRepositoryMock
            .Setup(x => x.GetAsync(lesson.ValidationMessages.First().Payload.AffectedByTeacherId!.Value,
                CancellationToken.None))
            .ReturnsAsync(new Teacher { Fullname = teacherFullname });

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(GetMessageTemplate(validationCode), teacherFullname),
            actualMessages.First().Message);

        return;

        string GetMessageTemplate(LessonValidationCode code)
        {
            return code switch
            {
                LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict => LessonValidationMessageTemplates
                    .RestrictedTimeTeacherPreferenceTypeConflictTemplate,
                LessonValidationCode.UndesirableTimeTeacherPreferenceTypeConflict => LessonValidationMessageTemplates
                    .UndesirableTimeTeacherPreferenceTypeConflictTemplate,
                LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict => LessonValidationMessageTemplates
                    .RestrictedRoomTeacherPreferenceTypeConflictTemplate,
                LessonValidationCode.UndesirableRoomTeacherPreferenceTypeConflict => LessonValidationMessageTemplates
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
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval,
                new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                    new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
            .Without(x => x.FlexibilityType)
            .Without(x => x.AllowCombining)
            .Without(x => x.HoursCost)
            .With(x => x.ValidationMessages,
            [
                new LessonValidationMessage
                {
                    Code = LessonValidationCode.MismatchedAcademicDisciplineTypeTotalHoursCount,
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
            .Setup(x => x.GetAsync(lesson.ValidationMessages.First().Payload.AffectedByAcademicDisciplineId!.Value,
                CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Name = academicDisciplineName,
                LecturePayload = new AcademicDisciplinePayload { TotalHoursCount = expectedTotalHoursCount },
            });

        _studentGroupRepositoryMock.Setup(r => r.GetAsync(lesson.ValidationMessages.First().Payload.AffectedByStudentGroupId!.Value, CancellationToken.None))
            .ReturnsAsync(lesson.StudentGroups.First());

        var service = CreateService();

        // Act
        var actualMessages = await service.FillValidationMessages([lesson]);

        // Assert
        Assert.Single(actualMessages);
        Assert.Equal(
            string.Format(LessonValidationMessageTemplates.MismatchedAcademicDisciplineTypeTotalHoursCountTemplate,
                AcademicDisciplineType.Lecture.GetDescription(), academicDisciplineName, actualTotalHoursCount,
                expectedTotalHoursCount, lesson.StudentGroups.First().Name),
            actualMessages.First().Message);
    }

    // [Fact]
    // public async Task
    //     FillValidationMessages_Should_Produce_MismatchedAcademicDisciplineTypeLessonPerWeekCount_Validation_Code_Message()
    // {
    //     // Arrange
    //     var expectedLessonsPerWeekCount = _fixture.Create<int>();
    //     var academicDisciplineName = _fixture.Create<string>();
    //     var lesson = _fixture.Build<Lesson>()
    //         .Without(x => x.Id)
    //         .With(x => x.ScheduleId, Guid.NewGuid())
    //         .Without(x => x.Schedule)
    //         .With(x => x.AcademicDisciplineId, Guid.NewGuid())
    //         .Without(x => x.AcademicDiscipline)
    //         .Without(x => x.AcademicDisciplineType)
    //         .With(x => x.StudentGroups,
    //             [new StudentGroup { Id = Guid.NewGuid(), Name = _fixture.Create<string>() }])
    //         .Without(x => x.TeacherId)
    //         .Without(x => x.Teacher)
    //         .Without(x => x.RoomId)
    //         .Without(x => x.Room)
    //         .With(x => x.DateWithTimeInterval,
    //             new DateWithTimeInterval(new DateOnly(2026, 9, 7),
    //                 new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
    //         .Without(x => x.FlexibilityType)
    //         .Without(x => x.AllowCombining)
    //         .Without(x => x.HoursCost)
    //         .With(x => x.ValidationMessages,
    //         [
    //             new LessonValidationMessage
    //             {
    //                 Code = LessonValidationCode.MismatchedAcademicDisciplineTypeLessonPerWeekCount,
    //                 Payload = new LessonValidationPayload
    //                 {
    //                     AffectedByAcademicDisciplineId = Guid.NewGuid(),
    //                     AffectedByAcademicDisciplineType = AcademicDisciplineType.Lecture,
    //                 }
    //             }
    //         ])
    //         .Create();
    //
    //     _lessonRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<LessonSearchModel>()))
    //         .ReturnsAsync([
    //             new Lesson
    //             {
    //                 DateWithTimeInterval = lesson.DateWithTimeInterval,
    //                 AcademicDisciplineType = AcademicDisciplineType.Lecture
    //             }
    //         ]);
    //
    //     _academicDisciplineRepositoryMock
    //         .Setup(x => x.GetAsync(lesson.ValidationMessages.First().Payload.AffectedByAcademicDisciplineId!.Value,
    //             CancellationToken.None))
    //         .ReturnsAsync(new AcademicDiscipline
    //         {
    //             Name = academicDisciplineName,
    //             LecturePayload = new AcademicDisciplinePayload { LessonsPerWeekCount = expectedLessonsPerWeekCount }
    //         });
    //
    //     var service = CreateService();
    //
    //     // Act
    //     var actualMessages = await service.FillValidationMessages([lesson]);
    //
    //     // Assert
    //     Assert.Single(actualMessages);
    //     Assert.Equal(
    //         string.Format(LessonValidationMessageTemplates.MismatchedAcademicDisciplineTypeLessonPerWeekCountTemplate,
    //             AcademicDisciplineType.Lecture.GetDescription(), academicDisciplineName, 1, expectedLessonsPerWeekCount,
    //             lesson.StudentGroup.Name),
    //         actualMessages.First().Message);
    // }
    //
    // [Fact]
    // public async Task
    //     FillValidationMessages_Should_Produce_MismatchedAcademicDisciplineTypeStudyWeeksCount_Validation_Code_Message()
    // {
    //     // Arrange
    //     var expectedStudyWeeksCount = _fixture.Create<int>();
    //     var academicDisciplineName = _fixture.Create<string>();
    //     var lesson = _fixture.Build<Lesson>()
    //         .Without(x => x.Id)
    //         .With(x => x.ScheduleId, Guid.NewGuid())
    //         .Without(x => x.Schedule)
    //         .With(x => x.AcademicDisciplineId, Guid.NewGuid())
    //         .Without(x => x.AcademicDiscipline)
    //         .Without(x => x.AcademicDisciplineType)
    //         .With(x => x.StudentGroups,
    //             [new StudentGroup { Id = Guid.NewGuid(), Name = _fixture.Create<string>() }])
    //         .Without(x => x.TeacherId)
    //         .Without(x => x.Teacher)
    //         .Without(x => x.RoomId)
    //         .Without(x => x.Room)
    //         .With(x => x.DateWithTimeInterval,
    //             new DateWithTimeInterval(new DateOnly(2026, 9, 7),
    //                 new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
    //         .Without(x => x.FlexibilityType)
    //         .Without(x => x.AllowCombining)
    //         .Without(x => x.HoursCost)
    //         .With(x => x.ValidationMessages,
    //         [
    //             new LessonValidationMessage
    //             {
    //                 Code = LessonValidationCode.MismatchedAcademicDisciplineTypeStudyWeeksCount,
    //                 Payload = new LessonValidationPayload
    //                 {
    //                     AffectedByAcademicDisciplineId = Guid.NewGuid(),
    //                     AffectedByAcademicDisciplineType = AcademicDisciplineType.Lecture,
    //                 }
    //             }
    //         ])
    //         .Create();
    //
    //     _lessonRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<LessonSearchModel>()))
    //         .ReturnsAsync([
    //             new Lesson
    //             {
    //                 DateWithTimeInterval = lesson.DateWithTimeInterval,
    //                 AcademicDisciplineType = AcademicDisciplineType.Lecture
    //             }
    //         ]);
    //
    //     _academicDisciplineRepositoryMock
    //         .Setup(x => x.GetAsync(lesson.ValidationMessages.First().Payload.AffectedByAcademicDisciplineId!.Value,
    //             CancellationToken.None))
    //         .ReturnsAsync(new AcademicDiscipline
    //         {
    //             Name = academicDisciplineName,
    //             LecturePayload = new AcademicDisciplinePayload { StudyWeeksCount = expectedStudyWeeksCount }
    //         });
    //
    //     var service = CreateService();
    //
    //     // Act
    //     var actualMessages = await service.FillValidationMessages([lesson]);
    //
    //     // Assert
    //     Assert.Single(actualMessages);
    //     Assert.Equal(
    //         string.Format(LessonValidationMessageTemplates.MismatchedAcademicDisciplineTypeStudyWeeksCountTemplate,
    //             AcademicDisciplineType.Lecture.GetDescription(), academicDisciplineName, 1, expectedStudyWeeksCount,
    //             lesson.StudentGroup.Name),
    //         actualMessages.First().Message);
    // }
}