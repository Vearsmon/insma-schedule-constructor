using System.Diagnostics.CodeAnalysis;

namespace Dal.Repositories;

public interface IReadonlyRepositoryMapper<in TEntity, out TModel>
{
    [return: NotNullIfNotNull(nameof(entity))]
    TModel? Map(TEntity? entity);
}
