namespace CMG.Commands;

public static class GifDurationOptionParser
{
    public static bool TryParse(string? value, out int? milliseconds, out string? error)
    {
        milliseconds = null;
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        var unitStart = trimmed.TakeWhile(character => char.IsDigit(character)).Count();
        if (unitStart is 0 || !long.TryParse(trimmed[..unitStart], out var amount) || amount < 0)
        {
            error = "--gif-max-duration must be a non-negative duration such as 500ms, 2s, 1m, or 2000.";
            return false;
        }

        var unit = trimmed[unitStart..].Trim().ToLowerInvariant();
        var multiplier = unit switch
        {
            "" or "ms" or "millisecond" or "milliseconds" => 1L,
            "s" or "sec" or "second" or "seconds" => 1000L,
            "m" or "min" or "minute" or "minutes" => 60_000L,
            _ => 0L
        };

        if (multiplier > 0 && amount <= int.MaxValue / multiplier)
        {
            milliseconds = (int)(amount * multiplier);
            return true;
        }

        error = multiplier is 0
            ? "--gif-max-duration must use ms, s, or m."
            : "--gif-max-duration is too large.";
        return false;
    }
}
