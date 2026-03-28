using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.TeacherPreference;

public class TeacherPreferenceRegistryMapper
    : IReadonlyRepositoryMapper<DbTeacherPreference, TeacherPreferenceRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public TeacherPreferenceRegistryItem? Map(DbTeacherPreference? entity) => MappingRegister.MapRegistryItem(entity);
}