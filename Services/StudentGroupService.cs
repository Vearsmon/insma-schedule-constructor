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
        return threadGroups.Select(x => new StudentGroupShortDto { Id = x.Id!.Value, Name = x.Name }).ToArray();
    }

    public async Task<StudentGroupTreeDto[]> SearchTreeAsync(Guid scheduleId)
    {
        var threadGroups = await studentGroupRepository.SearchAsync(new StudentGroupSearchModel
        {
            ScheduleId = scheduleId,
            StudentGroupTypes = [StudentGroupType.Thread],
        });
        return threadGroups.Select(x => new StudentGroupTreeDto
        {
            Id = x.Id!.Value,
            Name = x.Name,
            Children = x.Children.Select(y => new StudentGroupTreeDto
            {
                Id = y.Id!.Value,
                Name = y.Name,
                Children = y.Children.Select(z => new StudentGroupTreeDto
                {
                    Id = z.Id!.Value,
                    Name = z.Name
                }).ToArray()
            }).ToArray()
        }).ToArray();
    }

    public async Task<RegistryDto<StudentGroupRegistryItemDto>> SearchAsync(StudentGroupRegistrySearchModel searchModel)
    {
        var registryEntries =
            await studentGroupRegistryRepository.SearchAsync(RegistrySearchModelMappingRegister.Map(searchModel));
        return new RegistryDto<StudentGroupRegistryItemDto>
        {
            Items = registryEntries.Items.Select(DtoMappingRegister.Map).ToArray()!,
            ItemsCount = registryEntries.ItemsCount,
        };
    }

    public async Task<StudentGroupViewDto> GetViewAsync(Guid studentGroupId)
    {
        var studentGroup = await studentGroupRepository.GetAsync(studentGroupId);
        return DtoMappingRegister.Map(studentGroup)!;
    }

    public async Task SaveAsync(SaveStudentGroupDto saveStudentGroupDto)
    {
        var validationMessages = new List<ValidationMessage>();
        if (saveStudentGroupDto.Name == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие названия"));
        }

        if (saveStudentGroupDto.SemesterNumber is < 1 or > 12)
        {
            validationMessages.Add(new ValidationMessage(
                $"Указанный номер семестра ({saveStudentGroupDto.SemesterNumber}) должен лежать в интервале от 1 до 12"));
        }

        if (saveStudentGroupDto.Cypher == null!)
        {
            validationMessages.Add(new ValidationMessage("Не допускается отсутствие шифра"));
        }

        if (saveStudentGroupDto is { StudentGroupType: StudentGroupType.Thread, ParentId: not null })
        {
            validationMessages.Add(new ValidationMessage("При создании потока не может указываться группа-предок"));
        }

        if (saveStudentGroupDto is { StudentGroupType: StudentGroupType.Thread, Cypher: null })
        {
            validationMessages.Add(new ValidationMessage("При создании потока должен быть указан шифр"));
        }

        if (saveStudentGroupDto is { StudentGroupType: StudentGroupType.Thread, SemesterNumber: null })
        {
            validationMessages.Add(new ValidationMessage("При создании потока должен быть указан номер семестра"));
        }

        if (saveStudentGroupDto is { StudentGroupType: StudentGroupType.SemiGroup, ChildIds.Length: > 0 })
        {
            validationMessages.Add(
                new ValidationMessage("При создании подгруппы не могут указываться группы-наследники"));
        }

        if (!(await scheduleRepository.ExistsAsync(saveStudentGroupDto.ScheduleId)))
        {
            validationMessages.Add(
                new ValidationMessage("Не найден проект расписания для сохранения академической группы"));
        }

        if (saveStudentGroupDto.ParentId.HasValue
            && !(await studentGroupRepository.ExistsAsync(saveStudentGroupDto.ParentId!.Value)))
        {
            validationMessages.Add(
                new ValidationMessage("Не найдена группа-предок для сохранения академической группы"));
        }

        var previousStudentGroup = saveStudentGroupDto.Id.HasValue
            ? await studentGroupRepository.GetAsync(saveStudentGroupDto.Id!.Value)
            : null;
        if (previousStudentGroup != null && saveStudentGroupDto.ScheduleId != previousStudentGroup.ScheduleId)
        {
            validationMessages.Add(
                new ValidationMessage("Запрещено менять проект расписания для академической группы"));
        }

        var children = Array.Empty<StudentGroup>();
        var childIds = saveStudentGroupDto.ChildIds.Distinct().ToArray();
        if (childIds.Length != 0)
        {
            children = await studentGroupRepository.SelectAsync(childIds);
            if (children.Length != childIds.Length)
            {
                validationMessages.Add(
                    new ValidationMessage("Не найдены группы-наследники для сохранения академической группы"));
            }
        }

        if (validationMessages.Count > 0)
        {
            throw new ServiceException(validationMessages.ToArray());
        }

        var studentGroup = DtoMappingRegister.Map(saveStudentGroupDto)!;
        studentGroup.Children = children;
        await studentGroupRepository.SaveAsync(studentGroup);

        if (saveStudentGroupDto.Id.HasValue)
        {
            await lessonService.RecalculateConflictsForNewStudentGroup(studentGroup);
        }
    }

    public async Task<string[]> SearchCyphersAsync(Guid scheduleId)
    {
        return await studentGroupRepository.SearchCyphersAsync(scheduleId);
    }

    public async Task DeleteAsync(Guid studentGroupId)
    {
        await studentGroupRepository.DeleteAsync(studentGroupId);
    }
}