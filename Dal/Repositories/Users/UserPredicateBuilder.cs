using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Users;

public class UserPredicateBuilder : IPredicateBuilder<DbUser, UserSearchModel>
{
    public Expression<Func<DbUser, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbUser>();

    public Expression<Func<DbUser, bool>> Build(UserSearchModel searchModel)
    {
        return Predicate;
    }
}