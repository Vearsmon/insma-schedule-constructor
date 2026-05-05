using Dal.Entities;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class CampusMappingRegister
{
    public static partial Campus? MapEntityToModel(DbCampus? entity);
    public static partial DbCampus? MapModelToEntity(Campus? model);
    public static partial void UpdateEntityWithModel(Campus? model, DbCampus? entity);
    public static partial CampusRegistryItem? MapEntityToRegistryItem(DbCampus? entity);
}