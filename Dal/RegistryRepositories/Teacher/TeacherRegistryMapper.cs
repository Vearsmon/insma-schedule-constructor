using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.Teacher;

public class TeacherRegistryMapper : IReadonlyRepositoryMapper<DbTeacher, TeacherRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public TeacherRegistryItem? Map(DbTeacher? entity) => MappingRegister.MapRegistryItem(entity);
}