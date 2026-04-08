using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.RegistrySearchModels;

namespace Dal.RegistryRepositories.Room;

public class RoomRegistryPredicateBuilder : IPredicateBuilder<DbRoom, RoomRegistryInternalSearchModel>
{
    public Expression<Func<DbRoom, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbRoom>();

    public Expression<Func<DbRoom, bool>> Build(RoomRegistryInternalSearchModel searchModel)
    {
        return Predicate;
    }
}