using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(LessonId), nameof(TeacherId))]
public class DbLessonTeacher : IDbEntity
{
    public Guid LessonId { get; set; }

    public DbLesson Lesson { get; set; } = null!;

    public Guid TeacherId { get; set; }

    public DbTeacher Teacher { get; set; } = null!;
}