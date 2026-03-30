using AutoFixture;
using Dal.RegistryRepositories.AcademicDiscipline;
using Dal.Repositories.AcademicDisciplines;
using Dal.Repositories.Schedules;
using Domain.Dto;
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

    private AcademicDisciplineService CreateService() => new(
        _academicDisciplineRepositoryMock.Object,
        _academicDisciplineRegistryRepositoryMock.Object,
        _scheduleRepositoryMock.Object,
        _lessonServiceMock.Object
    );

    [Fact]
    public async Task SaveAsync_Should_Throw_When_Invalid_Data()
    {
        // Arrange
        var academicDisciplineToSave = _fixture.Build<SaveAcademicDisciplineDto>()
            .With(x => x.Id, Guid.NewGuid())
            .With(x => x.ScheduleId, Guid.NewGuid())
            .Without(x => x.Name)
            .Without(x => x.Cypher)
            .With(x => x.SemesterNumber, 0)
            .With(x => x.AcademicDisciplineTargetType, AcademicDisciplineTargetType.ByChoice)
            .With(x => x.AllowedLessonTypes, [])
            .With(x => x.LecturePayload, new AcademicDisciplinePayloadDto())
            .With(x => x.PracticePayload, new AcademicDisciplinePayloadDto())
            .With(x => x.LabPayload, new AcademicDisciplinePayloadDto())
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