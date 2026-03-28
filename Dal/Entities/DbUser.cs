using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class DbUser : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [MaxLength(64)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Время создания
    /// </summary>
    [Column(TypeName = DbDataTypes.DateTime)]
    public DateTime CreatedAt { get; set; }
}