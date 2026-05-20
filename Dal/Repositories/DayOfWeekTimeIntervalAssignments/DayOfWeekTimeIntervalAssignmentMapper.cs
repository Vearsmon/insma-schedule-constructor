using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.DayOfWeekTimeIntervalAssignments;

public class DayOfWeekTimeIntervalAssignmentMapper : IRepositoryMapper<DbDayOfWeekTimeIntervalAssignment, DayOfWeekTimeIntervalAssignment>
{
    [return: NotNullIfNotNull("entity")]
    public DayOfWeekTimeIntervalAssignment? Map(DbDayOfWeekTimeIntervalAssignment? entity) =>
        DayOfWeekTimeIntervalAssignmentMappingRegister.MapEntityToModel(entity);

    public void Update(DbDayOfWeekTimeIntervalAssignment entity, DayOfWeekTimeIntervalAssignment model)
    {
        DayOfWeekTimeIntervalAssignmentMappingRegister.UpdateEntityWithModel(model, entity);
    }
}