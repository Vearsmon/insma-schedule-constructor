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
        model.LessonBatchInfos = entity.LessonBatchInfos.Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        return model;
    }

    [UserMapping(Default = true)]
    public static AcademicDisciplineRegistryItem? MapEntityToRegistryItem(DbAcademicDiscipline? entity)
    {
        var item = AutoMapEntityToRegistryItem(entity);
        if (item == null || entity == null) return null;
        item.LectureLessonBatchInfos = entity.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Lecture).Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.LabLessonBatchInfos = entity.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Lab).Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.PracticeLessonBatchInfos = entity.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Practice).Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.ExamLessonBatchInfos = entity.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Exam).Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        item.TestLessonBatchInfos = entity.LessonBatchInfos.Where(x => x.Type == AcademicDisciplineType.Test).Select(LessonBatchInfoMappingRegister.MapEntityToModel).ToArray()!;
        return item;
    }

    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.LessonBatchInfos))]
    public static partial DbAcademicDiscipline? MapModelToEntity(AcademicDiscipline? model);

    [MapperIgnoreSource(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(AcademicDiscipline.LessonBatchInfos))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(DbAcademicDiscipline.LessonBatchInfos))]
    public static partial void UpdateEntityWithModel(AcademicDiscipline? model, DbAcademicDiscipline? entity);

    [MapperIgnoreSource(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.LessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.Schedule))]
    [MapperIgnoreTarget(nameof(AcademicDiscipline.LessonBatchInfos))]
    public static partial AcademicDiscipline? AutoMapEntityToModel(DbAcademicDiscipline? entity);

    [MapperIgnoreSource(nameof(DbAcademicDiscipline.ScheduleId))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.Schedule))]
    [MapperIgnoreSource(nameof(DbAcademicDiscipline.LessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.LectureLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.LabLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.PracticeLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.ExamLessonBatchInfos))]
    [MapperIgnoreTarget(nameof(AcademicDisciplineRegistryItem.TestLessonBatchInfos))]
    private static partial AcademicDisciplineRegistryItem? AutoMapEntityToRegistryItem(DbAcademicDiscipline? entity);
}