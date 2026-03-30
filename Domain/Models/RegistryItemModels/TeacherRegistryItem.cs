using Domain.Models.Common;

namespace Domain.Models.RegistryItemModels;

public class TeacherRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
    public string Fullname { get; set; } = null!;
    public string? Contacts { get; set; }
}