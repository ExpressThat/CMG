namespace CMG.Runner;

internal sealed record CmgGifDiagnostic(string Severity, string Message);

internal static class CmgGifDiagnostics
{
    public static IReadOnlyList<CmgGifDiagnostic> For(CmgTestResult test)
    {
        var diagnostics = test.Output
            .Where(line => line.StartsWith("GIF_WARN_", StringComparison.Ordinal) ||
                           line.StartsWith("GIF_SETTINGS_WARN", StringComparison.Ordinal))
            .Select(line => new CmgGifDiagnostic("warning", line))
            .ToList();
        if (IsRecordingSettingsError(test.Error))
            diagnostics.Add(new CmgGifDiagnostic("error", test.Error!));
        return diagnostics.Distinct().ToArray();
    }

    private static bool IsRecordingSettingsError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error)) return false;
        var value = error.ToLowerInvariant();
        var recordingTerm = new[]
        {
            "gif", "recording", "pointer", "caption", "palette", "dither", "redact", "crop", "timeline"
        }.Any(value.Contains);
        return recordingTerm && (value.Contains("option") || value.Contains("must be") || value.Contains("expects"));
    }
}
