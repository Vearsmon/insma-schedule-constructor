using System.ComponentModel.DataAnnotations;
using Domain.Models.Enums;

namespace Dal.Entities;

/// <summary>
/// Академическая группа
/// </summary>
public class DbStudentGroup : IDbEntityWithId
{
    public Guid Id { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public DbSchedule Schedule { get; set; } = null!;

    /// <summary>
    /// Название академической группы
    /// </summary>
    [MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Номер семестра
    /// </summary>
    public int? SemesterNumber { get; set; }

    /// <summary>
    /// Тип академической группы
    /// </summary>
    public StudentGroupType StudentGroupType { get; set; }

    /// <summary>
    /// Родительские академические группы
    /// </summary>
    public ICollection<DbStudentGroup> Parents { get; set; } = [];

    /// <summary>
    /// Составляющие академические группы
    /// </summary>
    public ICollection<DbStudentGroup> Children { get; set; } = [];
}