namespace Dal.Entities;

/// <summary>
/// Базовый интерфейс для сущностей с токенами конкурентного доступа.
/// </summary>
public interface IDbEntityWithConcurrencyToken : IDbEntityWithId
{
    /// <summary>
    /// ConcurrencyToken.
    /// </summary>
    uint ConcurrencyToken { get; set; }
}
