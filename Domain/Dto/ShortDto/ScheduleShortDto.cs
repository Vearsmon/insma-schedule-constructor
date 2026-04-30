using Domain.Models.Common;

namespace Domain.Dto.ShortDto;

public class ScheduleShortDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateInterval DateInterval { get; set; } = null!;
}