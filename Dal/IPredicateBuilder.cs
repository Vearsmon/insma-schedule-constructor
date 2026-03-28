using System.Linq.Expressions;

namespace Dal;

public interface IPredicateBuilder<TEntity, in TSearchModel>
{
    Expression<Func<TEntity, bool>> Predicate { get; }

    Expression<Func<TEntity, bool>> Build(TSearchModel searchModel);
}