using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class AcademicDisciplineSaveDto
{
    public Guid? Id { get; set; }
    public Guid ScheduleId { get; set; }
    public string Name { get; set; } = null!;
    public string[] AssociatedNames { get; set; } = [];
    public int? SemesterNumber { get; set; }
    public AcademicDisciplineTargetType AcademicDisciplineTargetType { get; set; }
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];
    public LessonBatchInfoSaveDto[] LectureLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoSaveDto[] PracticeLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoSaveDto[] LabLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoSaveDto[] ExamLessonBatchInfos { get; set; } = [];
    public LessonBatchInfoSaveDto[] TestLessonBatchInfos { get; set; } = [];
    public string? Comment { get; set; }
}