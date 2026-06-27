namespace CMG.E2E.Tests.Support;

public sealed class CmgBrowserFixture : IDisposable
{
    private readonly string workspace;
    private readonly StaticFixtureServer server;
    private readonly int browserPort;

    public CmgBrowserFixture()
    {
        workspace = Path.Combine(Path.GetTempPath(), "cmg-e2e-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        Directory.CreateDirectory(LocalAppData);
        Directory.CreateDirectory(OutputDirectory);
        server = new StaticFixtureServer(Path.Combine(E2ePaths.RepositoryRoot(), "tests", "CMG.E2E.Tests", "Fixtures"));
        browserPort = FreeTcpPort();
        Cli = new CmgCli(E2ePaths.RepositoryRoot(), LocalAppData, browserPort);

        var launch = Cli.Run("browser", "--port", browserPort.ToString(), "launch", "--headless", "--url", E2ePaths.FixtureUri("index.html"));
        launch.ShouldPass();
    }

    public CmgCli Cli { get; }

    public string LocalAppData => Path.Combine(workspace, "appdata");

    public string OutputDirectory => Path.Combine(workspace, "output");

    public int BrowserPort => browserPort;

    public string CreateScript(string name, string content)
    {
        var path = Path.Combine(workspace, name);
        File.WriteAllText(path, content);
        return path;
    }

    public string OutputPath(string name) => Path.Combine(OutputDirectory, name);

    public string FixtureHttpUri(string name) => server.Url(name);

    public string FixtureWebSocketUri(string path) => server.WebSocketUrl(path);

    public void Dispose()
    {
        try
        {
            Cli.Run("browser", "--port", browserPort.ToString(), "close");
            Cli.KillTrackedBrowser();
        }
        finally
        {
            try
            {
                Directory.Delete(workspace, recursive: true);
            }
            catch (IOException)
            {
            }

            server.Dispose();
        }
    }

    private static int FreeTcpPort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
