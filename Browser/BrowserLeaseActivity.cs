namespace CMG.Browser;

public sealed class BrowserLeaseActivity : IDisposable
{
    private readonly BrowserStateStore stateStore;
    private readonly BrowserKind browserKind;
    private readonly int? port;
    private readonly Timer? timer;

    internal BrowserLeaseActivity(
        BrowserStateStore stateStore,
        BrowserKind browserKind,
        int? port,
        BrowserState? state)
    {
        this.stateStore = stateStore;
        this.browserKind = browserKind;
        this.port = port;
        State = state;
        if (state?.HasIdleLease is true)
        {
            var interval = Math.Clamp(state.IdleTimeoutMilliseconds / 3, 500, 30_000);
            timer = new Timer(_ => Renew(), null, interval, interval);
        }
    }

    public BrowserState? State { get; private set; }

    public void Dispose() => timer?.Dispose();

    private void Renew()
    {
        try
        {
            State = stateStore.Renew(browserKind, port);
        }
        catch (IOException)
        {
        }
    }
}
