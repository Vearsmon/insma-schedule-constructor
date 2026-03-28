using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Пользователь
/// </summary>
public class User : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Время создания
    /// </summary>
    public DateTime CreatedAt { get; set; }
}