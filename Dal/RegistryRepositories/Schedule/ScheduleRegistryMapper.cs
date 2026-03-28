using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;
using Dal.Repositories;
using Domain.Models.RegistryItemModels;

namespace Dal.RegistryRepositories.Schedule;

public class ScheduleRegistryMapper : IReadonlyRepositoryMapper<DbSchedule, ScheduleRegistryItem>
{
    [return: NotNullIfNotNull("entity")]
    public ScheduleRegistryItem? Map(DbSchedule? entity) => MappingRegister.MapRegistryItem(entity);
}