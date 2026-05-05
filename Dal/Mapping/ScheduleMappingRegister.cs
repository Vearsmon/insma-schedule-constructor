using Dal.Entities;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class ScheduleMappingRegister
{
    [UserMapping(Default = true)]
    public static Schedule? MapEntityToModel(DbSchedule? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (entity == null) return model;
        model!.DateInterval = new DateInterval { DateFrom = entity.DateFrom, DateTo = entity.DateTo };
        return model;
    }

    [UserMapping(Default = true)]
    public static ScheduleRegistryItem? MapEntityToRegistryItem(DbSchedule? entity)
    {
        var item = AutoMapEntityToRegistryItem(entity);
        if (entity == null) return item;
        item!.DateInterval = new DateInterval { DateFrom = entity.DateFrom, DateTo = entity.DateTo };
        return item;
    }

    [MapProperty($"{nameof(Schedule.DateInterval)}.{nameof(Schedule.DateInterval.DateFrom)}", nameof(DbSchedule.DateFrom))]
    [MapProperty($"{nameof(Schedule.DateInterval)}.{nameof(Schedule.DateInterval.DateTo)}", nameof(DbSchedule.DateTo))]
    public static partial DbSchedule? MapModelToEntity(Schedule? model);

    [MapProperty($"{nameof(Schedule.DateInterval)}.{nameof(Schedule.DateInterval.DateFrom)}", nameof(DbSchedule.DateFrom))]
    [MapProperty($"{nameof(Schedule.DateInterval)}.{nameof(Schedule.DateInterval.DateTo)}", nameof(DbSchedule.DateTo))]
    public static partial void UpdateEntityWithModel(Schedule? model, DbSchedule? entity);

    [MapperIgnoreSource(nameof(DbSchedule.DateFrom))]
    [MapperIgnoreSource(nameof(DbSchedule.DateTo))]
    [MapperIgnoreTarget(nameof(Schedule.DateInterval))]
    private static partial Schedule? AutoMapEntityToModel(DbSchedule? entity);

    [MapperIgnoreSource(nameof(DbSchedule.DateFrom))]
    [MapperIgnoreSource(nameof(DbSchedule.DateTo))]
    [MapperIgnoreTarget(nameof(Schedule.DateInterval))]
    private static partial ScheduleRegistryItem? AutoMapEntityToRegistryItem(DbSchedule? entity);
}