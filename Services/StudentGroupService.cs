using Dal.RegistryRepositories.StudentGroup;
using Dal.Repositories.Schedules;
using Dal.Repositories.StudentGroups;
using Domain.Dto;
using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Exceptions;
using Domain.Mapping;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Models.RegistrySearchModels;
using Domain.Models.SearchModels;
using Domain.Models.ValidationMessages;
using Domain.Services;
using Services.Mapping;

namespace Services;

public class StudentGroupService(
    IStudentGroupRepository studentGroupRepository,
    IStudentGroupRegistryRepository studentGroupRegistryRepository,
    IScheduleRepository scheduleRepository,
    ILessonService lessonService) : IStudentGroupService
{
    public async Task<StudentGroupShortDto[]> SearchRootAsync(Guid scheduleId)
    {
        var threadGroups = await studentGroupRepository.SearchAsync(new StudentGroupSearchModel
        {
            ScheduleId = scheduleId,
            StudentGroupTypes = [StudentGroupType.Thread],
        });
        return threadGroups.Select(StudentGroupDtoMappingRegister.MapModelToShortDto).ToArray()!;
    }

    public async Task<StudentGroupTreeDto[]> SearchTreeAsync(Guid scheduleId)
    {
        var threadGroups = await studentGroupRepository.SearchAsync(new StudentGroupSearchModel
        {
            ScheduleId = scheduleId,
            StudentGroupTypes = [StudentGroupType.Thread],
            IncludeGroupsWithoutParents = true,
        });
        return threadGroups.Select(StudentGroupDtoMappingRegister.MapModelToTreeDto).ToArray()!;
    }

    public async Task<RegistryDto<StudentGroupRegistryItemDto>> SearchAsync(StudentGroupRegistrySearchModel searchModel)
    {
        var registryEntries =
            await studentGroupRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<StudentGroupRegistryItemDto>
        {
            Items = registryEntries.Items.Select(StudentGroupDtoMappingRegister.MapItemToItemDto).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<StudentGroupViewDto> GetViewAsync(Guid studentGroupId)
    {
        var studentGroup = await studentGroupRepository.GetAsync(studentGroupId);
        return StudentGroupDtoMappingRegister.MapModelToViewDto(studentGroup)!;
    }

    public async Task SaveAsync(StudentGroupSaveDto studentGroupSaveDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (studentGroupSaveDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }

        if (studentGroupSaveDto.SemesterNumber is < 1 or > 12)
        {
            validationMessages.Add(new ValidationMessage(
                $"Указанный номер семестра ({studentGroupSaveDto.SemesterNumber}) должен лежать в интервале от 1 до 12"));
        }

        if (studentGroupSaveDto is { StudentGroupType: StudentGroupType.Thread, ParentIds.Length: > 0 })
        {
            validationMessages.Add(new ValidationMessage("При создании потока не может указываться группа-предок"));
        }

        if (studentGroupSaveDto is { StudentGroupType: StudentGroupType.SemiGroup, Children.Length: > 0 })
        {
            validationMessages.Add(
                new ValidationMessage("При создании подгруппы не могут указываться группы-наследники"));
        }

        if (!await scheduleRepository.ExistsAsync(studentGroupSaveDto.ScheduleId))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден проект расписания для сохранения академической группы"));
        }

        var parentIds = studentGroupSaveDto.ParentIds.Distinct().ToArray();
        if (parentIds.Length != 0)
        {
            var parents = await studentGroupRepository.SelectAsync(parentIds);
            if (parents.Length != parentIds.Length)
            {
                validationMessages.Add(
                    new ValidationMessage("Не найдены группы-предки для сохранения академической группы"));
            }
        }

        var childIds = studentGroupSaveDto.Children.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).Distinct().ToArray();
        if (childIds.Length != 0)
        {
            var children = await studentGroupRepository.SelectAsync(childIds);
            if (children.Length != childIds.Length)
            {
                validationMessages.Add(
                    new ValidationMessage("Не найдены группы-наследники для сохранения академической группы"));
            }
        }

        var previousStudentGroup = studentGroupSaveDto.Id.HasValue
            ? await studentGroupRepository.GetAsync(studentGroupSaveDto.Id!.Value)
            : null;
        if (previousStudentGroup != null && studentGroupSaveDto.ScheduleId != previousStudentGroup.ScheduleId)
        {
            validationMessages.Add(
                new ValidationMessage("Запрещено менять проект расписания для академической группы"));
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        StudentGroup studentGroup;
        var id = studentGroupSaveDto.Id;
        if (id.HasValue)
        {
            studentGroup = await studentGroupRepository.GetAsync(studentGroupSaveDto.Id!.Value);
            StudentGroupDtoMappingRegister.UpdateModelWithSaveDto(studentGroupSaveDto, studentGroup);
            await studentGroupRepository.SaveAsync(studentGroup);
        }
        else
        {
            studentGroup = StudentGroupDtoMappingRegister.MapSaveDtoToModel(studentGroupSaveDto)!;
            id = await studentGroupRepository.SaveAsync(studentGroup);
        }

        var previousSemiGroupsSaveDto = studentGroupSaveDto.Children
            .Where(x => x.Id.HasValue)
            .ToArray();
        var previousSemiGroups =
            await studentGroupRepository.SelectAsync(previousSemiGroupsSaveDto.Select(x => x.Id!.Value).ToArray());
        foreach (var previousSemiGroup in previousSemiGroups)
        {
            previousSemiGroup.Name = previousSemiGroupsSaveDto.Single(x => x.Id == previousSemiGroup.Id).Name;
        }
        var newSemiGroups = studentGroupSaveDto.Children
            .Where(x => !x.Id.HasValue)
            .Select(semiGroup => new StudentGroup
            {
                ScheduleId = studentGroupSaveDto.ScheduleId,
                Name = semiGroup.Name,
                SemesterNumber = studentGroupSaveDto.SemesterNumber,
                StudentGroupType = StudentGroupType.SemiGroup,
                Parents = [new StudentGroup { Id = id!.Value }],
            }).ToArray();

        if (newSemiGroups.Length > 0 || previousSemiGroups.Length > 0)
        {
            await studentGroupRepository.SaveAllAsync(newSemiGroups
                .Concat(studentGroupSaveDto.StudentGroupType == StudentGroupType.Thread ? [] : previousSemiGroups)
                .ToArray());
        }

        if (studentGroupSaveDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForNewStudentGroup(studentGroup);
        }
    }

    public async Task DeleteAsync(Guid studentGroupId)
    {
        var studentGroup = await studentGroupRepository.GetAsync(studentGroupId);
        switch (studentGroup.StudentGroupType)
        {
            case StudentGroupType.Thread:
                studentGroup.Children = [];
                await studentGroupRepository.SaveAsync(studentGroup);
                break;
            case StudentGroupType.Group:
                await studentGroupRepository.DeleteAsync(studentGroup.Children.Select(x => x.Id!.Value).ToArray());
                break;
            case StudentGroupType.SemiGroup:
            default:
                break;
        }

        await studentGroupRepository.DeleteAsync(studentGroupId);
    }
}