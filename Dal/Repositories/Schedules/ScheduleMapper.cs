using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Domain.Models;

namespace Dal.Repositories.Schedules;

public class ScheduleMapper : IRepositoryMapper<DbSchedule, Schedule>
{
    [return: NotNullIfNotNull("entity")]
    public Schedule? Map(DbSchedule? entity) => ScheduleMappingRegister.MapEntityToModel(entity);

    public void Update(DbSchedule entity, Schedule model)
    {
        ScheduleMappingRegister.UpdateEntityWithModel(model, entity);
    }
}