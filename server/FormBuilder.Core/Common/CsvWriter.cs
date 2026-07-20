using System.Text;

namespace FormBuilder.Core.Common;

public static class CsvWriter
{
    // Serializes rows to RFC 4180 CSV. Values containing quotes, commas, or
    // line breaks are wrapped in double quotes; embedded quotes are doubled.
    public static string Write(IEnumerable<string> headers, IEnumerable<IEnumerable<string?>> rows)
    {
        var sb = new StringBuilder();
        AppendRow(sb, headers);
        foreach (var row in rows)
        {
            AppendRow(sb, row);
        }
        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, IEnumerable<string?> cells)
    {
        var first = true;
        foreach (var cell in cells)
        {
            if (!first) sb.Append(',');
            sb.Append(Escape(cell));
            first = false;
        }
        sb.Append("\r\n");
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var needsQuoting = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        if (!needsQuoting) return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
