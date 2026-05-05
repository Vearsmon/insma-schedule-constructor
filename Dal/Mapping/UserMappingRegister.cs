using Dal.Entities;
using Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class UserMappingRegister
{
    public static partial User? MapEntityToModel(DbUser? entity);
    public static partial DbUser? MapModelToEntity(User? model);
    public static partial void UpdateEntityWithModel(User? model, DbUser? entity);
}