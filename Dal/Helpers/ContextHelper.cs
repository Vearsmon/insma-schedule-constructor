using System.Globalization;
using System.Reflection;
using Dal.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Dal.Helpers;

public static class ContextHelper
{
    #region Update

    public static void Update<T>(this DbContext context, T item) where T : class, IDbEntityWithId
    {
        SafeCall(() =>
        {
            AssertAttached(context, item);
            context.SaveChanges();
        });
    }

    public static Task UpdateAsync<T>(this DbContext context, T item, CancellationToken cancellationToken) where T : class, IDbEntityWithId
    {
        return SafeCallAsync(() =>
        {
            AssertAttached(context, item);
            return context.SaveChangesAsync(cancellationToken);
        });
    }

    public static void UpdateAll<T>(this DbContext context, params T[] items) where T : class, IDbEntityWithId
    {
        SafeCall(() =>
        {
            AssertAttached(context, items);
            context.SaveChanges();
        });
    }

    public static Task UpdateAllAsync<T>(this DbContext context, T[] items, CancellationToken cancellationToken) where T : class, IDbEntityWithId
    {
        return SafeCallAsync(() =>
        {
            AssertAttached(context, items);
            return context.SaveChangesAsync(cancellationToken);
        });
    }

    #endregion

    #region Create

    public static void Create<T>(this DbContext context, T item) where T : class, IDbEntityWithId
    {
        SafeCall(() =>
        {
            context.Add(item);
            context.SaveChanges();
        });
    }

    public static Task CreateAsync<T>(this DbContext context, T item, CancellationToken cancellationToken) where T : class, IDbEntityWithId
    {
        return SafeCallAsync(() =>
        {
            context.Add(item);
            return context.SaveChangesAsync(cancellationToken);
        });
    }

    public static void CreateAll<T>(this DbContext context, params T[] items) where T : class, IDbEntityWithId
    {
        SafeCall(() =>
        {
            context.Set<T>().AddRange(items);
            context.SaveChanges();
        });
    }

    public static Task CreateAllAsync<T>(this DbContext context, T[] items, CancellationToken cancellationToken) where T : class, IDbEntityWithId
    {
        return SafeCallAsync(() =>
        {
            context.Set<T>().AddRange(items);
            return context.SaveChangesAsync(cancellationToken);
        });
    }

    #endregion

    #region Delete

    public static void Delete<T>(this DbContext context, T data) where T : class, IDbEntityWithId
    {
        SafeCall(() =>
        {
            AssertAttached(context, data);
            context.Remove(data);
            context.SaveChanges();
        });
    }

    public static Task DeleteAsync<T>(this DbContext context, T data) where T : class, IDbEntityWithId
    {
        return SafeCallAsync(() =>
        {
            AssertAttached(context, data);
            context.Remove(data);
            return context.SaveChangesAsync();
        });
    }

    public static void DeleteAll<T>(this DbContext context, params T[] items) where T : class, IDbEntityWithId
    {
        SafeCall(() =>
        {
            AssertAttached(context, items);
            context.Set<T>().RemoveRange(items);
            context.SaveChanges();
        });
    }

    public static Task DeleteAllAsync<T>(this DbContext context, params T[] items) where T : class, IDbEntityWithId
    {
        return SafeCallAsync(() =>
        {
            AssertAttached(context, items);
            context.Set<T>().RemoveRange(items);
            return context.SaveChangesAsync();
        });
    }

    #endregion

    private static void SafeCall(Action action)
    {
        try
        {
            action();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrentCallDbException(e);
        }
        catch (Exception e)
        {
            HandlePostgresException(e);
        }
    }

    private static async Task SafeCallAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrentCallDbException(e);
        }
        catch (Exception e)
        {
            HandlePostgresException(e);
        }
    }

    private static void AssertAttached<T>(DbContext context, params T[] items) where T : class, IDbEntityWithId
    {
        foreach (var item in items)
        {
            AssertAttached(context, item);
        }
    }

    private static void AssertAttached<T>(DbContext context, T item) where T : class, IDbEntityWithId
    {
        var entityEntry = context.Entry(item);
        if (entityEntry == null || entityEntry.State == EntityState.Detached)
        {
            throw new ItemDetachedDbException(item);
        }
    }

    private static void HandlePostgresException(Exception exception)
    {
        if (exception.InnerException is PostgresException { SqlState: "23505" } pgException)
        {
            throw new UniqueFieldDbException(pgException.Detail!, pgException);
        }

        throw exception;
    }

    private static string GetValue<T>(PropertyInfo info, T item) where T : class, IDbEntityWithId
    {
        var isDate = info.PropertyType == typeof(DateTime) || info.PropertyType == typeof(DateTime?);
        var value = isDate ?
            ((DateTime?)info.GetValue(item))?.ToString("o", CultureInfo.InvariantCulture) :
            info.GetValue(item);
        return value is null ?
            "null" :
            $"'{value.ToString()!.Replace("'", "''")}'";
    }
}
