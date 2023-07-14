using System.Text.RegularExpressions;

namespace B1ServiceLayer.Extensions;

public static class StringExtensions
{
    private static readonly Regex targetWhitespaceRegex = new Regex(@"\s+");

    public static bool In(this string source, params string[] values)
        => values.Contains(source);

    public static string ReplaceWhitespace(this string source, string replacement)
        => targetWhitespaceRegex.Replace(source, replacement);
}
