using Microsoft.EntityFrameworkCore;

namespace Dal.Entities;

[PrimaryKey(nameof(ChildStudentGroupId), nameof(ParentStudentGroupId))]
public class DbStudentGroupLink
{
    public Guid ChildStudentGroupId { get; set; }

    public DbStudentGroup ChildStudentGroup { get; set; } = null!;

    public Guid ParentStudentGroupId { get; set; }

    public DbStudentGroup ParentStudentGroup { get; set; } = null!;
}