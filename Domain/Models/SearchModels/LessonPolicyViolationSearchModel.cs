using Domain.Models.Enums;

namespace Domain.Models.SearchModels;

public class LessonPolicyViolationSearchModel
{
    public Guid[] AffectedByLessonIds { get; set; } = [];
    public Guid[] AffectedByAcademicDisciplineIds { get; set; } = [];
    public Guid[] LessonIds { get; set; } = [];
    public LessonPolicyViolationCode[] ValidationCodes { get; set; } = [];
}