using Domain.Dto.RegistryDto;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Services.Mapping;

[Mapper]
public static partial class TeacherPreferenceDtoMappingRegister
{
    public static partial TeacherPreferenceRegistryItemDto? MapItemToItemDto(TeacherPreferenceRegistryItem? item);
}