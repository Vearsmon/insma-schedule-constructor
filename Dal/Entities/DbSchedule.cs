using System.ComponentModel.DataAnnotations;

namespace Dal.Entities;

/// <summary>
/// Проект расписания
/// </summary>
public class DbSchedule : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Название проекта расписания
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; } = null!;
}