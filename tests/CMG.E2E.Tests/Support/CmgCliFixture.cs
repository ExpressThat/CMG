namespace CMG.E2E.Tests.Support;

public sealed class CmgCliFixture : IDisposable
{
    private readonly string workspace;

    public CmgCliFixture()
    {
        workspace = Path.Combine(Path.GetTempPath(), "cmg-e2e-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        Directory.CreateDirectory(LocalAppData);
        Directory.CreateDirectory(OutputDirectory);
        Cli = new CmgCli(E2ePaths.RepositoryRoot(), LocalAppData);
    }

    public CmgCli Cli { get; }

    public string LocalAppData => Path.Combine(workspace, "appdata");

    public string OutputDirectory => Path.Combine(workspace, "output");

    public string OutputPath(string name) => Path.Combine(OutputDirectory, name);

    public void Dispose()
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
