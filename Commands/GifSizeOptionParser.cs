namespace CMG.Commands;

public static class GifSizeOptionParser
{
    public static bool TryParse(string? value, out long? bytes, out string? error, string optionName = "--gif-warn-size")
    {
        bytes = null;
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var text = value.Trim();
        var suffixStart = text.TakeWhile(character => char.IsDigit(character)).Count();
        if (suffixStart is 0 || !long.TryParse(text[..suffixStart], out var number) || number < 0)
        {
            error = $"{optionName} must be a non-negative size such as 500KB, 2MB, or 1048576.";
            return false;
        }

        var suffix = text[suffixStart..].Trim().ToLowerInvariant();
        var multiplier = suffix switch
        {
            "" or "b" or "byte" or "bytes" => 1L,
            "k" or "kb" or "kib" => 1024L,
            "m" or "mb" or "mib" => 1024L * 1024L,
            "g" or "gb" or "gib" => 1024L * 1024L * 1024L,
            _ => 0L
        };
        if (multiplier is 0 || number > long.MaxValue / multiplier)
        {
            error = $"{optionName} must use bytes, KB, MB, or GB.";
            return false;
        }

        bytes = number * multiplier;
        return true;
    }
}
