using Domain.Models.Common;

namespace Domain.Models;

/// <summary>
/// Проект расписания
/// </summary>
public class Schedule : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Название проекта расписания
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Расписание начинается с четной недели
    /// </summary>
    public bool StartsWithEvenWeek { get; set; }

    /// <summary>
    /// Дата начала проведения занятий
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Дата завершения проведения занятий
    /// </summary>
    public DateOnly EndDate { get; set; }
}