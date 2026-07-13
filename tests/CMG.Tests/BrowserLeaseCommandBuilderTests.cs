using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserLeaseCommandBuilderTests
{
    [Fact]
    public void Launch_MapsConservativeIdleOptions()
    {
        var handler = new Handler();

        var exit = Root(handler).Parse("browser --port 9333 launch --headless --idle-timeout 45m").Invoke();

        Assert.Equal(0, exit);
        Assert.Equal(9333, handler.Port);
        Assert.True(handler.Headless);
        Assert.Equal(2_700_000, handler.IdleTimeout);
    }

    [Fact]
    public void LeaseCommands_MapSelectedPortAndDuration()
    {
        var handler = new Handler();
        var root = Root(handler);

        Assert.Equal(0, root.Parse("browser --port 9444 lease status").Invoke());
        Assert.Equal("status", handler.LeaseAction);
        Assert.Equal(0, root.Parse("browser --port 9444 lease keepAlive --idle-timeout 2h").Invoke());
        Assert.Equal("keepAlive", handler.LeaseAction);
        Assert.Equal(7_200_000, handler.IdleTimeout);
        Assert.Equal(0, root.Parse("browser --port 9444 lease disable").Invoke());
        Assert.Equal("disable", handler.LeaseAction);
    }

    [Fact]
    public void Launch_RejectsInvalidDurationBeforeHandler()
    {
        var handler = new Handler();

        Assert.Equal(1, Root(handler).Parse("browser launch --headless --idle-timeout tomorrow").Invoke());
        Assert.False(handler.Launched);
    }

    private static RootCommand Root(Handler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome); root.Options.Add(edge); root.Options.Add(firefox);
        root.Subcommands.Add(new BrowserCommandBuilder(handler, new BrowserControlCommandBuilder(new ControlHandler()))
            .Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class Handler : IBrowserCommandHandler
    {
        public bool Launched { get; private set; }
        public bool Headless { get; private set; }
        public int? Port { get; private set; }
        public int? IdleTimeout { get; private set; }
        public string? LeaseAction { get; private set; }

        public int Launch(BrowserKind kind, IReadOnlyList<string> arguments, bool headless, string? url) => 0;
        public int Launch(BrowserKind kind, IReadOnlyList<string> arguments, bool headless, string? url, int? port, int? timeout, bool disabled)
        {
            Launched = true; Headless = headless; Port = port; IdleTimeout = timeout; return 0;
        }
        public int LeaseStatus(BrowserKind kind, int? port) { LeaseAction = "status"; Port = port; return 0; }
        public int LeaseKeepAlive(BrowserKind kind, int? port, int? timeout) { LeaseAction = "keepAlive"; Port = port; IdleTimeout = timeout; return 0; }
        public int LeaseDisable(BrowserKind kind, int? port) { LeaseAction = "disable"; Port = port; return 0; }
        public int LaunchApp(BrowserKind kind, FileInfo file, string appKind, BrowserAppDebugOptions options, IReadOnlyList<string> arguments) => 0;
        public int AttachApp(BrowserKind kind, BrowserAppDebugOptions options, int processId) => 0;
        public int Close(BrowserKind kind, IReadOnlyList<string> arguments) => 0;
    }

    private sealed class ControlHandler : IBrowserControlCommandHandler
    {
        public int GetElement(BrowserKind kind, string selector, bool html, bool screenshot, FileInfo? output) => 0;
        public int RunScript(BrowserKind kind, string file, FileInfo? gif) => 0;
        public int RunScriptAction(BrowserKind kind, string line) => 0;
        public int ValidateScript(string file) => 0;
    }
}
