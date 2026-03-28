namespace Domain.Attributes;

/// <summary>
/// Атрибут которым помечаются поля моделей, по которым нужна сортировка
/// </summary>
public class SortFieldAttribute(string fieldName) : Attribute
{
    public string FieldName { get; } = fieldName;
}
