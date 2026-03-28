using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.AcademicDiscipline;

public class AcademicDisciplineRegistryMapper
    : IReadonlyRepositoryMapper<DbAcademicDiscipline, AcademicDisciplineRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public AcademicDisciplineRegistryItem? Map(DbAcademicDiscipline? entity) =>
        MappingRegister.MapRegistryItem(entity);
}