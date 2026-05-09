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

        model.LecturePayload = entity.LectureTotalHoursCount != null || entity.AcademicDisciplineLectureLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            TotalHoursCount = entity.LectureTotalHoursCount,
            LessonBatchInfos = entity.AcademicDisciplineLectureLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        model.LabPayload = entity.LabTotalHoursCount != null || entity.AcademicDisciplineLabLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            TotalHoursCount = entity.LabTotalHoursCount,
            LessonBatchInfos = entity.AcademicDisciplineLabLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        model.PracticePayload = entity.PracticeTotalHoursCount != null || entity.AcademicDisciplinePracticeLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            TotalHoursCount = entity.PracticeTotalHoursCount,
            LessonBatchInfos = entity.AcademicDisciplinePracticeLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        model.ExamPayload = entity.AcademicDisciplineExamLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            LessonBatchInfos = entity.AcademicDisciplineExamLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        model.TestPayload = entity.AcademicDisciplineTestLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            LessonBatchInfos = entity.AcademicDisciplineTestLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;

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
        entity.LectureTotalHoursCount = model.LecturePayload?.TotalHoursCount;
        entity.AcademicDisciplineLectureLessonBatchInfos = model.LecturePayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.PracticeTotalHoursCount = model.PracticePayload?.TotalHoursCount;
        entity.AcademicDisciplinePracticeLessonBatchInfos = model.PracticePayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.LabTotalHoursCount = model.LabPayload?.TotalHoursCount;
        entity.AcademicDisciplineLabLessonBatchInfos = model.LabPayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineExamLessonBatchInfos = model.ExamPayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineTestLessonBatchInfos = model.TestPayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
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
        entity.Schedule = ScheduleMappingRegister.MapModelToEntity(model.Schedule)!;
        entity.LectureTotalHoursCount = model.LecturePayload?.TotalHoursCount;
        entity.AcademicDisciplineLectureLessonBatchInfos = model.LecturePayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.PracticeTotalHoursCount = model.PracticePayload?.TotalHoursCount;
        entity.AcademicDisciplinePracticeLessonBatchInfos = model.PracticePayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.LabTotalHoursCount = model.LabPayload?.TotalHoursCount;
        entity.AcademicDisciplineLabLessonBatchInfos = model.LabPayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineExamLessonBatchInfos = model.ExamPayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
        entity.AcademicDisciplineTestLessonBatchInfos = model.TestPayload?.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapModelToEntity).ToList()!;
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

        item.LecturePayload = entity.LectureTotalHoursCount != null || entity.AcademicDisciplineLectureLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            TotalHoursCount = entity.LectureTotalHoursCount,
            LessonBatchInfos = entity.AcademicDisciplineLectureLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        item.LabPayload = entity.LabTotalHoursCount != null || entity.AcademicDisciplineLabLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            TotalHoursCount = entity.LabTotalHoursCount,
            LessonBatchInfos = entity.AcademicDisciplineLabLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        item.PracticePayload = entity.PracticeTotalHoursCount != null || entity.AcademicDisciplinePracticeLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            TotalHoursCount = entity.PracticeTotalHoursCount,
            LessonBatchInfos = entity.AcademicDisciplinePracticeLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        item.ExamPayload = entity.AcademicDisciplineExamLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            LessonBatchInfos = entity.AcademicDisciplineExamLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;
        item.TestPayload = entity.AcademicDisciplineTestLessonBatchInfos.Count > 0 ? new AcademicDisciplinePayload
        {
            LessonBatchInfos = entity.AcademicDisciplineTestLessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!,
        } : null;

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
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.LectureTotalHoursCount))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.LabTotalHoursCount))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.PracticeTotalHoursCount))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.LecturePayload))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.LabPayload))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.PracticePayload))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.ExamPayload))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.TestPayload))]
    private static partial AcademicDiscipline? AutoMapEntityToModel(DbAcademicDiscipline? entity);

    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LecturePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LabPayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.PracticePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.ExamPayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.TestPayload))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsExamLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsTestLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.LectureTotalHoursCount))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.LabTotalHoursCount))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.PracticeTotalHoursCount))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos))]
    private static partial DbAcademicDiscipline? AutoMapModelToEntity(AcademicDiscipline? model);

    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.AllowedLessonTypes))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LecturePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LabPayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.PracticePayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.ExamPayload))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.TestPayload))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLectureLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsLabLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsPracticeLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsExamLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.IsTestLessonsAllowed))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.LectureTotalHoursCount))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.LabTotalHoursCount))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.PracticeTotalHoursCount))]
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
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.LectureTotalHoursCount))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLectureLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.LabTotalHoursCount))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineLabLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.PracticeTotalHoursCount))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplinePracticeLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineExamLessonBatchInfos))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.AcademicDisciplineTestLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.AllowedLessonTypes))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.LecturePayload))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.LabPayload))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.PracticePayload))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.ExamPayload))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.TestPayload))]
    private static partial AcademicDisciplineRegistryItem? AutoMapEntityToRegistryItem(DbAcademicDiscipline? entity);
}