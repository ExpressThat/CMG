using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserAppCommandBuilderTests
{
    [Fact]
    public void AppLaunch_MapsElectronDefaults()
    {
        var handler = new CapturingBrowserCommandHandler();
        var exitCode = BuildRoot(handler).Parse("browser app launch C:\\apps\\demo.exe -- --profile demo").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal("C:\\apps\\demo.exe", handler.Executable?.FullName);
        Assert.Equal("electron", handler.Kind);
        Assert.Equal(9222, handler.Port);
        Assert.Equal("127.0.0.1", handler.Host);
        Assert.Equal(10_000, handler.ConnectTimeoutMilliseconds);
        Assert.Equal(["--profile", "demo"], handler.Arguments);
    }

    [Fact]
    public void AppLaunch_MapsWebView2AndEdgeState()
    {
        var handler = new CapturingBrowserCommandHandler();
        var exitCode = BuildRoot(handler).Parse("--edge browser app launch C:\\apps\\demo.exe --kind webview2 --port 9333").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Edge, handler.BrowserKind);
        Assert.Equal("webview2", handler.Kind);
        Assert.Equal(9333, handler.Port);
    }

    [Fact]
    public void AppAttach_MapsPortAndPid()
    {
        var handler = new CapturingBrowserCommandHandler();
        var exitCode = BuildRoot(handler).Parse("browser app attach --port 9444 --host localhost --connect-timeout 1 --pid 123").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(9444, handler.Port);
        Assert.Equal("localhost", handler.Host);
        Assert.Equal(1, handler.ConnectTimeoutMilliseconds);
        Assert.Equal(123, handler.ProcessId);
    }

    [Fact]
    public void BrowserPort_MapsLaunchAndCloseInstanceSelection()
    {
        var handler = new CapturingBrowserCommandHandler();
        var root = BuildRoot(handler);

        Assert.Equal(0, root.Parse("browser --port 9333 launch --headless --url https://example.test").Invoke());
        Assert.Equal(9333, handler.LaunchPort);

        Assert.Equal(0, root.Parse("browser --port 9333 close").Invoke());
        Assert.Equal(9333, handler.ClosePort);
    }

    [Fact]
    public void BrowserPort_MapsControlCommandsToSelectedInstance()
    {
        var controlHandler = new NoopBrowserControlCommandHandler();
        var exitCode = BuildRoot(new CapturingBrowserCommandHandler(), controlHandler)
            .Parse("browser --port 9334 control script --file flow.cmgscript")
            .Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(9334, controlHandler.Port);
    }

    private static RootCommand BuildRoot(
        CapturingBrowserCommandHandler browserHandler,
        NoopBrowserControlCommandHandler? controlHandler = null)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        var browserOptions = new BrowserSelectionOptions(chrome, edge, firefox);
        root.Options.Add(chrome);
        root.Options.Add(edge);
        root.Options.Add(firefox);
        root.Subcommands.Add(new BrowserCommandBuilder(
            browserHandler,
            new BrowserControlCommandBuilder(controlHandler ?? new NoopBrowserControlCommandHandler()))
            .Build(browserOptions));
        return root;
    }

    private sealed class CapturingBrowserCommandHandler : IBrowserCommandHandler
    {
        public BrowserKind BrowserKind { get; private set; }

        public FileInfo? Executable { get; private set; }

        public string? Kind { get; private set; }

        public int Port { get; private set; }

        public string? Host { get; private set; }

        public int ConnectTimeoutMilliseconds { get; private set; }

        public int ProcessId { get; private set; }

        public int? LaunchPort { get; private set; }

        public int? ClosePort { get; private set; }

        public IReadOnlyList<string> Arguments { get; private set; } = [];

        public int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url) => 0;

        public int Launch(BrowserKind browserKind, IReadOnlyList<string> arguments, bool headless, string? url, int? port)
        {
            LaunchPort = port;
            return 0;
        }

        public int LaunchApp(
            BrowserKind browserKind,
            FileInfo executable,
            string kind,
            BrowserAppDebugOptions options,
            IReadOnlyList<string> arguments)
        {
            BrowserKind = browserKind;
            Executable = executable;
            Kind = kind;
            CaptureOptions(options);
            Arguments = arguments;
            return 0;
        }

        public int AttachApp(BrowserKind browserKind, BrowserAppDebugOptions options, int processId)
        {
            BrowserKind = browserKind;
            CaptureOptions(options);
            ProcessId = processId;
            return 0;
        }

        public int Close(BrowserKind browserKind, IReadOnlyList<string> arguments) => 0;

        public int Close(BrowserKind browserKind, IReadOnlyList<string> arguments, int? port)
        {
            ClosePort = port;
            return 0;
        }

        private void CaptureOptions(BrowserAppDebugOptions options)
        {
            Port = options.Port;
            Host = options.Host;
            ConnectTimeoutMilliseconds = options.ConnectTimeoutMilliseconds;
        }
    }

    private sealed class NoopBrowserControlCommandHandler : IBrowserControlCommandHandler
    {
        public int? Port { get; private set; }

        public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif) => 0;

        public int RunScript(
            BrowserKind browserKind,
            int? port,
            string file,
            FileInfo? gif,
            FileInfo? trace,
            ScriptTimeoutOptions? timeouts,
            string? baseUrl,
            IReadOnlyDictionary<string, string> variables,
            GifQuality gifQuality = GifQuality.Highest,
            ScriptPointerMotionOptions? pointerMotion = null,
            ClickPulseStyle clickPulse = ClickPulseStyle.Ring,
            int holdAfterActionMilliseconds = ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds,
            int holdOnFailureMilliseconds = ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds)
        {
            Port = port;
            return 0;
        }

        public int RunScriptAction(BrowserKind browserKind, string scriptLine) => 0;

        public int ValidateScript(string file) => 0;
    }
}
