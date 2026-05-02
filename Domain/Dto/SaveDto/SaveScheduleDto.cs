using Domain.Models.Common;

namespace Domain.Dto.SaveDto;

public class SaveScheduleDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public DateInterval DateInterval { get; set; } = null!;
}