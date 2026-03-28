namespace Domain.Models.Common;

public class DateWithTimeInterval
{
    public DateOnly Date { get; set; }
    public TimeInterval TimeInterval { get; set; } = null!;
}