using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(LessonBatchInfoId), nameof(StudentGroupId))]
public class DbLessonBatchInfoStudentGroup : IDbEntity
{
    public Guid LessonBatchInfoId { get; set; }

    public DbLessonBatchInfo LessonBatchInfo { get; set; } = null!;

    public Guid StudentGroupId { get; set; }

    public DbStudentGroup StudentGroup { get; set; } = null!;
}