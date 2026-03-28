namespace Domain.Models.Common;

/// <summary>
/// Интерфейс пагинации для автокомплита.
/// </summary>
public interface IAutocompletePaginated
{
    /// <summary>
    /// Пропускаемое количество записей.
    /// </summary>
    int Skip { get; set; }

    /// <summary>
    /// Возвращаемое количество записей.
    /// </summary>
    int Take { get; set; }
}
