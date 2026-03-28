using Dal.Entities;
using Dal.Helpers;
using Dal.Transactions;
using Domain.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Dal.Repositories;

public abstract class Repository<TDbContext, TDbEntity, TModel>(
    TDbContext context,
    IRepositoryMapper<TDbEntity, TModel> mapper,
    ITransactionalService transactionService)
    : ReadonlyRepository<TDbContext, TDbEntity, TModel>(context, mapper),
        IRepository<TModel>
    where TDbContext : DbContext
    where TDbEntity : class, IDbEntityWithId, new()
    where TModel : IModelWithId
{
    protected readonly IRepositoryMapper<TDbEntity, TModel> Mapper = mapper;

    public virtual async Task<Guid> SaveAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var ids = await SaveAllInternalAsync([model], Mapper, cancellationToken);
        return ids[0];
    }

    public virtual async Task<Guid[]> SaveAllAsync(TModel[] models, CancellationToken cancellationToken = default)
    {
        return await SaveAllInternalAsync(models, Mapper, cancellationToken);
    }

    public virtual Task DeleteAsync(Guid id) => DeleteAsync(new[] { id }, CancellationToken.None);

    public virtual async Task DeleteAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        await BaseQuery().Where(x => ids.Contains(x.Id))
                         .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        await Query().ExecuteDeleteAsync(cancellationToken);
    }

    protected void ThrowIfConcurrencyConflicts(IDbEntityWithConcurrencyToken[] dbEntities, IModelWithConcurrencyToken[] models)
    {
        var dbConcurrencyTokens = dbEntities.ToDictionary(x => x.Id, x => x.ConcurrencyToken);
        var modelConcurrencyTokens = models.Where(x => x.Id != null)
            .ToDictionary(x => x.Id!.Value, x => x.ConcurrencyToken);

        if (dbConcurrencyTokens.Any(x => modelConcurrencyTokens[x.Key] != x.Value))
        {
            throw new DbUpdateConcurrencyException($"Конфликт параллельного доступа к сущности типа {dbEntities.First().GetType().Name}.");
        }
    }

    protected TDbEntity[] GetEntitiesToUpdate<TModelType>(TDbEntity[] dbEntities, TModelType[] models,
        IRepositoryMapper<TDbEntity, TModelType> mapper) where TModelType : IModelWithId
    {
        if (typeof(TModelType).GetInterfaces().Any(x => x == typeof(IModelWithConcurrencyToken)))
        {
            ThrowIfConcurrencyConflicts(dbEntities.Select(x => (IDbEntityWithConcurrencyToken)x).ToArray(),
                models.Select(x => (IModelWithConcurrencyToken)x).ToArray());
        }
        return dbEntities.Join(models,
                               db => db.Id,
                               model => model.Id,
                               (db, model) =>
                               {
                                   mapper.Update(db, model);
                                   return db;
                               })
                         .ToArray();
    }

    protected async Task<TDbEntity[]> GetEntitiesAsync<TModelType>(TModelType[] models,
        CancellationToken cancellationToken) where TModelType : IModelWithId
    {
        var ids = models.Where(x => x.Id != Guid.Empty).Select(x => x.Id);

        var query = Query().Where(x => ids.Any(id => x.Id == id));

        var result = await query.ToArrayAsync(cancellationToken);
        return result;
    }

    protected TDbEntity[] GetEntitiesToAdd<TModelType>(TModelType[] models, TDbEntity[] existingInDb,
        IRepositoryMapper<TDbEntity, TModelType> mapper) where TModelType : IModelWithId
    {
        var toUpdateDic = existingInDb.ToDictionary(x => x.Id);
        var toAddModels = models.Where(x => x.Id == null || !toUpdateDic.ContainsKey(x.Id!.Value)).ToArray();

        var result = new List<TDbEntity>(toAddModels.Length);
        foreach (var model in toAddModels)
        {
            result.Add(new TDbEntity());
            mapper.Update(result.Last(), model);
        }

        return result.ToArray();
    }

    protected async Task<Guid[]> SaveAllInternalAsync<TModelType>(TModelType[] models,
        IRepositoryMapper<TDbEntity, TModelType> mapper, CancellationToken cancellationToken) where TModelType : IModelWithId
    {
        var toUpdate = await GetEntitiesToUpdateAsync(models, mapper, cancellationToken);
        var toAdd = GetEntitiesToAdd(models, toUpdate, mapper);

        if (toUpdate.Length > 0 && toAdd.Length > 0)
        {
            await transactionService.ExecuteInTransactionAsync(SaveAction);
        }
        else
        {
            await SaveAction();
        }

        UpdateConcurrencyTokensIfNeeded(models, toUpdate, toAdd);

        return toUpdate.Concat(toAdd)
                       .Select(x => x.Id)
                       .ToArray();

        async Task SaveAction()
        {
            await Context.UpdateAllAsync(toUpdate, cancellationToken);
            await Context.CreateAllAsync(toAdd, cancellationToken);
        }
    }

    private async Task<TDbEntity[]> GetEntitiesToUpdateAsync<TModelType>(TModelType[] models,
        IRepositoryMapper<TDbEntity, TModelType> mapper, CancellationToken cancellationToken) where TModelType : IModelWithId
    {
        var result = await GetEntitiesAsync(models, cancellationToken);
        return GetEntitiesToUpdate(result, models, mapper);
    }

    private void UpdateConcurrencyTokensIfNeeded<TModelType>(TModelType[] models, TDbEntity[] toUpdate, TDbEntity[] toAdd) where TModelType : IModelWithId
    {
        if ((toUpdate.Length <= 0 && toAdd.Length <= 0)
            || typeof(TModelType).GetInterfaces().All(x => x != typeof(IModelWithConcurrencyToken)))
        {
            return;
        }
        var concurrencyTokenToAdd = toAdd.Length > 0 ? ((IDbEntityWithConcurrencyToken)toAdd.First()).ConcurrencyToken : 0;
        var dbConcurrencyTokens = toUpdate.ToDictionary(x => x.Id, x => ((IDbEntityWithConcurrencyToken)x).ConcurrencyToken);
        foreach (var model in models)
        {
            if (model.Id == null)
            {
                ((IModelWithConcurrencyToken)model).ConcurrencyToken = concurrencyTokenToAdd;
            }
            else if (dbConcurrencyTokens.TryGetValue(model.Id!.Value, out var token))
            {
                ((IModelWithConcurrencyToken)model).ConcurrencyToken = token;
            }
        }
    }
}
