using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.AcademicDiscipline;

public class AcademicDisciplineRegistryPredicateBuilder
    : IPredicateBuilder<DbAcademicDiscipline, AcademicDisciplineRegistryInternalSearchModel>
{
    public Expression<Func<DbAcademicDiscipline, bool>> Predicate { get; } =
        PredicateBuilderExtensions.True<DbAcademicDiscipline>();

    public Expression<Func<DbAcademicDiscipline, bool>> Build(
        AcademicDisciplineRegistryInternalSearchModel searchModel)
    {
        return Predicate;
    }
}