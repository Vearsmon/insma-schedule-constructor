namespace Domain.Dto.SaveDto;

public class SaveTeacherDto
{
    public Guid? Id { get; set; }
    public string Fullname { get; set; } = null!;
    public string? Contacts { get; set; }
}