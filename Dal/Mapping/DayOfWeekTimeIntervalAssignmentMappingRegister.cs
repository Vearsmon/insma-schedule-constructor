using Dal.Entities;
using Domain.Models;
using Domain.Models.Common;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class DayOfWeekTimeIntervalAssignmentMappingRegister
{
    [UserMapping(Default = true)]
    public static DayOfWeekTimeIntervalAssignment? MapEntityToModel(DbDayOfWeekTimeIntervalAssignment? entity)
    {
        return entity == null ? null : new DayOfWeekTimeIntervalAssignment
        {
            Id = entity.Id,
            LessonBatchInfoId = entity.LessonBatchInfoId,
            DayOfWeekTimeInterval = new DayOfWeekTimeInterval
            {
                DayOfWeek = entity.DayOfWeek,
                TimeInterval = new TimeInterval { TimeFrom = entity.TimeFrom, TimeTo = entity.TimeTo },
            },
        };
    }

    [UserMapping(Default = true)]
    public static void UpdateEntityWithModel(DayOfWeekTimeIntervalAssignment? model, DbDayOfWeekTimeIntervalAssignment? entity)
    {
        if (model == null || entity == null) return;
        entity.Id = model.Id.HasValue ? model.Id!.Value : entity.Id;
        entity.LessonBatchInfoId = model.LessonBatchInfoId;
        entity.DayOfWeek = model.DayOfWeekTimeInterval.DayOfWeek;
        entity.TimeFrom = model.DayOfWeekTimeInterval.TimeInterval.TimeFrom;
        entity.TimeTo = model.DayOfWeekTimeInterval.TimeInterval.TimeTo;
    }
}