using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Campus;

public class CampusRegistryPredicateBuilder : IPredicateBuilder<DbCampus, CampusRegistryInternalSearchModel>
{
    public Expression<Func<DbCampus, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbCampus>();

    public Expression<Func<DbCampus, bool>> Build(CampusRegistryInternalSearchModel searchModel)
    {
        return Predicate;
    }
}