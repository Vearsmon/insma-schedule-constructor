using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models.RegistryItemModels;

public class AcademicDisciplineRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public string[] AssociatedNames { get; set; } = [];
    public int? SemesterNumber { get; set; }
    public AcademicDisciplineTargetType AcademicDisciplineTargetType { get; set; }
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];
    public LessonBatchInfo[] LectureLessonBatchInfos { get; set; } = [];
    public LessonBatchInfo[] PracticeLessonBatchInfos { get; set; } = [];
    public LessonBatchInfo[] LabLessonBatchInfos { get; set; } = [];
    public LessonBatchInfo[] ExamLessonBatchInfos { get; set; } = [];
    public LessonBatchInfo[] TestLessonBatchInfos { get; set; } = [];
    public string? Comment { get; set; }
}