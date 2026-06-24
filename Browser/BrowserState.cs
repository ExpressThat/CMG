namespace CMG.Browser;

public sealed record BrowserState(
    int ProcessId,
    int RemoteDebuggingPort,
    string RemoteDebuggingUrl,
    string UserDataDirectory)
{
    public static BrowserState Empty { get; } = new(0, 0, string.Empty, string.Empty);
}
