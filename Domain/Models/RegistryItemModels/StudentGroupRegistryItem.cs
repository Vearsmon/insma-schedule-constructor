using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models.RegistryItemModels;

public class StudentGroupRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public int SemesterNumber { get; set; }
    public StudentGroupType StudentGroupType { get; set; }
    public string Cypher { get; set; } = null!;
}