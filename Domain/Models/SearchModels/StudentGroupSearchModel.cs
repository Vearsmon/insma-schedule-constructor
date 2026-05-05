using Domain.Models.Enums;

namespace Domain.Models.SearchModels;

public class StudentGroupSearchModel
{
    public Guid? Id { get; set; }
    public Guid? ScheduleId { get; set; }
    public StudentGroupType[] StudentGroupTypes { get; set; } = [];
    public bool IncludeGroupsWithoutParents { get; set; }
}