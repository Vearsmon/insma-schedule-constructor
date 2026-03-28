namespace Domain.Models.Common;

public interface IWithVersion : IModelWithId
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
