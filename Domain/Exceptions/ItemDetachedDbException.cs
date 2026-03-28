using System.Data.Common;

namespace Domain.Exceptions;

/// <summary>
/// Генерируется контекстом данных, если при попытке изменения или удаления сущности она не была найдена в контексте,
/// либо отсоединена от контекста.
/// </summary>
public class ItemDetachedDbException : DbException
{
    public ItemDetachedDbException(object item) : base($"Item {item} detached from current context")
    {
    }
}
