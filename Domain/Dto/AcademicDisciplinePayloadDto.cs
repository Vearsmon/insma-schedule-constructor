namespace Domain.Dto;

public class AcademicDisciplinePayloadDto
{
    public int TotalHoursCount { get; set; }
    public LessonBatchInfoDto[] LessonBatchInfos { get; set; } = [];
}