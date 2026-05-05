using Dal.Entities;
using Domain.Models;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class TeacherMappingRegister
{
    public static partial Teacher? MapEntityToModel(DbTeacher? entity);
    public static partial DbTeacher? MapModelToEntity(Teacher? model);
    public static partial void UpdateEntityWithModel(Teacher? model, DbTeacher? entity);
    public static partial TeacherRegistryItem? MapEntityToRegistryItem(DbTeacher? entity);
}