using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.Campus;

public class CampusRegistryMapper : IReadonlyRepositoryMapper<DbCampus, CampusRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public CampusRegistryItem? Map(DbCampus? entity) => MappingRegister.MapRegistryItem(entity);
}