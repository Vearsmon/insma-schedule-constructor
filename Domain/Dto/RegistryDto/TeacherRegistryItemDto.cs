using Domain.Models.Common;

namespace Domain.Dto.RegistryDto;

public class TeacherRegistryItemDto : IModelWithId
{
    public Guid? Id { get; set; }
    public string Fullname { get; set; } = null!;
    public string Contacts { get; set; } = null!;
}