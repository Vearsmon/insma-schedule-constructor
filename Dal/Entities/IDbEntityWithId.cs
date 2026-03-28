namespace Dal.Entities;

/// <summary>
/// Базовый интерфейс для сущностей базы данных c Id.
/// </summary>
public interface IDbEntityWithId : IDbEntity
{
    /// <summary>
    /// Id.
    /// </summary>
    Guid Id { get; set; }
}