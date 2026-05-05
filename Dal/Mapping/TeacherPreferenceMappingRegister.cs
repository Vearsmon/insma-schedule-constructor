using Dal.Entities;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class TeacherPreferenceMappingRegister
{
    [UserMapping(Default = true)]
    public static TeacherPreference? MapEntityToModel(DbTeacherPreference? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (entity is not { DayOfWeek: not null, TimeFrom: not null, TimeTo: not null }) return model;
        model!.DayOfWeekTimeInterval ??= new DayOfWeekTimeInterval
        {
            DayOfWeek = entity.DayOfWeek.Value,
            TimeInterval = new TimeInterval
            {
                TimeFrom = entity.TimeFrom!.Value,
                TimeTo = entity.TimeTo!.Value
            },
        };
        return model;
    }

    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.DayOfWeek)}", nameof(DbTeacherPreference.DayOfWeek))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeFrom)}", nameof(DbTeacherPreference.TimeFrom))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeTo)}", nameof(DbTeacherPreference.TimeTo))]
    [MapProperty(nameof(TeacherPreference.Schedule), nameof(DbTeacherPreference.Schedule), Use = nameof(@ScheduleMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(TeacherPreference.Teacher), nameof(DbTeacherPreference.Teacher), Use = nameof(@TeacherMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(TeacherPreference.Room), nameof(DbTeacherPreference.Room), Use = nameof(@RoomMappingRegister.MapModelToEntity))]
    public static partial DbTeacherPreference? MapModelToEntity(TeacherPreference? model);

    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.DayOfWeek)}", nameof(DbTeacherPreference.DayOfWeek))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeFrom)}", nameof(DbTeacherPreference.TimeFrom))]
    [MapProperty($"{nameof(TeacherPreference.DayOfWeekTimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval)}.{nameof(TeacherPreference.DayOfWeekTimeInterval.TimeInterval.TimeTo)}", nameof(DbTeacherPreference.TimeTo))]
    [MapProperty(nameof(TeacherPreference.Schedule), nameof(DbTeacherPreference.Schedule), Use = nameof(@ScheduleMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(TeacherPreference.Teacher), nameof(DbTeacherPreference.Teacher), Use = nameof(@TeacherMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(TeacherPreference.Room), nameof(DbTeacherPreference.Room), Use = nameof(@RoomMappingRegister.MapModelToEntity))]
    public static partial void UpdateEntityWithModel(TeacherPreference? model, DbTeacherPreference? entity);

    [MapperIgnoreSource(nameof(DbTeacherPreference.ScheduleId))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Schedule))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TeacherId))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Teacher))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.RoomId))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Room))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.DayOfWeek))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeFrom))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeTo))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TeacherPreferenceType))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.Comment))]
    public static partial TeacherPreferenceRegistryItem? MapEntityToRegistryItem(DbTeacherPreference? entity);

    [MapperIgnoreSource(nameof(DbTeacherPreference.DayOfWeek))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeFrom))]
    [MapperIgnoreSource(nameof(DbTeacherPreference.TimeTo))]
    [MapperIgnoreTarget(nameof(TeacherPreference.DayOfWeekTimeInterval))]
    [MapProperty(nameof(DbTeacherPreference.Schedule), nameof(TeacherPreference.Schedule), Use = nameof(@ScheduleMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbTeacherPreference.Teacher), nameof(TeacherPreference.Teacher), Use = nameof(@TeacherMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbTeacherPreference.Room), nameof(TeacherPreference.Room), Use = nameof(@RoomMappingRegister.MapEntityToModel))]
    private static partial TeacherPreference? AutoMapEntityToModel(DbTeacherPreference? entity);
}