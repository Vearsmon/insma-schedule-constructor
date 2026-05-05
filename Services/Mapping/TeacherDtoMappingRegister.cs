using Domain.Dto.RegistryDto;
using Domain.Dto.SaveDto;
using Domain.Dto.ShortDto;
using Domain.Dto.ViewDto;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class TeacherDtoMappingRegister
{
    public static partial TeacherViewDto? MapModelToViewDto(Teacher? model);
    public static partial Teacher? MapSaveDtoToModel(TeacherSaveDto? dto);
    public static partial TeacherRegistryItemDto? MapItemToItemDto(TeacherRegistryItem? item);
    public static partial TeacherShortDto? MapModelToShortDto(Teacher? model);
}