using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Аудитория
/// </summary>
public class Room : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Название аудитории
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Учебный корпус
    /// </summary>
    public Guid CampusId { get; set; }

    /// <summary>
    /// Учебный корпус
    /// </summary>
    public Campus Campus { get; set; } = null!;

    /// <summary>
    /// Тип аудитории
    /// </summary>
    public RoomType RoomType { get; set; }

    /// <summary>
    /// Вместимость
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Тип доски
    /// </summary>
    public RoomBoardType RoomBoardType { get; set; }

    /// <summary>
    /// Имеет в наличии проектор
    /// </summary>
    public bool HasProjector { get; set; }
}