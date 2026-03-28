using AutoFixture;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonValidationMessages;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
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
    public async Task SaveLesson_Should_Produce_MismatchedCyphers_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = _fixture.Create<string>(),
            });

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = _fixture.Create<string>(),
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
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
    public async Task SaveLesson_Should_Produce_MismatchedSemesterNumber_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                SemesterNumber = _fixture.Create<int>(),
            });

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                SemesterNumber = _fixture.Create<int>(),
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
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
    public async Task SaveLesson_Should_Produce_MismatchedAcademicDisciplineType_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lab)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .Without(x => x.DateWithTimeInterval)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
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
    public async Task SaveLesson_Should_Produce_FixedLessonTypeConflictByGroup_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
            {
                Date = DateTime.Today.ToDateOnly(),
                TimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(lessonToSave.StudentGroupId))
            .ReturnsAsync([lessonToSave.StudentGroupId]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([
            new Lesson
            {
                Id = Guid.NewGuid(),
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
        Assert.Equal(LessonValidationCode.FlexibleLessonTypeConflictByGroup, validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
    }

    [Fact]
    public async Task SaveLesson_Should_Produce_FlexibleLessonTypeConflictByGroup_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
            {
                Date = DateTime.Today.ToDateOnly(),
                TimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(lessonToSave.StudentGroupId))
            .ReturnsAsync([lessonToSave.StudentGroupId]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([
            new Lesson
            {
                Id = Guid.NewGuid(),
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
        Assert.Equal(LessonValidationCode.FixedLessonTypeConflictByGroup, validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
    }

    [Fact]
    public async Task SaveLesson_Should_Produce_RestrictedTeacherPreferenceTypeConflict_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .With(x => x.TeacherId, Guid.NewGuid())
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
            {
                Date = DateTime.Today.ToDateOnly(),
                TimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(lessonToSave.StudentGroupId))
            .ReturnsAsync([lessonToSave.StudentGroupId]);

        _teacherRepositoryMock.Setup(r => r.FindAsync(lessonToSave.TeacherId!.Value, CancellationToken.None))
            .ReturnsAsync(new Teacher { Id = Guid.NewGuid() });

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x => x.TeacherId == lessonToSave.TeacherId)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
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
        Assert.Equal(LessonValidationCode.RestrictedTeacherPreferenceTypeConflict, validationResult.Messages.First().Code);
    }

    [Fact]
    public async Task SaveLesson_Should_Produce_UndesirableTeacherPreferenceTypeConflict_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .With(x => x.TeacherId, Guid.NewGuid())
            .Without(x => x.Teacher)
            .Without(x => x.RoomId)
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
            {
                Date = DateTime.Today.ToDateOnly(),
                TimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(lessonToSave.StudentGroupId))
            .ReturnsAsync([lessonToSave.StudentGroupId]);

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
            });

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(lessonToSave.ScheduleId))
            .ReturnsAsync(true);

        _lessonRepositoryMock.Setup(r => r.SearchAsync(It.Is<LessonSearchModel>(m =>
            m.Date != null
        ))).ReturnsAsync([]);

        _teacherRepositoryMock.Setup(r => r.FindAsync(lessonToSave.TeacherId!.Value, CancellationToken.None))
            .ReturnsAsync(new Teacher { Id = Guid.NewGuid() });

        _teacherPreferenceRepositoryMock.Setup(r =>
                r.SearchAsync(It.Is<TeacherPreferenceSearchModel>(x => x.TeacherId == lessonToSave.TeacherId)))
            .ReturnsAsync([
                new TeacherPreference
                {
                    Id = Guid.NewGuid(),
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
        Assert.Equal(LessonValidationCode.UndesirableTeacherPreferenceTypeConflict, validationResult.Messages.First().Code);
    }

    [Fact]
    public async Task SaveLesson_Should_Produce_FixedLessonTypeConflictByRoom_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .With(x => x.RoomId, Guid.NewGuid())
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
            {
                Date = DateTime.Today.ToDateOnly(),
                TimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Fixed)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(lessonToSave.StudentGroupId))
            .ReturnsAsync([lessonToSave.StudentGroupId]);

        _roomRepositoryMock.Setup(r => r.FindAsync(lessonToSave.RoomId!.Value, CancellationToken.None))
            .ReturnsAsync(new Room());

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
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
        Assert.Equal(LessonValidationCode.FixedLessonTypeConflictByRoom, validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
    }

    [Fact]
    public async Task SaveLesson_Should_Produce_FlexibleLessonTypeConflictByRoom_Validation_code()
    {
        // Arrange
        var lessonToSave = _fixture.Build<Lesson>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.AcademicDisciplineId, Guid.NewGuid())
            .Without(x => x.AcademicDiscipline)
            .With(x => x.AcademicDisciplineType, AcademicDisciplineType.Lecture)
            .With(x => x.StudentGroupId, Guid.NewGuid())
            .Without(x => x.StudentGroup)
            .Without(x => x.TeacherId)
            .Without(x => x.Teacher)
            .With(x => x.RoomId, Guid.NewGuid())
            .Without(x => x.Room)
            .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
            {
                Date = DateTime.Today.ToDateOnly(),
                TimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            })
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .Without(x => x.HoursCost)
            .Without(x => x.CreatedFromDiscipline)
            .Without(x => x.ValidationMessages)
            .Create();

        var cypher = _fixture.Create<string>();
        var semesterNumber = _fixture.Create<int>();

        _studentGroupRepositoryMock.Setup(r => r.FindAsync(lessonToSave.StudentGroupId, CancellationToken.None))
            .ReturnsAsync(new StudentGroup
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
            });

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(lessonToSave.StudentGroupId))
            .ReturnsAsync([lessonToSave.StudentGroupId]);

        _roomRepositoryMock.Setup(r => r.FindAsync(lessonToSave.RoomId!.Value, CancellationToken.None))
            .ReturnsAsync(new Room());

        _academicDisciplineRepositoryMock
            .Setup(r => r.FindAsync(lessonToSave.AcademicDisciplineId!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                Id = Guid.NewGuid(),
                Cypher = cypher,
                SemesterNumber = semesterNumber,
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice, AcademicDisciplineType.Lab],
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
        Assert.Equal(LessonValidationCode.FlexibleLessonTypeConflictByRoom, validationResult.LessonsWithConflictById.First().Value.ValidationMessages.First().Code);
    }
}