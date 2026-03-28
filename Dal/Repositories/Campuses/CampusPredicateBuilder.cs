using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Campuses;

public class CampusPredicateBuilder : IPredicateBuilder<DbCampus, CampusSearchModel>
{
    public Expression<Func<DbCampus, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbCampus>();

    public Expression<Func<DbCampus, bool>> Build(CampusSearchModel searchModel)
    {
        return Predicate
            ;
    }
}