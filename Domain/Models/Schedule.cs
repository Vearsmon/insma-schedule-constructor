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
}