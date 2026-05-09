using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(LessonId), nameof(LessonPolicyViolationId))]
public class DbLessonPolicyViolationLink : IDbEntity
{
    public Guid LessonId { get; set; }

    public DbLesson Lesson { get; set; } = null!;

    public Guid LessonPolicyViolationId { get; set; }

    public DbLessonPolicyViolation LessonPolicyViolation { get; set; } = null!;
}