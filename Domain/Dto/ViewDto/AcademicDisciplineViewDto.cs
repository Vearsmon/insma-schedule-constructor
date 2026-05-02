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
    public AcademicDisciplinePayloadDto? LecturePayload { get; set; }
    public AcademicDisciplinePayloadDto? PracticePayload { get; set; }
    public AcademicDisciplinePayloadDto? LabPayload { get; set; }
    public AcademicDisciplinePayloadDto? ExamPayload { get; set; }
    public AcademicDisciplinePayloadDto? TestPayload { get; set; }
    public string? Comment { get; set; }
}