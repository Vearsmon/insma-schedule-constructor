namespace Domain.Dto.RegistryDto;

public class SearchParametersDto
{
    public int Page { get; set; }
    public int? ItemsPerPage { get; set; }
    public string? OrderBy { get; set; }
    public bool? OrderAsc { get; set; }
    public string? ThenBy { get; set; }
    public bool? ThenAsc { get; set; }
}
