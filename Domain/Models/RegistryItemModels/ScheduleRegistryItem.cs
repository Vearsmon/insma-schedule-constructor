using Domain.Models.Common;

namespace Domain.Models.RegistryItemModels;

public class ScheduleRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public bool StartsWithEvenWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}