namespace Domain.Attributes;

public class ErrorCodeAttribute(string code) : Attribute
{
    public string Code { get; } = code;
}
