using AutoFixture;
using Dal.RegistryRepositories.Lesson;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.StudentGroups;
using Dal.Repositories.TeacherPreferences;
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
    private readonly Mock<ILessonValidationService> _lessonValidationServiceMock = new();
    private readonly Mock<IAcademicDisciplineRepository> _academicDisciplineRepositoryMock = new();
    private readonly Mock<IStudentGroupRepository> _studentGroupRepositoryMock = new();
    private readonly Mock<ITeacherPreferenceRepository> _teacherPreferenceRepositoryMock = new();

    private LessonService CreateService() => new(
        _lessonRepositoryMock.Object,
        _lessonRegistryRepositoryMock.Object,
        _lessonValidationServiceMock.Object,
        _academicDisciplineRepositoryMock.Object,
        _studentGroupRepositoryMock.Object,
        _teacherPreferenceRepositoryMock.Object
    );

    [Fact]
    public async Task RecalculateConflictsForUpdatedAcademicDiscipline_Should_Produce_Validation_Messages()
    {
        // Arrange
        var academicDiscipline = _fixture.Build<AcademicDiscipline>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .Without(x => x.Name)
            .With(x => x.Cypher, _fixture.Create<string>())
            .With(x => x.SemesterNumber, 5)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [AcademicDisciplineType.Lecture])
            .Without(x => x.LecturePayload)
            .Without(x => x.PracticePayload)
            .Without(x => x.LabPayload)
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
                    StudentGroupId = Guid.NewGuid(),
                    StudentGroup = new StudentGroup { Cypher = _fixture.Create<string>(), SemesterNumber = 6 },
                    AcademicDisciplineType = AcademicDisciplineType.Lecture,
                },
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    StudentGroupId = Guid.NewGuid(),
                    StudentGroup = new StudentGroup
                    {
                        Cypher = academicDiscipline.Cypher,
                        SemesterNumber = academicDiscipline.SemesterNumber
                    },
                    AcademicDisciplineType = AcademicDisciplineType.Practice,
                },
            ]);

        var service = CreateService();

        var actualLessons = new List<Lesson>();
        _lessonRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .Callback<Lesson[], CancellationToken>((lessons, _) => actualLessons.AddRange(lessons));

        // Act
        await service.RecalculateConflictsForUpdatedAcademicDiscipline(academicDiscipline);

        // Assert
        Assert.Equal(2, actualLessons.Count);
        Assert.Equal(2, actualLessons.First().ValidationMessages.Length);
        Assert.Contains(actualLessons.First().ValidationMessages, x => x.Code == LessonValidationCode.MismatchedCyphers);
        Assert.Contains(actualLessons.First().ValidationMessages, x => x.Code == LessonValidationCode.MismatchedSemesterNumber);
        Assert.Single(actualLessons.Last().ValidationMessages);
        Assert.Contains(actualLessons.Last().ValidationMessages, x => x.Code == LessonValidationCode.MismatchedAcademicDisciplineType);
    }

    [Fact]
    public async Task UpdateAcademicDisciplineLessons_Should_Produce_New_Lessons_When_Discipline_Primary_Lessons_Save()
    {
        // Arrange
        var firstTimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30));
        var secondTimeInterval = new TimeInterval(new TimeOnly(10, 0), new TimeOnly(11, 30));
        var academicDiscipline = _fixture.Build<AcademicDiscipline>()
            .Without(x => x.Id)
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.Name, _fixture.Create<string>())
            .With(x => x.Cypher, _fixture.Create<string>())
            .With(x => x.SemesterNumber, 5)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [AcademicDisciplineType.Lecture])
            .With(x => x.LecturePayload, _fixture
                .Build<AcademicDisciplinePayload>()
                .With(x => x.TotalHoursCount, _fixture.Create<int>())
                .With(x => x.StudyWeeksCount, _fixture.Create<int>())
                .With(x => x.LessonsPerWeekCount, _fixture.Create<int>())
                .With(x => x.LessonBatchInfo, _fixture
                    .Build<AcademicDisciplineLessonBatchInfo>()
                    .Without(x => x.Id)
                    .With(x => x.StudentGroupId, Guid.NewGuid())
                    .Without(x => x.StudentGroup)
                    .Without(x => x.TeacherId)
                    .Without(x => x.Teacher)
                    .Without(x => x.RoomId)
                    .Without(x => x.Room)
                    .With(x => x.DayOfWeekTimeIntervals,
                    [
                        new DayOfWeekTimeInterval(DayOfWeek.Tuesday, firstTimeInterval),
                        new DayOfWeekTimeInterval(DayOfWeek.Thursday, secondTimeInterval),
                    ])
                    .With(x => x.RepeatType, DisciplineLessonRepeatType.Weekly)
                    .With(x => x.DateInterval,
                        // три недели - с понедельника 07.09 по понедельник 28.09 включительно
                        new DateInterval(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 28)))
                    .Without(x => x.AllowCombining)
                    .With(x => x.HoursCost, _fixture.Create<int>())
                    .Create()
                )
                .Create())
            .Without(x => x.PracticePayload)
            .Without(x => x.LabPayload)
            .Without(x => x.Comment)
            .Create();

        var expectedLessonDates = new[]
        {
            new DateOnly(2026, 9, 8),
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 15),
            new DateOnly(2026, 9, 17),
            new DateOnly(2026, 9, 22),
            new DateOnly(2026, 9, 24),
        };

        _lessonValidationServiceMock.Setup(x => x.ValidateAsync(It.IsAny<Lesson>()))
            .ReturnsAsync(new LessonValidationResult
            {
                Messages = [],
                LessonsWithConflictById = new Dictionary<Guid, Lesson>(),
            });

        var service = CreateService();

        var actualLessons = new List<Lesson>();
        _lessonRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .Callback<Lesson[], CancellationToken>((lessons, _) => actualLessons.AddRange(lessons));

        // Act
        await service.UpdateAcademicDisciplineLessons(academicDiscipline);

        // Assert
        Assert.Equal(6, actualLessons.Count);
        for (var i = 0; i < actualLessons.Count; i++)
        {
            Assert.Equal(new DateWithTimeInterval(expectedLessonDates[i],
                    i % 2 == 0 ? firstTimeInterval : secondTimeInterval),
                actualLessons[i].DateWithTimeInterval);
        }
    }

    [Fact]
    public async Task UpdateAcademicDisciplineLessons_Should_Update_Previous_Academic_Discipline_Version_Lessons()
    {
        // Arrange
        var firstTimeInterval = new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30));
        var secondTimeInterval = new TimeInterval(new TimeOnly(10, 0), new TimeOnly(11, 30));
        var payloadFixture = _fixture
            .Build<AcademicDisciplinePayload>()
            .With(x => x.TotalHoursCount, _fixture.Create<int>())
            .With(x => x.StudyWeeksCount, _fixture.Create<int>())
            .With(x => x.LessonsPerWeekCount, _fixture.Create<int>())
            .With(x => x.LessonBatchInfo, _fixture
                .Build<AcademicDisciplineLessonBatchInfo>()
                .Without(x => x.Id)
                .With(x => x.StudentGroupId, Guid.NewGuid())
                .Without(x => x.StudentGroup)
                .Without(x => x.TeacherId)
                .Without(x => x.Teacher)
                .Without(x => x.RoomId)
                .Without(x => x.Room)
                .With(x => x.DayOfWeekTimeIntervals,
                [
                    new DayOfWeekTimeInterval(DayOfWeek.Tuesday, firstTimeInterval),
                    new DayOfWeekTimeInterval(DayOfWeek.Thursday, secondTimeInterval),
                ])
                .With(x => x.RepeatType, DisciplineLessonRepeatType.Weekly)
                .With(x => x.DateInterval,
                    // три недели - с понедельника 07.09 по понедельник 28.09 включительно
                    new DateInterval(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 28)))
                .Without(x => x.AllowCombining)
                .With(x => x.HoursCost, _fixture.Create<int>())
                .Create()
            )
            .Create();

        var academicDiscipline = _fixture.Build<AcademicDiscipline>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Schedule)
            .With(x => x.Name, _fixture.Create<string>())
            .With(x => x.Cypher, _fixture.Create<string>())
            .With(x => x.SemesterNumber, 5)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [AcademicDisciplineType.Practice, AcademicDisciplineType.Lab])
            .Without(x => x.LecturePayload)
            .With(x => x.PracticePayload, payloadFixture)
            .With(x => x.LabPayload, payloadFixture)
            .Without(x => x.Comment)
            .Create();

        _academicDisciplineRepositoryMock.Setup(x => x.GetAsync(academicDiscipline.Id!.Value, CancellationToken.None))
            .ReturnsAsync(new AcademicDiscipline
            {
                AllowedLessonTypes = [AcademicDisciplineType.Lecture, AcademicDisciplineType.Practice]
            });

        var existingLessonIds = new[] { Guid.NewGuid() };

        _lessonRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync(existingLessonIds.Select(id => new Lesson { Id = id }).ToArray());

        _lessonRepositoryMock.Setup(x =>
            x.DeleteAsync(It.Is<Guid[]>(y => y == existingLessonIds), CancellationToken.None));

        _lessonValidationServiceMock.Setup(x => x.ValidateAsync(It.IsAny<Lesson>()))
            .ReturnsAsync(new LessonValidationResult
            {
                Messages = [],
                LessonsWithConflictById = new Dictionary<Guid, Lesson>(),
            });

        var service = CreateService();

        var actualLessons = new List<Lesson>();
        _lessonRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .Callback<Lesson[], CancellationToken>((lessons, _) => actualLessons.AddRange(lessons));

        // Act
        await service.UpdateAcademicDisciplineLessons(academicDiscipline);

        // Assert
        Assert.Equal(12, actualLessons.Count);
        for (var i = 0; i < actualLessons.Count; i++)
        {
            Assert.Equal(i < 6 ? AcademicDisciplineType.Lab : AcademicDisciplineType.Practice,
                actualLessons[i].AcademicDisciplineType);
        }
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
                    .With(x => x.DayOfWeekTimeInterval,
                        new DayOfWeekTimeInterval(DayOfWeek.Monday,
                            new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))))
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
            .ReturnsAsync([new Lesson
            {
                Id = Guid.NewGuid(),
                DateWithTimeInterval = new DateWithTimeInterval(
                    new DateOnly(2026, 9, 7),
                    teacherPreferences.First().DayOfWeekTimeInterval!.TimeInterval),
            }])
            .ReturnsAsync([new Lesson
            {
                Id = Guid.NewGuid(),
                RoomId = teacherPreferences.Last().RoomId,
            }]);

        var service = CreateService();

        var actualLessons = new List<Lesson>();
        _lessonRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .Callback<Lesson[], CancellationToken>((lessons, _) => actualLessons.AddRange(lessons));

        // Act
        await service.RecalculateConflictsForNewTeacherPreferences(teacherPreferences);

        // Assert
        Assert.Equal(2, actualLessons.Count);
        Assert.Single(actualLessons.First().ValidationMessages);
        Assert.Contains(actualLessons.First().ValidationMessages, x => x.Code == LessonValidationCode.RestrictedTimeTeacherPreferenceTypeConflict);
        Assert.Single(actualLessons.Last().ValidationMessages);
        Assert.Contains(actualLessons.Last().ValidationMessages, x => x.Code == LessonValidationCode.RestrictedRoomTeacherPreferenceTypeConflict);
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
            .With(x => x.Cypher, _fixture.Create<string>())
            .Without(x => x.ParentId)
            .Without(x => x.Parent)
            .Without(x => x.Children)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(studentGroup.Id!.Value))
            .ReturnsAsync([studentGroup.Id!.Value, firstStudentGroupId, secondStudentGroupId]);

        _lessonRepositoryMock.SetupSequence(r => r.SearchAsync(It.IsAny<LessonSearchModel>()))
            .ReturnsAsync([])
            .ReturnsAsync([
                new Lesson
                {
                    Id = Guid.NewGuid(),
                    AcademicDiscipline = new AcademicDiscipline
                    {
                        Id = Guid.NewGuid(),
                        Cypher = _fixture.Create<string>(),
                        SemesterNumber = 6,
                    }
                },
                new Lesson
                {
                    Id = firstExpectedLessonId,
                    StudentGroupId = firstStudentGroupId,
                    DateWithTimeInterval = new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                        new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 30))),
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AcademicDiscipline = new AcademicDiscipline
                    {
                        Id = Guid.NewGuid(),
                        Cypher = studentGroup.Cypher,
                        SemesterNumber = studentGroup.SemesterNumber,
                    }
                },
                new Lesson
                {
                    Id = secondExpectedLessonId,
                    StudentGroupId = studentGroup.Id!.Value,
                    DateWithTimeInterval = new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                        new TimeInterval(new TimeOnly(10, 0), new TimeOnly(11, 30))),
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AcademicDiscipline = new AcademicDiscipline
                    {
                        Id = Guid.NewGuid(),
                        Cypher = studentGroup.Cypher,
                        SemesterNumber = studentGroup.SemesterNumber,
                    }
                },
                new Lesson
                {
                    Id = thirdExpectedLessonId,
                    StudentGroupId = secondStudentGroupId,
                    DateWithTimeInterval = new DateWithTimeInterval(new DateOnly(2026, 9, 7),
                        new TimeInterval(new TimeOnly(11, 0), new TimeOnly(12, 30))),
                    FlexibilityType = LessonFlexibilityType.Fixed,
                    AcademicDiscipline = new AcademicDiscipline
                    {
                        Id = Guid.NewGuid(),
                        Cypher = studentGroup.Cypher,
                        SemesterNumber = studentGroup.SemesterNumber,
                    }
                },
            ]);

        var service = CreateService();

        var actualLessons = new List<Lesson>();
        _lessonRepositoryMock.Setup(m => m.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .Callback<Lesson[], CancellationToken>((lessons, _) => actualLessons.AddRange(lessons));

        // Act
        await service.RecalculateConflictsForNewStudentGroup(studentGroup);

        // Assert
        Assert.Equal(4, actualLessons.Count);

        Assert.Equal(2, actualLessons.First().ValidationMessages.Length);
        Assert.Contains(actualLessons.First().ValidationMessages, x => x.Code == LessonValidationCode.MismatchedCyphers);
        Assert.Contains(actualLessons.First().ValidationMessages, x => x.Code == LessonValidationCode.MismatchedSemesterNumber);

        Assert.Single(actualLessons[1].ValidationMessages);
        Assert.Contains(actualLessons[1].ValidationMessages, x =>
            x.Code == LessonValidationCode.FixedLessonTypeConflictByGroup && x.Payload.AffectedByLessonId == secondExpectedLessonId);

        Assert.Equal(2, actualLessons[2].ValidationMessages.Length);
        Assert.Contains(actualLessons[2].ValidationMessages, x =>
            x.Code == LessonValidationCode.FixedLessonTypeConflictByGroup && x.Payload.AffectedByLessonId == firstExpectedLessonId);
        Assert.Contains(actualLessons[2].ValidationMessages, x =>
            x.Code == LessonValidationCode.FixedLessonTypeConflictByGroup && x.Payload.AffectedByLessonId == thirdExpectedLessonId);

        Assert.Single(actualLessons[3].ValidationMessages);
        Assert.Contains(actualLessons[3].ValidationMessages, x =>
            x.Code == LessonValidationCode.FixedLessonTypeConflictByGroup && x.Payload.AffectedByLessonId == secondExpectedLessonId);
    }
}