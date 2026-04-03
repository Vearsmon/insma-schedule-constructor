using Domain.Models.Enums;

namespace Domain.Models.SearchModels;

public class LessonValidationMessageSearchModel
{
    public Guid[] AffectedByLessonIds { get; set; } = [];
    public Guid[] LessonIds { get; set; } = [];
    public LessonValidationCode[] ValidationCodes { get; set; } = [];
}