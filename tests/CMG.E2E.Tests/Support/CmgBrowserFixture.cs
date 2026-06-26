namespace CMG.E2E.Tests.Support;

public sealed class CmgBrowserFixture : IDisposable
{
    private readonly string workspace;

    public CmgBrowserFixture()
    {
        workspace = Path.Combine(Path.GetTempPath(), "cmg-e2e-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        Directory.CreateDirectory(LocalAppData);
        Directory.CreateDirectory(OutputDirectory);
        Cli = new CmgCli(E2ePaths.RepositoryRoot(), LocalAppData);

        var launch = Cli.Run("browser", "launch", "--headless", "--url", E2ePaths.FixtureUri("index.html"));
        launch.ShouldPass();
    }

    public CmgCli Cli { get; }

    public string LocalAppData => Path.Combine(workspace, "appdata");

    public string OutputDirectory => Path.Combine(workspace, "output");

    public string CreateScript(string name, string content)
    {
        var path = Path.Combine(workspace, name);
        File.WriteAllText(path, content);
        return path;
    }

    public string OutputPath(string name) => Path.Combine(OutputDirectory, name);

    public void Dispose()
    {
        try
        {
            Cli.Run("browser", "close");
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
        }
    }
}

[CollectionDefinition(Name)]
public sealed class CmgE2eCollection : ICollectionFixture<CmgBrowserFixture>
{
    public const string Name = "CMG browser E2E";
}
