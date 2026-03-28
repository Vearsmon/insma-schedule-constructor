using Domain.Models.Enums;

namespace Domain.Dto.ViewDto;

public class StudentGroupViewDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int SemesterNumber { get; set; }
    public StudentGroupType StudentGroupType { get; set; }
    public string Cypher { get; set; } = null!;
    public StudentGroupShortViewDto[] Children { get; set; } = [];
}