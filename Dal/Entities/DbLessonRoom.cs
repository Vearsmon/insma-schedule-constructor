using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(LessonId), nameof(RoomId))]
public class DbLessonRoom : IDbEntity
{
    public Guid LessonId { get; set; }

    public DbLesson Lesson { get; set; } = null!;

    public Guid RoomId { get; set; }

    public DbRoom Room { get; set; } = null!;
}