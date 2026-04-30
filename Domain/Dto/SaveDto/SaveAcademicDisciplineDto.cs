using Domain.Models.Enums;

namespace Domain.Dto.SaveDto;

public class SaveAcademicDisciplineDto
{
    public Guid? Id { get; set; }
    public Guid ScheduleId { get; set; }
    public string Name { get; set; } = null!;
    public string[] AssociatedNames { get; set; } = [];
    public int SemesterNumber { get; set; }
    public AcademicDisciplineTargetType AcademicDisciplineTargetType { get; set; }
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];
    public AcademicDisciplinePayloadDto? LecturePayload { get; set; } = null!;
    public AcademicDisciplinePayloadDto? PracticePayload { get; set; } = null!;
    public AcademicDisciplinePayloadDto? LabPayload { get; set; } = null!;
    public string? Comment { get; set; }
}