using System.ComponentModel.DataAnnotations;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Аудитория
/// </summary>
public class DbRoom : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Название аудитории
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Учебный корпус
    /// </summary>
    public Guid CampusId { get; set; }

    /// <summary>
    /// Учебный корпус
    /// </summary>
    public DbCampus Campus { get; set; } = null!;

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