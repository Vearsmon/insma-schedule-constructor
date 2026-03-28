using System.ComponentModel;
using Domain.Attributes;

namespace Domain.Helpers;

public static class EnumExtensions
{
    public static T? GetAttribute<T>(this Enum value) where T : Attribute
    {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
        return attributes.Length > 0
            ? (T)attributes[0]
            : null;
    }

    public static string GetDescription(this Enum value)
    {
        var attribute = value.GetAttribute<DescriptionAttribute>();
        return attribute == null ? value.ToString() : attribute.Description;
    }

    public static string? GetSortOrder(this Enum value)
    {
        var attribute = value.GetAttribute<SortEnumOrderAttribute>();
        return attribute?.Order.ToString("000");
    }

    public static string GetErrorCode(this Enum value)
    {
        var attribute = value.GetAttribute<ErrorCodeAttribute>();
        return attribute == null ? value.ToString() : attribute.Code;
    }

    public static T GetValueFromDescription<T>(string description) where T : Enum
    {
        var items = Values<T>().Select(x => (x, x.GetDescription())).ToArray();
        return items.FirstOrDefault(tuple => tuple.Item2 == description).x;
    }

    public static T GetValueFromDescription<T>(string? description, Func<string?, string?> normalizeFunc) where T : Enum
    {
        var items = Values<T>().Select(x => (x, normalizeFunc.Invoke(x.GetDescription()))).ToArray();
        return items.FirstOrDefault(tuple => tuple.Item2 == normalizeFunc.Invoke(description)).x;
    }

    public static bool HasAll<TEnum>(this TEnum[] value, params TEnum[] variants) where TEnum : struct
    {
        return variants.All(value.Contains);
    }

    public static bool IsValidEnumDescription<TEnum>(this string? input, Func<string, string>? normalizeFunc = null)
        where TEnum : struct, Enum
    {
        normalizeFunc ??= s => s;
        return !string.IsNullOrWhiteSpace(input) &&
            Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Any(e =>
                    normalizeFunc.Invoke(e.GetDescription()) == normalizeFunc.Invoke(input));
    }

    public static IEnumerable<TEnum> Values<TEnum>() where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
    }

    public static TEnum[] ValuesOrderedByDescription<TEnum>() where TEnum : Enum
    {
        return Values<TEnum>()
            .OrderBy(x => x.GetDescription())
            .ToArray();
    }

    public static string Description<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        var attributes = value.GetAttributes<DescriptionAttribute, TEnum>().ToArray();
        return attributes.Length > 0
            ? attributes[0].Description
            : throw new NotSupportedException($"Не найден атрибут Description для значения {value}");
    }

    public static bool IsOneOf<TEnum>(this TEnum value, params TEnum[] variants) where TEnum : struct
    {
        return variants.Contains(value);
    }

    public static bool IsOneOf<TEnum>(this TEnum[] value, params TEnum[] variants) where TEnum : struct
    {
        return variants.Intersect(value).Any();
    }

    public static TEnum[] ValuesExcept<TEnum>(params TEnum[] enums) where TEnum : Enum
    {
        return Values<TEnum>().Except(enums).ToArray();
    }

    public static string GetKey<TEnum>(this IEnumerable<TEnum>? enums) where TEnum : Enum
    {
        return enums is null
            ? string.Empty
            : string.Concat(enums.Select(x =>
            {
                var enumString = x.ToString();
                return string.IsNullOrEmpty(enumString) ? string.Empty : enumString;
            }).OrderBy(x => x));
    }

    private static IEnumerable<TAttribute> GetAttributes<TAttribute, TEnum>(this TEnum value)
        where TAttribute : Attribute
        where TEnum : Enum
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (IEnumerable<TAttribute>)fieldInfo!.GetCustomAttributes(typeof(TAttribute), false);
        return attributes;
    }
}
