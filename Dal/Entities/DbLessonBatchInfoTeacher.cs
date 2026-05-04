using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(LessonBatchInfoId), nameof(TeacherId))]
public class DbLessonBatchInfoTeacher : IDbEntity
{
    public Guid LessonBatchInfoId { get; set; }

    public DbLessonBatchInfo LessonBatchInfo { get; set; } = null!;

    public Guid TeacherId { get; set; }

    public DbTeacher Teacher { get; set; } = null!;
}