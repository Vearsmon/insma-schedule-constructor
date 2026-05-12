using AutoFixture;
using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Lessons;
using Dal.Repositories.Schedules;
using Domain.Dto.SaveDto;
using Domain.Exceptions;
using Domain.Models.Enums;
using Domain.Services;
using Moq;
using Services;

namespace Tests;

public class AcademicDisciplineServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IAcademicDisciplineRepository> _academicDisciplineRepositoryMock = new();
    private readonly Mock<IAcademicDisciplineRegistryRepository> _academicDisciplineRegistryRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<ILessonService> _lessonServiceMock = new();
    private readonly Mock<ILessonRepository> _lessonRepositoryMock = new();

    private AcademicDisciplineService CreateService() => new(
        _academicDisciplineRepositoryMock.Object,
        _academicDisciplineRegistryRepositoryMock.Object,
        _scheduleRepositoryMock.Object,
        _lessonServiceMock.Object,
        _lessonRepositoryMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var academicDisciplineToSave = _fixture.Build<AcademicDisciplineSaveDto>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Name)
            .With(x => x.SemesterNumber, 0)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [])
            .With(x => x.LectureLessonBatchInfos)
            .With(x => x.PracticeLessonBatchInfos)
            .With(x => x.LabLessonBatchInfos)
            .With(x => x.ExamLessonBatchInfos)
            .With(x => x.TestLessonBatchInfos)
            .Without(x => x.Comment)
            .Create();

        _academicDisciplineRepositoryMock.Setup(r => r.ExistsAsync(academicDisciplineToSave.Id!.Value))
            .ReturnsAsync(false);

        _scheduleRepositoryMock.Setup(r => r.ExistsAsync(academicDisciplineToSave.ScheduleId))
            .ReturnsAsync(false);

        var service = CreateService();
        var serviceFunc = () => service.SaveAsync(academicDisciplineToSave);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<ServiceException>(serviceFunc);
        Assert.Equal(8, actualException.ValidationMessages.Length);
    }
}