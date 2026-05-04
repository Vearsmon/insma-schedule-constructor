using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(LessonId), nameof(StudentGroupId))]
public class DbLessonStudentGroup : IDbEntity
{
    public Guid LessonId { get; set; }

    public DbLesson Lesson { get; set; } = null!;

    public Guid StudentGroupId { get; set; }

    public DbStudentGroup StudentGroup { get; set; } = null!;
}