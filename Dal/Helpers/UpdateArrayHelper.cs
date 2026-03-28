using Dal.Entities;
using Dal.Repositories;
using Domain.Models.Common;

namespace Dal.Helpers;

public static class UpdateArrayHelper
{
    public static void UpdateArray<TEntityItem, TModelItem>(this ICollection<TEntityItem> entities, TModelItem[] models,
        IRepositoryMapper<TEntityItem, TModelItem> mapper)
        where TEntityItem : IDbEntityWithId, new()
        where TModelItem : IModelWithId
    {
        entities.UpdateArray(models, mapper, item => item.Id, item => item.Id);
    }

    public static void UpdateArray<TEntityItem, TModelItem>(this ICollection<TEntityItem> entities, ICollection<TModelItem> models,
        Action<TEntityItem, TModelItem> updateAction)
        where TEntityItem : IDbEntityWithId, new()
        where TModelItem : IModelWithId
    {
        entities.UpdateArray(models, updateAction, item => item.Id, item => item.Id);
    }

    public static void UpdateArray<TEntityItem, TModelItem>(this ICollection<TEntityItem> entities, TModelItem[] models,
        IRepositoryMapper<TEntityItem, TModelItem> mapper, Func<TEntityItem, object> entityKey,
        Func<TModelItem, object?> modelKey)
        where TEntityItem : new()
    {
        entities.UpdateArray(models, mapper.Update, entityKey, modelKey);
    }

    public static void UpdateArray<TEntityItem, TModelItem>(this ICollection<TEntityItem> entities, ICollection<TModelItem> models,
        Action<TEntityItem, TModelItem> updateAction, Func<TEntityItem, object> entityKey,
        Func<TModelItem, object?> modelKey)
        where TEntityItem : new()
    {
        foreach (var item in models)
        {
            var dbItem = entities.FirstOrDefault(group => entityKey(group).Equals(modelKey(item)));
            if (dbItem != null)
            {
                updateAction(dbItem, item);
            }
            else
            {
                var newDbItem = new TEntityItem();
                updateAction(newDbItem, item);
                entities.Add(newDbItem);
            }
        }
    }
}
