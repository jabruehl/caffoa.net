using System.Globalization;
using System.Text;
using CdIts.Caffoa.Cli.Errors;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace CdIts.Caffoa.Cli;

public static class Extensions
{
    public static string ToCamelCase(this string str)
    {
        var parts = str.Replace('-', '_').Split('_');
        return string.Join("", parts.Select(part => part.FirstCharUpper()));
    }

    public static string FirstCharUpper(this string str)
    {
        if (str.Length > 1)
            return str[..1].ToUpper() + str[1..];
        return str;
    }

    public static bool IsPrimitiveType(this OpenApiSchema schema)
    {
        return schema.Type is "string" or "integer" or "number" or "boolean";
    }

    public static bool HasOnlyAdditionalProperties(this OpenApiSchema schema)
    {
        return schema.Type == "object" && schema.AdditionalProperties != null && schema.Properties.Count == 0;
    }
    
    public static bool IsArray(this OpenApiSchema schema)
    {
        return schema.Type is "array";
    }

    public static string GetArrayType(this OpenApiSchema schema, Func<string, string> classNameFunc)
    {
        var item = schema.Items;
        if (item.Reference != null)
            return classNameFunc(item.Reference.Name());
        if (item.IsPrimitiveType())
            return item.TypeName();
        throw new CaffoaParserError(
            $"Cannot parse array trees: the array item should be declared in own schema directly under 'components'");
    }

    public static string TypeName(this OpenApiSchema schema)
    {
        var type = schema.Type;
        var format = schema.Format?.ToLower();
        var suffix = schema.Nullable ? "?" : "";
        return type switch
        {
            "string" when format == "uuid" => $"Guid{suffix}",
            "string" when format is "date-time" => $"DateTime{suffix}",
            "string" when format is "date" => $"DateOnly{suffix}",
            "integer" when format == "int64" => $"long{suffix}",
            "integer" when format == "uint64" => $"ulong{suffix}",
            "integer" when format == "uint32" => $"uint{suffix}",
            "integer" => $"int{suffix}",
            "number" => $"double{suffix}",
            "boolean" => $"bool{suffix}",
            "string" => "string",
            _ => $"{type}{suffix}"
        };
    }

    public static string? DefaultAsString(this OpenApiSchema schema)
    {
        return AnyValue(schema.Default);
    }

    public static string? AnyValue(IOpenApiAny any)
    {
        switch (any)
        {
            case OpenApiString str:
                return $"\"{str.Value}\"";
            case OpenApiBoolean boolean:
                return boolean.Value ? "true" : "false";
            case OpenApiDouble dbl:
                return dbl.Value.ToString(CultureInfo.InvariantCulture);
            case OpenApiFloat flt:
                return flt.Value.ToString(CultureInfo.InvariantCulture);
            case OpenApiInteger integer:
                return integer.Value.ToString();
            case OpenApiLong vLong:
                return vLong.Value.ToString();
            default:
                return null;
        }
    }
    public static List<string?> EnumsAsStrings(this OpenApiSchema schema)
    {
        return schema.Enum.Select(AnyValue).ToList();
    }

    public static string FormatDict(this string str, IDictionary<string, object> values)
    {
        var i = 0;
        var newFormatString = new StringBuilder(str);
        var keyToInt = new Dictionary<string, int>();
        foreach (var key in values.Keys)
        {
            newFormatString = newFormatString.Replace("{" + key + "}", "{" + i + "}");
            keyToInt.Add(key, i);
            i++;
        }

        return string.Format(newFormatString.ToString(),
            values.OrderBy(x => keyToInt[x.Key]).Select(x => x.Value).ToArray());
    }
    
    public static string Name(this OpenApiReference reference)
    {
        return reference.Id.Split("/").Last();
    }
}