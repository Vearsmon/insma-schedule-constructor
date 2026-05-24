using AutoFixture;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.LessonPolicyViolations;
using Dal.Repositories.Rooms;
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
    private readonly Mock<ILessonRepository> _lessonRepositoryMock = new();
    private readonly Mock<ILessonPolicyViolationRepository> _lessonPolicyViolationRepositoryMock = new();

    private LessonValidationService CreateService() => new(
        _lessonRepositoryMock.Object,
        _lessonPolicyViolationRepositoryMock.Object,
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
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.StudentGroups,
                [
                    _fixture.Build<StudentGroup>()
                        .Without(x => x.Parents)
                        .Without(x => x.Children)
                        .Create(),
                ])
            .With(x => x.Teachers, [_fixture.Create<Teacher>()])
            .With(x => x.Rooms, [_fixture.Create<Room>()])
            .Without(x => x.LessonBatchInfo)
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(lesson.StudentGroups.Select(x => x.Id!.Value).ToArray(), CancellationToken.None))
            .ReturnsAsync([new StudentGroup()]);

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson]);

        var service = CreateService();
        var serviceFunc = () => service.ValidateAsync([lesson]);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(3, actualException.ValidationMessages.Length);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_MismatchedSemesterNumber_Validation_Code()
    {
        // Arrange
        var studentGroup = _fixture.Build<StudentGroup>()
            .Without(x => x.Parents)
            .Without(x => x.Children)
            .Create();
        var academicDisciplineType = _fixture.Create<AcademicDisciplineType>();
        var lessonToSave = _fixture.Build<Lesson>()
            .With(x => x.StudentGroups, [studentGroup])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .With(x => x.LessonBatchInfo,
                _fixture.Build<LessonBatchInfo>()
                    .With(x => x.AcademicDiscipline,
                        _fixture.Build<AcademicDiscipline>()
                            .With(x => x.AllowedLessonTypes, [academicDisciplineType])
                            .Without(x => x.LessonBatchInfos)
                            .Create())
                    .With(x => x.Type, academicDisciplineType)
                    .Without(x => x.StudentGroups)
                    .Without(x => x.Teachers)
                    .Without(x => x.Rooms)
                    .Create())
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { studentGroup.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([studentGroup]);

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(new[] { studentGroup.Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { studentGroup.Id!.Value, [studentGroup.Id!.Value] }
            });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lessonToSave]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(LessonPolicyViolationCode.MismatchedSemesterNumber, violations.Single().Code);
    }

    [Fact]
    public async Task ValidateAsync_Should_Produce_MismatchedAcademicDisciplineType_Validation_Code()
    {
        // Arrange
        var semesterNumber = _fixture.Create<int>();
        var studentGroup = _fixture.Build<StudentGroup>()
            .With(x => x.SemesterNumber, semesterNumber)
            .Without(x => x.Parents)
            .Without(x => x.Children)
            .Create();
        const AcademicDisciplineType batchType = AcademicDisciplineType.Practice;
        const AcademicDisciplineType academicDisciplineAllowedType = AcademicDisciplineType.Lecture;
        var lessonToSave = _fixture.Build<Lesson>()
            .With(x => x.StudentGroups, [studentGroup])
            .Without(x => x.Teachers)
            .Without(x => x.Rooms)
            .With(x => x.FlexibilityType, LessonFlexibilityType.Flexible)
            .With(x => x.LessonBatchInfo,
                _fixture.Build<LessonBatchInfo>()
                    .With(x => x.AcademicDiscipline,
                        _fixture.Build<AcademicDiscipline>()
                            .With(x => x.SemesterNumber, semesterNumber)
                            .With(x => x.AllowedLessonTypes, [academicDisciplineAllowedType])
                            .Without(x => x.LessonBatchInfos)
                            .Create())
                    .With(x => x.Type, batchType)
                    .Without(x => x.StudentGroups)
                    .Without(x => x.Teachers)
                    .Without(x => x.Rooms)
                    .Create())
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { studentGroup.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([studentGroup]);

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(new[] { studentGroup.Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>>
            {
                { studentGroup.Id!.Value, [studentGroup.Id!.Value] }
            });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lessonToSave.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lessonToSave]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lessonToSave]);

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
    public async Task ValidateAsync_Should_Produce_LessonTypeConflict_Validation_Code(
        LessonPolicyViolationCode policyViolationCode)
    {
        // Arrange
        var semesterNumber = _fixture.Create<int>();
        var studentGroup = _fixture.Build<StudentGroup>()
            .With(x => x.SemesterNumber, semesterNumber)
            .Without(x => x.Parents)
            .Without(x => x.Children)
            .Create();
        var academicDisciplineType = _fixture.Create<AcademicDisciplineType>();
        var teacher = policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByTeacher
            or LessonPolicyViolationCode.FlexibleLessonTypeConflictByTeacher
            ? _fixture.Create<Teacher>()
            : null;
        var room = policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByRoom
            or LessonPolicyViolationCode.FlexibleLessonTypeConflictByRoom
            ? _fixture.Create<Room>()
            : null;
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.StudentGroups, [studentGroup])
            .With(x => x.Teachers, teacher == null ? [] : [teacher])
            .With(x => x.Rooms, room == null ? [] : [room])
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
                    ? LessonFlexibilityType.Fixed
                    : LessonFlexibilityType.Flexible)
            .Without(x => x.AllowCombining)
            .With(x => x.LessonBatchInfo,
                _fixture.Build<LessonBatchInfo>()
                    .With(x => x.AcademicDiscipline,
                        _fixture.Build<AcademicDiscipline>()
                            .With(x => x.SemesterNumber, semesterNumber)
                            .With(x => x.AllowedLessonTypes, [academicDisciplineType])
                            .Without(x => x.LessonBatchInfos)
                            .Create())
                    .With(x => x.Type, academicDisciplineType)
                    .Without(x => x.StudentGroups)
                    .Without(x => x.Teachers)
                    .Without(x => x.Rooms)
                    .Create())
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { studentGroup.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([studentGroup]);

        if (room != null)
        {
            _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { room.Id!.Value }, CancellationToken.None))
                .ReturnsAsync([room]);
        }

        if (teacher != null)
        {
            _teacherRepositoryMock.Setup(r => r.SelectAsync(new[] { teacher.Id!.Value }, CancellationToken.None))
                .ReturnsAsync([teacher]);
        }

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(new[] { studentGroup.Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { studentGroup.Id!.Value, [studentGroup.Id!.Value] } });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson]);

        _lessonRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<LessonConflictsSearchModel>()))
            .ReturnsAsync(
            [
                _fixture.Build<Lesson>()
                    .With(x => x.StudentGroups,
                        policyViolationCode is LessonPolicyViolationCode.FixedLessonTypeConflictByGroup
                            or LessonPolicyViolationCode.FlexibleLessonTypeConflictByGroup ? [studentGroup] : [])
                    .With(x => x.Teachers, teacher == null ? [] : [teacher])
                    .With(x => x.Rooms, room == null ? [] : [room])
                    .With(x => x.FlexibilityType,
                       lesson.FlexibilityType == LessonFlexibilityType.Fixed
                            ? LessonFlexibilityType.Flexible : LessonFlexibilityType.Fixed)
                    .With(x => x.DateWithTimeInterval, new DateWithTimeInterval
                    {
                        Date = DateTime.Today.ToDateOnly(),
                        TimeInterval = new TimeInterval
                        {
                            TimeFrom = new TimeOnly(11, 0),
                            TimeTo = new TimeOnly(15, 0),
                        },
                    })
                    .Without(x => x.AllowCombining)
                    .Without(x => x.LessonBatchInfo)
                    .Without(x => x.Violations)
                    .Create(),
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
    public async Task ValidateAsync_Should_Produce_TeacherPreferenceTypeConflict_Validation_Code_Message(
        LessonPolicyViolationCode policyViolationCode)
    {
        // Arrange
        var semesterNumber = _fixture.Create<int>();
        var studentGroup = _fixture.Build<StudentGroup>()
            .With(x => x.SemesterNumber, semesterNumber)
            .Without(x => x.Parents)
            .Without(x => x.Children)
            .Create();
        var teacher = _fixture.Create<Teacher>();
        var room = _fixture.Create<Room>();
        var academicDisciplineType = _fixture.Create<AcademicDisciplineType>();
        var lesson = _fixture.Build<Lesson>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.StudentGroups, [studentGroup])
            .With(x => x.Teachers, [teacher])
            .With(x => x.Rooms, [room])
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
            .With(x => x.LessonBatchInfo,
                _fixture.Build<LessonBatchInfo>()
                    .With(x => x.AcademicDiscipline,
                        _fixture.Build<AcademicDiscipline>()
                            .With(x => x.SemesterNumber, semesterNumber)
                            .With(x => x.AllowedLessonTypes, [academicDisciplineType])
                            .Without(x => x.LessonBatchInfos)
                            .Create())
                    .With(x => x.Type, academicDisciplineType)
                    .Without(x => x.StudentGroups)
                    .Without(x => x.Teachers)
                    .Without(x => x.Rooms)
                    .Create())
            .Without(x => x.Violations)
            .Create();

        _studentGroupRepositoryMock.Setup(r => r.SelectAsync(new[] { studentGroup.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([studentGroup]);

        _teacherRepositoryMock.Setup(r => r.SelectAsync(new[] { teacher.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([teacher]);

        _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { room.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([room]);

        _studentGroupRepositoryMock.Setup(r => r.GetStudentGroupTreeIdsAsync(new[] { studentGroup.Id!.Value }))
            .ReturnsAsync(new Dictionary<Guid, List<Guid>> { { studentGroup.Id!.Value, [studentGroup.Id!.Value] } });

        _lessonRepositoryMock.Setup(r => r.SelectAsync(new[] { lesson.Id!.Value }, CancellationToken.None))
            .ReturnsAsync([lesson]);

        _lessonRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<LessonConflictsSearchModel>()))
            .ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchConflictsAsync(It.IsAny<TeacherPreferenceConflictsSearchModel>()))
            .ReturnsAsync([
                _fixture.Build<TeacherPreference>()
                    .With(x => x.TeacherId, teacher.Id!.Value)
                    .With(x => x.RoomId, policyViolationCode is LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict
                        or LessonPolicyViolationCode.RestrictedRoomTeacherPreferenceTypeConflict
                        ? room.Id!.Value : null)
                    .With(x => x.DayOfWeekTimeInterval, policyViolationCode is LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict
                        or LessonPolicyViolationCode.RestrictedTimeTeacherPreferenceTypeConflict
                        ? new DayOfWeekTimeInterval
                        {
                            DayOfWeek = DateTime.Today.ToDateOnly().DayOfWeek,
                            TimeInterval = new TimeInterval
                            {
                                TimeFrom = new TimeOnly(11, 0),
                                TimeTo = new TimeOnly(15, 0),
                            },
                        } : null)
                    .With(x => x.TeacherPreferenceType, policyViolationCode is LessonPolicyViolationCode.UndesirableRoomTeacherPreferenceTypeConflict
                        or LessonPolicyViolationCode.UndesirableTimeTeacherPreferenceTypeConflict
                        ? TeacherPreferenceType.Undesirable : TeacherPreferenceType.Restricted)
                    .Create(),
            ]);

        _lessonRepositoryMock.Setup(r => r.SaveAllAsync(It.IsAny<Lesson[]>(), CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var violations = await service.ValidateAsync([lesson]);

        // Assert
        Assert.Single(violations);
        Assert.Equal(policyViolationCode, violations.Single().Code);
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