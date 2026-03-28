namespace Domain.Models.SearchModels;

public class LessonValidationMessageSearchModel
{
    public Guid[] AffectedByLessonIds { get; set; } = [];
}