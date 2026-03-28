using System.Linq.Expressions;
using Dal.Entities;
using Domain.Dto.RegistryDto;

namespace Dal.RegistryRepositories;

public class EntityOrderer<TEntity, TSearchModel> : Orderer<TEntity, TSearchModel>
    where TSearchModel : IWithSearchParameters
    where TEntity : IDbEntityWithId
{
    protected override Expression<Func<TEntity, object>> DefaultOrderExpression => x => x.Id;
}
