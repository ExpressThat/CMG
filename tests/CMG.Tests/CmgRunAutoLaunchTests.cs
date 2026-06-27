using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgRunAutoLaunchTests
{
    [Fact]
    public void Run_MissingBrowserReportsLaunchCommand()
    {
        using var files = new TempRunFile();
        var port = UniquePort();
        var stateStore = new BrowserStateStore();
        stateStore.Clear(BrowserKind.Chrome, port);

        var result = Service(stateStore, new FakeBrowserController()).Run(files.Path, Options(browserPort: port));

        Assert.False(result.Success);
        Assert.Equal($"No CMG-controlled Chrome instance is running. Run 'cmg browser --port {port} launch' first.", result.Error);
    }

    [Fact]
    public void Run_AutoLaunchStartsBrowserWhenStateIsMissing()
    {
        using var files = new TempRunFile();
        var port = UniquePort();
        var stateStore = new BrowserStateStore();
        stateStore.Clear(BrowserKind.Chrome, port);
        var controller = new FakeBrowserController(stateStore);

        var result = Service(stateStore, controller).Run(files.Path, Options(browserPort: port, autoLaunch: true));

        Assert.True(result.Success, result.Error);
        Assert.True(controller.Launched);
        Assert.Contains("TEST PASS empty", result.StdoutLines);
    }

    [Fact]
    public void Run_AutoLaunchHeadlessUsesBrowserHeadlessArgument()
    {
        using var files = new TempRunFile();
        var port = UniquePort();
        var stateStore = new BrowserStateStore();
        stateStore.Clear(BrowserKind.Chrome, port);
        var controller = new FakeBrowserController(stateStore);

        var result = Service(stateStore, controller).Run(files.Path, Options(browserPort: port, autoLaunch: true, autoLaunchHeadless: true));

        Assert.True(result.Success, result.Error);
        Assert.Equal(["--headless=new"], controller.LaunchArguments);
    }

    [Fact]
    public void Run_AutoLaunchFirefoxHeadlessUsesFirefoxArgument()
    {
        using var files = new TempRunFile();
        var port = UniquePort();
        var stateStore = new BrowserStateStore();
        stateStore.Clear(BrowserKind.Firefox, port);
        var controller = new FakeBrowserController(stateStore);

        var result = Service(stateStore, controller).Run(files.Path, Options(browserKind: BrowserKind.Firefox, browserPort: port, autoLaunch: true, autoLaunchHeadless: true));

        Assert.True(result.Success, result.Error);
        Assert.Equal(["--headless"], controller.LaunchArguments);
    }

    [Fact]
    public void Run_AutoLaunchFailureReturnsUsefulError()
    {
        using var files = new TempRunFile();
        var port = UniquePort();
        var stateStore = new BrowserStateStore();
        stateStore.Clear(BrowserKind.Chrome, port);

        var result = Service(stateStore, new FakeBrowserController(null, fail: true)).Run(files.Path, Options(browserPort: port, autoLaunch: true));

        Assert.False(result.Success);
        Assert.Equal("Could not auto-launch Chrome for cmg run. Chrome did not start.", result.Error);
    }

    private static CmgRunOptions Options(
        int? browserPort = null,
        bool autoLaunch = false,
        bool autoLaunchHeadless = false,
        BrowserKind browserKind = BrowserKind.Chrome) =>
        new(
            browserKind,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            0,
            1,
            false,
            1,
            1,
            null,
            null,
            null,
            null,
            new Dictionary<string, string>(),
            string.Empty,
            browserPort,
            autoLaunch,
            autoLaunchHeadless);

    private static CmgRunService Service(BrowserStateStore stateStore, IBrowserController browserController) =>
        new(
            stateStore,
            browserController,
            new BrowserAutomationClientFactory(new ChromeDevToolsClient(), new FirefoxBiDiClient()),
            new BrowserScriptRunner(new BrowserScriptParser()),
            new CmgDslParser(),
            new CmgTestPlanner(),
            new CmgActionLowerer(),
            new CmgValidator(),
            new CmgApiRequestRunner(),
            new CmgStorageStateRunner(),
            new CmgVisualAssertionRunner(),
            new CmgUploadRunner());

    private static int UniquePort() => 30_000 + Random.Shared.Next(10_000);

    private sealed class FakeBrowserController : IBrowserController
    {
        private readonly BrowserStateStore? stateStore;
        private readonly bool fail;

        public FakeBrowserController(BrowserStateStore? stateStore = null, bool fail = false)
        {
            this.stateStore = stateStore;
            this.fail = fail;
        }

        public bool Launched { get; private set; }

        public IReadOnlyList<string> LaunchArguments { get; private set; } = [];

        public BrowserLaunchResult Launch(BrowserKind browserKind, IReadOnlyList<string> additionalArguments, int? remoteDebuggingPort = null)
        {
            Launched = true;
            LaunchArguments = additionalArguments.ToArray();
            if (fail)
            {
                return new BrowserLaunchResult(1, "Chrome did not start.", null);
            }

            var port = remoteDebuggingPort ?? browserKind.DefaultRemoteDebuggingPort();
            var url = $"http://127.0.0.1:{port}";
            stateStore?.Save(browserKind, new BrowserState(123, port, url, string.Empty), remoteDebuggingPort);
            return new BrowserLaunchResult(0, "Chrome launched for CMG. PID: 123.", url);
        }

        public BrowserCloseResult Close(BrowserKind browserKind) => new(0, "closed");

        public BrowserCloseResult Close(BrowserKind browserKind, int? port) => new(0, "closed");
    }

    private sealed class TempRunFile : IDisposable
    {
        private readonly string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public TempRunFile()
        {
            Directory.CreateDirectory(directory);
            Path = System.IO.Path.Combine(directory, "flow.cmgscript");
            File.WriteAllText(Path, """
            test "empty" {
            }
            """);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
