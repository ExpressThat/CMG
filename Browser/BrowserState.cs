namespace CMG.Browser;

public sealed record BrowserState(
    int ProcessId,
    int RemoteDebuggingPort,
    string RemoteDebuggingUrl,
    string UserDataDirectory,
    bool IsHeadless = false,
    string OwnershipToken = "",
    long ProcessStartTimeUtcTicks = 0,
    long LastActivityUtcTicks = 0,
    int IdleTimeoutMilliseconds = 0)
{
    public static BrowserState Empty { get; } = new(0, 0, string.Empty, string.Empty);

    public bool HasIdleLease =>
        IsHeadless &&
        ProcessId > 0 &&
        OwnershipToken.Length > 0 &&
        LastActivityUtcTicks > 0 &&
        IdleTimeoutMilliseconds > 0;

    public DateTimeOffset? IdleDeadline => HasIdleLease
        ? new DateTimeOffset(LastActivityUtcTicks, TimeSpan.Zero).AddMilliseconds(IdleTimeoutMilliseconds)
        : null;
}
