using AutoFixture;
using Dal.RegistryRepositories.TeacherPreference;
using Dal.Repositories.Rooms;
using Dal.Repositories.Schedules;
using Dal.Repositories.TeacherPreferences;
using Dal.Repositories.Teachers;
using Domain.Dto;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.Enums;
using Domain.Models.SearchModels;
using Domain.Services;
using Moq;
using Services;

namespace Tests;

public class TeacherPreferenceServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<ITeacherRepository> _teacherRepositoryMock = new();
    private readonly Mock<ITeacherPreferenceRepository> _teacherPreferenceRepositoryMock = new();
    private readonly Mock<ITeacherPreferenceRegistryRepository> _teacherPreferenceRegistryRepositoryMock = new();
    private readonly Mock<IRoomRepository> _roomRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<ILessonService> _lessonServiceMock = new();

    private TeacherPreferenceService CreateService() => new(
        _teacherPreferenceRepositoryMock.Object,
        _teacherPreferenceRegistryRepositoryMock.Object,
        _scheduleRepositoryMock.Object,
        _teacherRepositoryMock.Object,
        _roomRepositoryMock.Object,
        _lessonServiceMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var teacherPreferenceToSave = _fixture.Build<SaveTeacherPreferenceDto>()
            .With(x => x.ScheduleId, Guid.NewGuid())
            .With(x => x.TeacherId, Guid.NewGuid())
            .With(x => x.TeacherTimeAvailabilities,
                [
                    new TeacherTimeAvailabilityDto
                    {
                        TeacherPreferenceType = TeacherPreferenceType.Restricted,
                        DayOfWeekTimeInterval = new DayOfWeekTimeInterval(DayOfWeek.Monday,
                            new TimeInterval(new TimeOnly(9, 0), new TimeOnly(10, 0))),
                    },
                    new TeacherTimeAvailabilityDto
                    {
                        TeacherPreferenceType = TeacherPreferenceType.Preferred,
                        DayOfWeekTimeInterval = new DayOfWeekTimeInterval(DayOfWeek.Monday,
                            new TimeInterval(new TimeOnly(9, 30), new TimeOnly(10, 30))),
                    },
                ])
            .With(x => x.TeacherRoomPreferences,
                [
                    new TeacherRoomPreferenceDto
                    {
                        TeacherPreferenceType = TeacherPreferenceType.Restricted,
                        RoomId = roomId,
                    },
                    new TeacherRoomPreferenceDto
                    {
                        TeacherPreferenceType = TeacherPreferenceType.Preferred,
                        RoomId = roomId,
                    },
                ])
            .Without(x => x.Comment)
            .Create();

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(teacherPreferenceToSave.ScheduleId))
            .ReturnsAsync(false);

        _teacherRepositoryMock.Setup(r => r.ExistsAsync(teacherPreferenceToSave.TeacherId))
            .ReturnsAsync(false);

        _roomRepositoryMock.Setup(r => r.SelectAsync(new[] { roomId }, CancellationToken.None))
            .ReturnsAsync([]);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(teacherPreferenceToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(5, actualException.ValidationMessages.Length);
    }

    [Fact]
    public async Task SaveAsync_Should_Merge_Time_Intervals()
    {
        // Arrange
        var preferencesToSave = _fixture.Build<SaveTeacherPreferenceDto>()
            .With(x => x.ScheduleId, Guid.NewGuid())
            .With(x => x.TeacherId, Guid.NewGuid())
            .With(x => x.TeacherTimeAvailabilities, [
                new TeacherTimeAvailabilityDto
                {
                    TeacherPreferenceType = TeacherPreferenceType.Restricted,
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval(DayOfWeek.Monday,
                        new TimeInterval(new TimeOnly(9, 0), new TimeOnly(12, 0))),
                },
                new TeacherTimeAvailabilityDto
                {
                    TeacherPreferenceType = TeacherPreferenceType.Restricted,
                    DayOfWeekTimeInterval = new DayOfWeekTimeInterval(DayOfWeek.Monday,
                        new TimeInterval(new TimeOnly(10, 0), new TimeOnly(15, 0))),
                },
            ])
            .Without(x => x.TeacherRoomPreferences)
            .Without(x => x.Comment)
            .Create();

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(preferencesToSave.ScheduleId))
            .ReturnsAsync(true);

        _teacherRepositoryMock.Setup(t => t.ExistsAsync(preferencesToSave.TeacherId))
            .ReturnsAsync(true);

        _teacherPreferenceRepositoryMock.Setup(r => r.SearchAsync(It.IsAny<TeacherPreferenceSearchModel>()))
            .ReturnsAsync([]);

        _teacherPreferenceRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid[]>(), CancellationToken.None));

        _lessonServiceMock.Setup(r => r.RecalculateConflictsForNewTeacherPreferences(It.IsAny<TeacherPreference[]>()));

        _teacherPreferenceRepositoryMock.Setup(r =>
            r.SaveAllAsync(It.IsAny<TeacherPreference[]>(), CancellationToken.None));

        var service = CreateService();

        // Act
        await service.SaveAsync(preferencesToSave);

        // Assert
        _teacherPreferenceRepositoryMock.Verify(x => x.SaveAllAsync(It.Is<TeacherPreference[]>(v =>
            v.Length == 1
            && v.First().TeacherPreferenceType == TeacherPreferenceType.Restricted
            && v.First().DayOfWeekTimeInterval!.DayOfWeek == DayOfWeek.Monday
            && v.First().DayOfWeekTimeInterval!.TimeInterval.Equals(new TimeInterval(new TimeOnly(9, 0), new TimeOnly(15, 0)))),
            CancellationToken.None));
    }
}