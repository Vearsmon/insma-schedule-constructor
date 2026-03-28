namespace Domain.Dto.ViewDto;

public class TeacherViewDto
{
    public Guid Id { get; set; }
    public string Fullname { get; set; } = null!;
    public string Contacts { get; set; } = null!;
}