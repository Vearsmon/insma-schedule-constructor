using Dal.Entities;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.AcademicDiscipline;

public class AcademicDisciplineRegistryOrderer
    : IRegistryRepositoryOrderer<DbAcademicDiscipline, AcademicDisciplineRegistryInternalSearchModel>
{
    public IQueryable<DbAcademicDiscipline> Order(IQueryable<DbAcademicDiscipline> queryable,
        AcademicDisciplineRegistryInternalSearchModel searchModel)
    {
        return queryable;
    }
}