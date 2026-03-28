using Domain.Models.Common;
using Domain.Models.Enums;

namespace Domain.Models.RegistryItemModels;

public class AcademicDisciplineRegistryItem : IModelWithId
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = null!;
    public string Cypher { get; set; } = null!;
    public int SemesterNumber { get; set; }
    public AcademicDisciplineTargetType AcademicDisciplineTargetType { get; set; }
    public AcademicDisciplineType[] AllowedLessonTypes { get; set; } = [];
    public AcademicDisciplinePayload? LecturePayload { get; set; }
    public AcademicDisciplinePayload? PracticePayload { get; set; }
    public AcademicDisciplinePayload? LabPayload { get; set; }
    public string? Comment { get; set; }
}