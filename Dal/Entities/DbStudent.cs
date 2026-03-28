using System.ComponentModel.DataAnnotations;

namespace Dal.Entities;

/// <summary>
/// Студент
/// </summary>
public class DbStudent : IDbEntityWithId
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
    /// ФИО студента
    /// </summary>
    [MaxLength(255)]
    public string Fullname { get; set; } = null!;

    /// <summary>
    /// Академическая группа
    /// </summary>
    public Guid StudentGroupId { get; set; }

    /// <summary>
    /// Академическая группа
    /// </summary>
    public DbStudentGroup StudentGroup { get; set; } = null!;
}