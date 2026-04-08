using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>
    /// Расписание начинается с четной недели
    /// </summary>
    public bool StartsWithEvenWeek { get; set; }

    /// <summary>
    /// Дата начала проведения занятий
    /// </summary>
    [Column(TypeName = DbDataTypes.Date)]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Дата завершения проведения занятий
    /// </summary>
    [Column(TypeName = DbDataTypes.Date)]
    public DateOnly EndDate { get; set; }
}