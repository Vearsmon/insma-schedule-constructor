using Dal.Entities;
using Domain.Models;
using Domain.Models.Common;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class LessonBatchInfoMappingRegister
{
    [UserMapping(Default = true)]
    public static LessonBatchInfo? MapEntityToModel(DbLessonBatchInfo? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (entity == null) return model;
        model!.DateInterval = new DateInterval { DateFrom = entity.DateFrom, DateTo = entity.DateTo };
        model.StudentGroups = entity.StudentGroups.Select(StudentGroupMappingRegister.MapEntityToModel).ToArray()!;
        model.Teachers = entity.Teachers.Select(TeacherMappingRegister.MapEntityToModel).ToArray()!;
        model.Rooms = entity.Rooms.Select(RoomMappingRegister.MapEntityToModel).ToArray()!;
        return model;
    }

    [UserMapping(Default = true)]
    public static DbLessonBatchInfo? MapModelToEntity(LessonBatchInfo? model)
    {
        var entity = AutoMapModelToEntity(model);
        if (model == null) return entity;
        entity!.DateFrom = model.DateInterval.DateFrom;
        entity.DateTo = model.DateInterval.DateTo;
        return entity;
    }

    [UserMapping(Default = true)]
    public static void UpdateEntityWithModel(LessonBatchInfo? model, DbLessonBatchInfo? entity)
    {
        AutoUpdateEntityWithModel(model, entity);
        if (model == null) return;
        entity!.DateFrom = model.DateInterval.DateFrom;
        entity.DateTo = model.DateInterval.DateTo;
    }

    [MapperIgnoreSource(nameof(DbLessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(DbLessonBatchInfo.DateFrom))]
    [MapperIgnoreSource(nameof(DbLessonBatchInfo.DateTo))]
    [MapperIgnoreSource(nameof(DbLessonBatchInfo.StudentGroups))]
    [MapperIgnoreSource(nameof(DbLessonBatchInfo.Teachers))]
    [MapperIgnoreSource(nameof(DbLessonBatchInfo.Rooms))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.DateInterval))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.StudentGroups))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.Teachers))]
    [MapperIgnoreTarget(nameof(LessonBatchInfo.Rooms))]
    private static partial LessonBatchInfo? AutoMapEntityToModel(DbLessonBatchInfo? entity);

    [MapperIgnoreSource(nameof(LessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.DateInterval))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.StudentGroups))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Teachers))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Rooms))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.DateFrom))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.DateTo))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.StudentGroups))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.Teachers))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.Rooms))]
    private static partial DbLessonBatchInfo? AutoMapModelToEntity(LessonBatchInfo? model);

    [MapperIgnoreSource(nameof(LessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.DateInterval))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.StudentGroups))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Teachers))]
    [MapperIgnoreSource(nameof(LessonBatchInfo.Rooms))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.AcademicDiscipline))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.DateFrom))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.DateTo))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.StudentGroups))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.Teachers))]
    [MapperIgnoreTarget(nameof(DbLessonBatchInfo.Rooms))]
    private static partial void AutoUpdateEntityWithModel(LessonBatchInfo? model, DbLessonBatchInfo? entity);
}