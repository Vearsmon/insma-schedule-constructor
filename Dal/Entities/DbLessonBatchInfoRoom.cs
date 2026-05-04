using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(LessonBatchInfoId), nameof(RoomId))]
public class DbLessonBatchInfoRoom : IDbEntity
{
    public Guid LessonBatchInfoId { get; set; }

    public DbLessonBatchInfo LessonBatchInfo { get; set; } = null!;

    public Guid RoomId { get; set; }

    public DbRoom Room { get; set; } = null!;
}