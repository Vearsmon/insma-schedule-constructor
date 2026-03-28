using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.StudentGroup;

public class StudentGroupRegistryMapper : IReadonlyRepositoryMapper<DbStudentGroup, StudentGroupRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public StudentGroupRegistryItem? Map(DbStudentGroup? entity) => MappingRegister.MapRegistryItem(entity);
}