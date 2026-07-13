namespace CMG.Commands;

public static class BrowserIdleTimeoutParser
{
    public static bool TryParse(string? value, out int? milliseconds, out string? error)
    {
        milliseconds = null;
        error = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        var trimmed = value.Trim();
        var unitStart = trimmed.TakeWhile(char.IsDigit).Count();
        if (unitStart is 0 || !long.TryParse(trimmed[..unitStart], out var amount) || amount <= 0)
        {
            error = "--idle-timeout must be a positive duration such as 30m, 2h, 60s, or 5000ms.";
            return false;
        }

        var multiplier = trimmed[unitStart..].Trim().ToLowerInvariant() switch
        {
            "" or "ms" or "millisecond" or "milliseconds" => 1L,
            "s" or "sec" or "second" or "seconds" => 1_000L,
            "m" or "min" or "minute" or "minutes" => 60_000L,
            "h" or "hour" or "hours" => 3_600_000L,
            _ => 0L
        };
        if (multiplier is 0)
        {
            error = "--idle-timeout must use ms, s, m, or h.";
            return false;
        }
        if (amount > int.MaxValue / multiplier)
        {
            error = "--idle-timeout is too large.";
            return false;
        }

        milliseconds = (int)(amount * multiplier);
        return true;
    }
}
