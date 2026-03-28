using System.Linq.Expressions;
using Domain.Dto.RegistryDto;
using Domain.Models.Common;

namespace Dal.RegistryRepositories;

public class RegistryItemOrderer<TModel, TSearchModel> : Orderer<TModel, TSearchModel>
    where TSearchModel : IWithSearchParameters
    where TModel : IModelWithId
{
    protected override Expression<Func<TModel, object>> DefaultOrderExpression => x => x.Id!;
}
