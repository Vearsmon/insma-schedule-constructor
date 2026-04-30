using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class SaveStudentGroupDto
{
    public Guid? Id { get; set; }
    public Guid ScheduleId { get; set; }
    public string Name { get; set; } = null!;
    public int? SemesterNumber { get; set; }
    public StudentGroupType StudentGroupType { get; set; }
    public Guid[] ParentIds { get; set; } = [];
    public Guid[] ChildIds { get; set; } = [];
    public string[] SemiGroupToCreateNames { get; set; } = [];
}