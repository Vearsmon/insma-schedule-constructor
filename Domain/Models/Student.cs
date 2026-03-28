using Domain.Models.Common;

namespace Domain.Models;

/// <summary>
/// Студент
/// </summary>
public class Student : IModelWithId
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
    /// ФИО студента
    /// </summary>
    public string Fullname { get; set; } = null!;

    /// <summary>
    /// Академическая группа
    /// </summary>
    public Guid StudentGroupId { get; set; }

    /// <summary>
    /// Академическая группа
    /// </summary>
    public StudentGroup StudentGroup { get; set; } = null!;
}