namespace Domain.Dto;

public class AcademicDisciplinePayloadDto
{
    public int TotalHoursCount { get; set; }
    public AcademicDisciplineLessonBatchInfoDto? LessonBatchInfo { get; set; }
}