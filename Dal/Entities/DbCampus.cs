using System.ComponentModel.DataAnnotations;

namespace Dal.Entities;

/// <summary>
/// Учебный корпус
/// </summary>
public class DbCampus : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Название учебного корпуса
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; } = null!;
}