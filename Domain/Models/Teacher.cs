using Domain.Models.Common;

namespace Domain.Models;

/// <summary>
/// Преподаватель
/// </summary>
public class Teacher : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// ФИО преподавателя
    /// </summary>
    public string Fullname { get; set; } = null!;

    /// <summary>
    /// Контактные данные преподавателя
    /// </summary>
    public string Contacts { get; set; } = null!;
}