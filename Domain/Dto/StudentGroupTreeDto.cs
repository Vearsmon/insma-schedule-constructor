namespace Domain.Dto;

public class StudentGroupTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public StudentGroupTreeDto[] Children { get; set; } = [];
}