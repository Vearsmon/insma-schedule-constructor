using System.Linq.Expressions;
using Dal.Entities;
using Dal.Helpers;
using Domain.Models.SearchModels;

namespace Dal.Repositories.Rooms;

public class RoomPredicateBuilder : IPredicateBuilder<DbRoom, RoomSearchModel>
{
    public Expression<Func<DbRoom, bool>> Predicate { get; } = PredicateBuilderExtensions.True<DbRoom>();

    public Expression<Func<DbRoom, bool>> Build(RoomSearchModel searchModel)
    {
        return Predicate;
    }
}