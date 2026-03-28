using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models;

/// <summary>
/// Академическая группа
/// </summary>
public class StudentGroup : IModelWithId
{
    public Guid? Id { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Проект расписания
    /// </summary>
    public Schedule Schedule { get; set; } = null!;

    /// <summary>
    /// Название академической группы
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Номер семестра
    /// </summary>
    public int SemesterNumber { get; set; }

    /// <summary>
    /// Тип академической группы
    /// </summary>
    public StudentGroupType StudentGroupType { get; set; }

    /// <summary>
    /// Шифр направления
    /// </summary>
    public string Cypher { get; set; } = null!;

    /// <summary>
    /// Академическая группа-предок
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Академическая группа-предок
    /// </summary>
    public StudentGroup? Parent { get; set; }

    /// <summary>
    /// Академические группы-наследники
    /// </summary>
    public StudentGroup[] Children { get; set; } = [];

    private StudentGroup[]? _childrenFlat;

    public StudentGroup[] ChildrenFlat
    {
        get
        {
            _childrenFlat ??= GetChildrenFlat().ToArray();
            return _childrenFlat;
        }
    }

    private IEnumerable<StudentGroup> GetChildrenFlat()
    {
        if (Children.Length > 0)
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var result in child.GetChildrenFlat())
                {
                    yield return result;
                }
            }
        }
    }
}