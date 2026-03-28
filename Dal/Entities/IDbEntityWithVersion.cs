namespace Dal.Entities;

public interface IDbEntityWithVersion : IDbEntityWithId
{
    /// <summary>
    /// Признак актуальности версиии
    /// </summary>
    public bool IsActualVersion { get; set; }

    /// <summary>
    /// Id версии
    /// </summary>
    public Guid RootId { get; set; }
}
