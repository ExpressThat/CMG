using System.Globalization;

namespace CMG.Browser;

public static class PdfPaper
{
    private static readonly IReadOnlyDictionary<string, (double Width, double Height)> Formats =
        new Dictionary<string, (double Width, double Height)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Letter"] = (8.5, 11),
            ["Legal"] = (8.5, 14),
            ["Tabloid"] = (11, 17),
            ["Ledger"] = (17, 11),
            ["A0"] = (33.1102, 46.811),
            ["A1"] = (23.3858, 33.1102),
            ["A2"] = (16.5354, 23.3858),
            ["A3"] = (11.6929, 16.5354),
            ["A4"] = (8.2677, 11.6929),
            ["A5"] = (5.8268, 8.2677),
            ["A6"] = (4.1339, 5.8268)
        };

    public static (double Width, double Height)? TryFormat(string? format) =>
        string.IsNullOrWhiteSpace(format) ? null :
        Formats.TryGetValue(format, out var size) ? size : null;

    public static double? Inches(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        var unitStart = trimmed.TakeWhile(ch => char.IsDigit(ch) || ch is '.' or ',').Count();
        var number = trimmed[..unitStart].Replace(',', '.');
        var unit = trimmed[unitStart..].Trim().ToLowerInvariant();
        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            return null;
        }

        return unit switch
        {
            "" or "in" or "inch" or "inches" => parsed,
            "cm" => parsed / 2.54,
            "mm" => parsed / 25.4,
            "px" => parsed / 96,
            _ => null
        };
    }

    public static double? Centimeters(string? value) => Inches(value) * 2.54;
}
