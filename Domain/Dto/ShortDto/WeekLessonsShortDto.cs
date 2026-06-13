namespace Domain.Dto.ShortDto;

public class WeekLessonsShortDto
{
    public LessonBatchInfoShortDto[] LessonBatches { get; set; } = [];
    public LessonShortDto[] Lessons { get; set; } = [];
}