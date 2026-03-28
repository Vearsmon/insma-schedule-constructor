namespace Domain.Dto.RegistryDto;

public class RegistryDto<TModel>
{
    /// <summary>
    /// Набор элементов реестра
    /// </summary>
    public TModel[] Items { get; set; } = [];

    /// <summary>
    /// Размер реестра
    /// </summary>
    public int ItemsCount { get; set; }
}
