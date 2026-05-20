using Dal.Entities;
using Domain.Models;
using Domain.Models.Common;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class LessonMappingRegister
{
    [UserMapping(Default = true)]
    public static Lesson? MapEntityToModel(DbLesson? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (entity == null) return model;
        model!.DateWithTimeInterval = entity.Date.HasValue ? new DateWithTimeInterval
        {
            Date = entity.Date!.Value,
            TimeInterval = new TimeInterval
            {
                TimeFrom = entity.TimeFrom!.Value,
                TimeTo = entity.TimeTo!.Value,
            },
        } : null;
        model.AcademicDiscipline = AcademicDisciplineMappingRegister.MapEntityToModel(entity.AcademicDiscipline);
        model.StudentGroups = entity.StudentGroups.Select(StudentGroupMappingRegister.MapEntityToModel).ToArray()!;
        model.Teachers = entity.Teachers.Select(TeacherMappingRegister.MapEntityToModel).ToArray()!;
        model.Rooms = entity.Rooms.Select(RoomMappingRegister.MapEntityToModel).ToArray()!;
        model.DayOfWeekTimeIntervalAssignment = DayOfWeekTimeIntervalAssignmentMappingRegister.MapEntityToModel(entity.DayOfWeekTimeIntervalAssignment)!;
        model.LessonBatchInfo = LessonBatchInfoMappingRegister.MapEntityToModel(entity.LessonBatchInfo);
        model.Violations = entity.Violations.Select(LessonPolicyViolationMappingRegister.MapEntityToModel).ToArray()!;
        return model;
    }

    [UserMapping(Default = true)]
    public static DbLesson? MapModelToEntity(Lesson? model)
    {
        var entity = AutoMapModelToEntity(model);
        if (model == null) return entity;
        entity!.Date = model.DateWithTimeInterval?.Date;
        entity.TimeFrom = model.DateWithTimeInterval?.TimeInterval.TimeFrom;
        entity.TimeTo = model.DateWithTimeInterval?.TimeInterval.TimeTo;
        return entity;
    }

    [UserMapping(Default = true)]
    public static void UpdateEntityWithModel(Lesson? model, DbLesson? entity)
    {
        AutoUpdateEntityWithModel(model, entity);
        if (entity == null) return;
        entity.Date = model!.DateWithTimeInterval?.Date;
        entity.TimeFrom = model.DateWithTimeInterval?.TimeInterval.TimeFrom;
        entity.TimeTo = model.DateWithTimeInterval?.TimeInterval.TimeTo;
    }

    [UserMapping(Default = true)]
    public static LessonRegistryItem? MapEntityToRegistryItem(DbLesson? entity)
    {
        var item = AutoMapEntityToRegistryItem(entity);
        if (entity == null) return item;
        item!.DateWithTimeInterval = entity.Date.HasValue ? new DateWithTimeInterval
        {
            Date = entity.Date!.Value,
            TimeInterval = new TimeInterval
            {
                TimeFrom = entity.TimeFrom!.Value,
                TimeTo = entity.TimeTo!.Value,
            },
        } : null;
        item.StudentGroupIds = entity.StudentGroups.Select(x => x.Id).ToArray();
        item.TeacherIds = entity.Teachers.Select(x => x.Id).ToArray();
        item.RoomIds = entity.Rooms.Select(x => x.Id).ToArray();
        item.Violations = entity.Violations.Select(LessonPolicyViolationMappingRegister.MapEntityToModel).ToArray()!;
        return item;
    }

    [MapperIgnoreSource(nameof(DbLesson.Schedule))]
    [MapperIgnoreSource(nameof(DbLesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(DbLesson.Date))]
    [MapperIgnoreSource(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreSource(nameof(DbLesson.TimeTo))]
    [MapperIgnoreSource(nameof(DbLesson.StudentGroups))]
    [MapperIgnoreSource(nameof(DbLesson.Teachers))]
    [MapperIgnoreSource(nameof(DbLesson.Rooms))]
    [MapperIgnoreSource(nameof(DbLesson.DayOfWeekTimeIntervalAssignment))]
    [MapperIgnoreSource(nameof(DbLesson.LessonBatchInfo))]
    [MapperIgnoreSource(nameof(DbLesson.Violations))]
    [MapperIgnoreTarget(nameof(Lesson.Schedule))]
    [MapperIgnoreTarget(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(Lesson.DateWithTimeInterval))]
    [MapperIgnoreTarget(nameof(Lesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(Lesson.Teachers))]
    [MapperIgnoreTarget(nameof(Lesson.Rooms))]
    [MapperIgnoreTarget(nameof(Lesson.DayOfWeekTimeIntervalAssignment))]
    [MapperIgnoreTarget(nameof(Lesson.LessonBatchInfo))]
    [MapperIgnoreTarget(nameof(Lesson.Violations))]
    private static partial Lesson? AutoMapEntityToModel(DbLesson? entity);

    [MapProperty(nameof(Lesson.Schedule), nameof(DbLesson.Schedule), Use = nameof(@ScheduleMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(Lesson.AcademicDiscipline), nameof(DbLesson.AcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapModelToEntity))]
    [MapperIgnoreSource(nameof(Lesson.DateWithTimeInterval))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
    [MapperIgnoreSource(nameof(Lesson.DayOfWeekTimeIntervalAssignment))]
    [MapperIgnoreSource(nameof(Lesson.LessonBatchInfo))]
    [MapperIgnoreSource(nameof(Lesson.Violations))]
    [MapperIgnoreTarget(nameof(DbLesson.Date))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeTo))]
    [MapperIgnoreTarget(nameof(DbLesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(DbLesson.Teachers))]
    [MapperIgnoreTarget(nameof(DbLesson.Rooms))]
    [MapperIgnoreTarget(nameof(DbLesson.DayOfWeekTimeIntervalAssignment))]
    [MapperIgnoreTarget(nameof(DbLesson.LessonBatchInfo))]
    [MapperIgnoreTarget(nameof(DbLesson.Violations))]
    private static partial DbLesson? AutoMapModelToEntity(Lesson? model);

    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(Lesson.DateWithTimeInterval))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
    [MapperIgnoreSource(nameof(Lesson.DayOfWeekTimeIntervalAssignment))]
    [MapperIgnoreSource(nameof(Lesson.LessonBatchInfo))]
    [MapperIgnoreSource(nameof(Lesson.Violations))]
    [MapperIgnoreTarget(nameof(DbLesson.Schedule))]
    [MapperIgnoreTarget(nameof(DbLesson.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(DbLesson.Date))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeTo))]
    [MapperIgnoreTarget(nameof(DbLesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(DbLesson.Teachers))]
    [MapperIgnoreTarget(nameof(DbLesson.Rooms))]
    [MapperIgnoreTarget(nameof(DbLesson.DayOfWeekTimeIntervalAssignment))]
    [MapperIgnoreTarget(nameof(DbLesson.LessonBatchInfo))]
    [MapperIgnoreTarget(nameof(DbLesson.Violations))]
    private static partial void AutoUpdateEntityWithModel(Lesson? model, DbLesson? entity);

    [MapperIgnoreSource(nameof(DbLesson.ScheduleId))]
    [MapperIgnoreSource(nameof(DbLesson.Schedule))]
    [MapperIgnoreSource(nameof(DbLesson.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(DbLesson.Date))]
    [MapperIgnoreSource(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreSource(nameof(DbLesson.TimeTo))]
    [MapperIgnoreSource(nameof(DbLesson.StudentGroups))]
    [MapperIgnoreSource(nameof(DbLesson.Teachers))]
    [MapperIgnoreSource(nameof(DbLesson.Rooms))]
    [MapperIgnoreSource(nameof(DbLesson.DayOfWeekTimeIntervalAssignmentId))]
    [MapperIgnoreSource(nameof(DbLesson.DayOfWeekTimeIntervalAssignment))]
    [MapperIgnoreSource(nameof(DbLesson.DetachedFromBatch))]
    [MapperIgnoreSource(nameof(DbLesson.LessonBatchInfoId))]
    [MapperIgnoreSource(nameof(DbLesson.LessonBatchInfo))]
    [MapperIgnoreSource(nameof(DbLesson.Violations))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.DateWithTimeInterval))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.StudentGroupIds))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.TeacherIds))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.RoomIds))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.Violations))]
    private static partial LessonRegistryItem? AutoMapEntityToRegistryItem(DbLesson? entity);
}