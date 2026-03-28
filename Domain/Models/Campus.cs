using Domain.Models.Common;

namespace Domain.Models;

/// <summary>
/// Учебный корпус
/// </summary>
public class Campus : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Название учебного корпуса
    /// </summary>
    public string Name { get; set; } = null!;
}