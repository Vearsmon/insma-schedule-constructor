namespace Dal.Entities;

public interface IDbSoftDeleteEntity : IDbEntityWithId
{
    /// <summary>
    /// Признак мягкого удаления записи.
    /// </summary>
    bool IsDeleted { get; set; }
}
