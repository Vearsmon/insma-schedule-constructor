using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Dto.RegistryDto;

public class AcademicDisciplineRegistryItemDto : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public string Cypher { get; set; } = null!;
    public int SemesterNumber { get; set; }
    public AcademicDisciplineTargetType AcademicDisciplineTargetType { get; set; }
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];
    public AcademicDisciplinePayloadDto? LecturePayload { get; set; }
    public AcademicDisciplinePayloadDto? PracticePayload { get; set; }
    public AcademicDisciplinePayloadDto? LabPayload { get; set; }
    public string? Comment { get; set; }
}