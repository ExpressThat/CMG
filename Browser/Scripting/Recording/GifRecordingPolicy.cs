namespace CMG.Browser.Scripting.Recording;

public static class GifRecordingPolicy
{
    private static readonly AsyncLocal<int> SuppressionDepth = new();

    public static bool IsDisabled => SuppressionDepth.Value > 0 || EnvironmentDisabled();

    public static string DisabledSource => SuppressionDepth.Value > 0 ? "cli" : "environment";

    public static IDisposable Suppress(bool disabled) => disabled ? new Scope() : NoopScope.Instance;

    private static bool EnvironmentDisabled()
    {
        var value = Environment.GetEnvironmentVariable("CMG_DISABLE_GIF");
        return value?.Trim().ToLowerInvariant() is "1" or "true" or "yes" or "on";
    }

    private sealed class Scope : IDisposable
    {
        private bool disposed;

        public Scope() => SuppressionDepth.Value++;

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            SuppressionDepth.Value = Math.Max(0, SuppressionDepth.Value - 1);
        }
    }

    private sealed class NoopScope : IDisposable
    {
        public static readonly NoopScope Instance = new();
        public void Dispose() { }
    }
}
