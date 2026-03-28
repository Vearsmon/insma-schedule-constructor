namespace Domain.Dto;

public class AcademicDisciplinePayloadDto
{
    public int TotalHoursCount { get; set; }
    public int StudyWeeksCount { get; set; }
    public int LessonsPerWeekCount { get; set; }
    public AcademicDisciplineLessonBatchInfoDto? LessonBatchInfo { get; set; }
}