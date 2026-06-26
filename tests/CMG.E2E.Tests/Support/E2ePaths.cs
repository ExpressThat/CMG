namespace CMG.E2E.Tests.Support;

public static class E2ePaths
{
    public static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CMG.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate CMG repository root.");
    }

    public static string FixtureFile(string name) =>
        Path.Combine(RepositoryRoot(), "tests", "CMG.E2E.Tests", "Fixtures", name);

    public static string FixtureUri(string name) =>
        new Uri(FixtureFile(name)).AbsoluteUri;
}
