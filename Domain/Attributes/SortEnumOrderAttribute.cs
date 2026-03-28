namespace Domain.Attributes;

/// <summary>
/// Аттрибут, которым значения перечислений, по которым нужна сортировка, отличная от лексикографической
/// </summary>
public class SortEnumOrderAttribute(int order) : Attribute
{
    public int Order { get; set; } = order;
}
