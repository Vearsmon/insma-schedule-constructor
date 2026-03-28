using System.Diagnostics.CodeAnalysis;
using Dal.Entities;
using Dal.Mapping;

namespace Dal.Repositories.ScheduleSettings;

public class ScheduleSettingsMapper : IRepositoryMapper<DbScheduleSettings, Domain.Models.ScheduleSettings>
{
    [return: NotNullIfNotNull("entity")]
    public Domain.Models.ScheduleSettings? Map(DbScheduleSettings? entity)
    {
        return MappingRegister.Map(entity);
    }

    public void Update(DbScheduleSettings entity, Domain.Models.ScheduleSettings model)
    {
        MappingRegister.Update(model, entity);
    }
}