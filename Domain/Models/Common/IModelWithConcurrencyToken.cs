namespace Domain.Models.Common;

/// <summary>
/// Базовый интерфейс для моделей сущностей с токенами конкурентного доступа.
/// </summary>
public interface IModelWithConcurrencyToken : IModelWithId
{
    /// <summary>
    /// ConcurrencyToken.
    /// </summary>
    uint ConcurrencyToken { get; set; }
}
