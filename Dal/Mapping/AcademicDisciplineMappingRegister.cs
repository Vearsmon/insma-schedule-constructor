using Dal.Entities;
using Domain.Models;
using Domain.Models.Enums;
using Domain.Models.RegistryItemModels;
using Riok.Mapperly.Abstractions;

namespace Dal.Mapping;

[Mapper]
public static partial class AcademicDisciplineMappingRegister
{
    [UserMapping(Default = true)]
    public static AcademicDiscipline? MapEntityToModel(DbAcademicDiscipline? entity)
    {
        var model = AutoMapEntityToModel(entity);
        if (model == null || entity == null) return null;
        model.Schedule = ScheduleMappingRegister.MapEntityToModel(entity.Schedule)!;

        model.LectureLessonBatchInfos = entity.AcademicDisciplineLectureLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        model.LabLessonBatchInfos = entity.AcademicDisciplineLabLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        model.PracticeLessonBatchInfos = entity.AcademicDisciplinePracticeLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        model.ExamLessonBatchInfos = entity.AcademicDisciplineExamLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        model.TestLessonBatchInfos = entity.AcademicDisciplineTestLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;

        var allowedLessonTypes = new List<AcademicDisciplineType>();
        if (entity.IsLectureLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lecture);
        if (entity.IsLabLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lab);
        if (entity.IsPracticeLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Practice);
        if (entity.IsExamLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Exam);
        if (entity.IsTestLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Test);
        model.AllowedLessonTypes = allowedLessonTypes.ToArray();

        return model;
    }

    [UserMapping(Default = true)]
    public static DbAcademicDiscipline? MapModelToEntity(AcademicDiscipline? model)
    {
        var entity = AutoMapModelToEntity(model);
        if (model == null || entity == null) return null;
        entity.Schedule = ScheduleMappingRegister.MapModelToEntity(model.Schedule)!;
        entity.AcademicDisciplineLectureLessonBatchInfos = model.LectureLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplinePracticeLessonBatchInfos = model.PracticeLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineLabLessonBatchInfos = model.LabLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineExamLessonBatchInfos = model.ExamLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineTestLessonBatchInfos = model.TestLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.IsLectureLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lecture);
        entity.IsLabLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lab);
        entity.IsPracticeLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Practice);
        entity.IsExamLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Exam);
        entity.IsTestLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Test);
        return entity;
    }

    [UserMapping(Default = true)]
    public static void UpdateEntityWithModel(AcademicDiscipline? model, DbAcademicDiscipline? entity)
    {
        AutoUpdateEntityWithModel(model, entity);
        if (model == null || entity == null) return;
        entity.AcademicDisciplineLectureLessonBatchInfos = model.LectureLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplinePracticeLessonBatchInfos = model.PracticeLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineLabLessonBatchInfos = model.LabLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineExamLessonBatchInfos = model.ExamLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineTestLessonBatchInfos = model.TestLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.IsLectureLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lecture);
        entity.IsLabLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Lab);
        entity.IsPracticeLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Practice);
        entity.IsExamLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Exam);
        entity.IsTestLessonsAllowed = model.AllowedLessonTypes.Contains(AcademicDisciplineType.Test);
    }

    [UserMapping(Default = true)]
    public static AcademicDisciplineRegistryItem? MapEntityToRegistryItem(DbAcademicDiscipline? entity)
    {
        var item = AutoMapEntityToRegistryItem(entity);
        if (item == null || entity == null) return null;

        item.LectureLessonBatchInfos = entity.AcademicDisciplineLectureLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.LabLessonBatchInfos = entity.AcademicDisciplineLabLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.PracticeLessonBatchInfos = entity.AcademicDisciplinePracticeLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.ExamLessonBatchInfos = entity.AcademicDisciplineExamLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.TestLessonBatchInfos = entity.AcademicDisciplineTestLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;

        var allowedLessonTypes = new List<AcademicDisciplineType>();
        if (entity.IsLectureLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lecture);
        if (entity.IsLabLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Lab);
        if (entity.IsPracticeLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Practice);
        if (entity.IsExamLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Exam);
        if (entity.IsTestLessonsAllowed) allowedLessonTypes.Add(AcademicDisciplineType.Test);
        item.AllowedLessonTypes = allowedLessonTypes.ToArray();
        return item;
    }

    [MapperIgnoreSource(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsExamLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsTestLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.LectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.LabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.PracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.ExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.TestLessonBatchInfos))]
    private static partial AcademicDiscipline? AutoMapEntityToModel(DbAcademicDiscipline? entity);

    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.PracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.ExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.TestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsExamLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsTestLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos))]
    private static partial DbAcademicDiscipline? AutoMapModelToEntity(AcademicDiscipline? model);

    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.PracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.ExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.TestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsExamLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsTestLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos))]
    private static partial void AutoUpdateEntityWithModel(AcademicDiscipline? model, DbAcademicDiscipline? entity);

    [MapperIgnoreSource(nameof(DbAcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsExamLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.IsTestLessonsAllowed))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.AllowedLessonTypes))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.LectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.LabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.PracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.ExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.TestLessonBatchInfos))]
    private static partial AcademicDisciplineRegistryItem? AutoMapEntityToRegistryItem(DbAcademicDiscipline? entity);
}