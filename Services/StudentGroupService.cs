using Dal.RegistryRepositories.StudentGroup;
using Dal.Repositories.StudentGroups;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ViewDto;
using Domain.Mapping;
using Domain.Models.RegistrySearchModels;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class StudentGroupService(
    IStudentGroupRepository studentGroupRepository,
    IStudentGroupRegistryRepository studentGroupRegistryRepository,
    ILessonService lessonService) : IStudentGroupService
{
    public async Task<RegistryDto<StudentGroupRegistryItemDto>> SearchAsync(StudentGroupRegistrySearchModel searchModel)
    {
        var registryEntries = await studentGroupRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<StudentGroupRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray(),
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<StudentGroupViewDto> GetViewAsync(Guid studentGroupId)
    {
        var studentGroup = await studentGroupRepository.GetAsync(studentGroupId);
        return DtoMappingRegister.Map(studentGroup)!;
    }

    public async Task<Guid> SaveAsync(SaveStudentGroupDto saveStudentGroupDto)
    {
        if (saveStudentGroupDto.ParentId.HasValue
            && !(await studentGroupRepository.ExistsAsync(saveStudentGroupDto.ParentId!.Value))
            || saveStudentGroupDto.ChildIds.Length > 0
            && (await studentGroupRepository.SelectAsync(saveStudentGroupDto.ChildIds)).Length == saveStudentGroupDto.ChildIds.Length)
        {
            throw new NotImplementedException();
        }
        var previousStudentGroup = saveStudentGroupDto.Id.HasValue ? await studentGroupRepository.FindAsync(saveStudentGroupDto.Id!.Value) : null;
        if (previousStudentGroup != null && saveStudentGroupDto.ScheduleId != previousStudentGroup.ScheduleId)
        {
            throw new NotImplementedException();
        }

        var children = await studentGroupRepository.SelectAsync(saveStudentGroupDto.ChildIds);

        var studentGroup = DtoMappingRegister.Map(saveStudentGroupDto)!;
        studentGroup.Children = children;
        var id = await studentGroupRepository.SaveAsync(studentGroup);
        if (saveStudentGroupDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForNewStudentGroup(studentGroup);
        }
        return id;
    }
}