using Domain.Models.Enums;

namespace Domain.Dto.ViewDto;

public class AcademicDisciplineViewDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public string[] AssociatedNames { get; set; } = [];
    public int? SemesterNumber { get; set; }
    public AcademicDisciplineTargetType AcademicDisciplineTargetType { get; set; }
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];
    public LessonBatchInfoDto[] LectureLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoDto[] PracticeLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoDto[] LabLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoDto[] ExamLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoDto[] TestLessonBatchInfos { get; set; } = [];
    public string? Comment { get; set; }
}