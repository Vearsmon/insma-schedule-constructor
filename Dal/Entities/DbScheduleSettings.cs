namespace Dal.Entities;

public class DbScheduleSettings : IDbEntityWithId
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public DbSchedule Schedule { get; set; } = null!;
    public bool IgnoreTeacherPreferences { get; set; }
    public bool IgnoreSequentialLectures { get; set; }
    public bool IgnoreSequentialSeminars { get; set; }
    public int StudentTimeWindowMinutes { get; set; }
    public int TeacherTimeWindowMinutes { get; set; }
    public int StudentMaxLessonsCount { get; set; }
    public int TeacherMaxLessonsCount { get; set; }
    public bool IgnoreMultipleStudentGroupLessons { get; set; }
    public bool IgnoreMultipleTeacherLessons { get; set; }
    // ...
}