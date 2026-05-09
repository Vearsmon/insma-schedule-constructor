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
        model!.DateWithTimeInterval ??= new DateWithTimeInterval
        {
            Date = entity.Date!.Value,
            TimeInterval = new TimeInterval
            {
                TimeFrom = entity.TimeFrom!.Value,
                TimeTo = entity.TimeTo!.Value,
            },
        };
        model.StudentGroups = entity.StudentGroups.Select(x => StudentGroupMappingRegister.MapEntityToModel(x.StudentGroup)).ToArray()!;
        model.Teachers = entity.Teachers.Select(x => TeacherMappingRegister.MapEntityToModel(x.Teacher)).ToArray()!;
        model.Rooms = entity.Rooms.Select(x => RoomMappingRegister.MapEntityToModel(x.Room)).ToArray()!;
        model.Violations = entity.Violations.Select(x => LessonPolicyViolationMappingRegister.MapEntityToModel(x.LessonPolicyViolation)).ToArray()!;
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
        entity.StudentGroups = model.StudentGroups.Select(x => new DbLessonStudentGroup { LessonId = model.Id ?? Guid.Empty, StudentGroupId = x.Id ?? Guid.Empty }).ToList();
        entity.Teachers = model.Teachers.Select(x => new DbLessonTeacher { LessonId = model.Id ?? Guid.Empty, TeacherId = x.Id ?? Guid.Empty }).ToList();
        entity.Rooms = model.Rooms.Select(x => new DbLessonRoom { LessonId = model.Id ?? Guid.Empty, RoomId = x.Id ?? Guid.Empty }).ToList();
        entity.Violations = model.Violations.Select(x => new DbLessonPolicyViolationLink { LessonId = model.Id ?? Guid.Empty, LessonPolicyViolationId = x.Id ?? Guid.Empty }).ToList();
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
        entity.StudentGroups = model.StudentGroups.Select(x => new DbLessonStudentGroup { LessonId = model.Id ?? Guid.Empty, StudentGroupId = x.Id ?? Guid.Empty }).ToList();
        entity.Teachers = model.Teachers.Select(x => new DbLessonTeacher { LessonId = model.Id ?? Guid.Empty, TeacherId = x.Id ?? Guid.Empty }).ToList();
        entity.Rooms = model.Rooms.Select(x => new DbLessonRoom { LessonId = model.Id ?? Guid.Empty, RoomId = x.Id ?? Guid.Empty }).ToList();
        entity.Violations = model.Violations.Select(x => new DbLessonPolicyViolationLink
        {
            LessonId = model.Id ?? Guid.Empty,
            LessonPolicyViolationId = x.Id ?? Guid.Empty,
            LessonPolicyViolation = LessonPolicyViolationMappingRegister.MapModelToEntity(x)!,
        }).ToList();
    }

    [UserMapping(Default = true)]
    public static LessonRegistryItem? MapEntityToRegistryItem(DbLesson? entity)
    {
        var item = AutoMapEntityToRegistryItem(entity);
        if (entity == null) return item;
        item!.DateWithTimeInterval ??= new DateWithTimeInterval
        {
            Date = entity.Date!.Value,
            TimeInterval = new TimeInterval
            {
                TimeFrom = entity.TimeFrom!.Value,
                TimeTo = entity.TimeTo!.Value,
            },
        };
        item.StudentGroupIds = entity.StudentGroups.Select(x => x.StudentGroupId).ToArray();
        item.TeacherIds = entity.Teachers.Select(x => x.TeacherId).ToArray();
        item.RoomIds = entity.Rooms.Select(x => x.RoomId).ToArray();
        item.Violations = entity.Violations.Select(x => LessonPolicyViolationMappingRegister.MapEntityToModel(x.LessonPolicyViolation)).ToArray()!;
        return item;
    }

    // [MapProperty(nameof(DbLesson.Schedule), nameof(Lesson.Schedule), Use = nameof(@ScheduleMappingRegister.MapEntityToModel))]
    [MapProperty(nameof(DbLesson.AcademicDiscipline), nameof(Lesson.AcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapEntityToModel))]
    [MapperIgnoreSource(nameof(DbLesson.Schedule))]
    [MapperIgnoreSource(nameof(DbLesson.Date))]
    [MapperIgnoreSource(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreSource(nameof(DbLesson.TimeTo))]
    [MapperIgnoreSource(nameof(DbLesson.StudentGroups))]
    [MapperIgnoreSource(nameof(DbLesson.Teachers))]
    [MapperIgnoreSource(nameof(DbLesson.Rooms))]
    [MapperIgnoreSource(nameof(DbLesson.Violations))]
    [MapperIgnoreTarget(nameof(Lesson.Schedule))]
    [MapperIgnoreTarget(nameof(Lesson.DateWithTimeInterval))]
    [MapperIgnoreTarget(nameof(Lesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(Lesson.Teachers))]
    [MapperIgnoreTarget(nameof(Lesson.Rooms))]
    [MapperIgnoreTarget(nameof(Lesson.Violations))]
    private static partial Lesson? AutoMapEntityToModel(DbLesson? entity);

    [MapProperty(nameof(Lesson.Schedule), nameof(DbLesson.Schedule), Use = nameof(@ScheduleMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(Lesson.AcademicDiscipline), nameof(DbLesson.AcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapModelToEntity))]
    [MapperIgnoreSource(nameof(Lesson.DateWithTimeInterval))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
    [MapperIgnoreSource(nameof(Lesson.Violations))]
    [MapperIgnoreTarget(nameof(DbLesson.Date))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeTo))]
    [MapperIgnoreTarget(nameof(DbLesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(DbLesson.Teachers))]
    [MapperIgnoreTarget(nameof(DbLesson.Rooms))]
    [MapperIgnoreTarget(nameof(DbLesson.Violations))]
    private static partial DbLesson? AutoMapModelToEntity(Lesson? model);

    // [MapProperty(nameof(Lesson.Schedule), nameof(DbLesson.Schedule), Use = nameof(@ScheduleMappingRegister.MapModelToEntity))]
    [MapProperty(nameof(Lesson.AcademicDiscipline), nameof(DbLesson.AcademicDiscipline), Use = nameof(@AcademicDisciplineMappingRegister.MapModelToEntity))]
    [MapperIgnoreSource(nameof(Lesson.Schedule))]
    [MapperIgnoreSource(nameof(Lesson.DateWithTimeInterval))]
    [MapperIgnoreSource(nameof(Lesson.StudentGroups))]
    [MapperIgnoreSource(nameof(Lesson.Teachers))]
    [MapperIgnoreSource(nameof(Lesson.Rooms))]
    [MapperIgnoreSource(nameof(Lesson.Violations))]
    [MapperIgnoreTarget(nameof(DbLesson.Schedule))]
    [MapperIgnoreTarget(nameof(DbLesson.Date))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeFrom))]
    [MapperIgnoreTarget(nameof(DbLesson.TimeTo))]
    [MapperIgnoreTarget(nameof(DbLesson.StudentGroups))]
    [MapperIgnoreTarget(nameof(DbLesson.Teachers))]
    [MapperIgnoreTarget(nameof(DbLesson.Rooms))]
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
    [MapperIgnoreSource(nameof(DbLesson.Violations))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.DateWithTimeInterval))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.StudentGroupIds))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.TeacherIds))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.RoomIds))]
    [MapperIgnoreTarget(nameof(LessonRegistryItem.Violations))]
    private static partial LessonRegistryItem? AutoMapEntityToRegistryItem(DbLesson? entity);
}