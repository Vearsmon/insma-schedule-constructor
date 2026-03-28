using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class SaveStudentGroupDto
{
    public Guid? Id { get; set; }
    public Guid ScheduleId { get; set; }
    public string Name { get; set; } = null!;
    public int SemesterNumber { get; set; }
    public StudentGroupType StudentGroupType { get; set; }
    public string Cypher { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Guid[] ChildIds { get; set; } = [];
}