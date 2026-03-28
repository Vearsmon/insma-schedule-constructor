using System.ComponentModel.DataAnnotations;

namespace Dal.Entities;

/// <summary>
/// Преподаватель
/// </summary>
public class DbTeacher : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public DbUser User { get; set; } = null!;

    /// <summary>
    /// ФИО преподавателя
    /// </summary>
    [MaxLength(255)]
    public string Fullname { get; set; } = null!;

    /// <summary>
    /// Контактные данные преподавателя
    /// </summary>
    [MaxLength(64)]
    public string Contacts { get; set; } = null!;
}