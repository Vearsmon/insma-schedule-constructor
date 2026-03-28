namespace Domain.Models.Common;

public interface ISoftDeleteModel : IModelWithId
{
    bool IsDeleted { get; set; }
}
