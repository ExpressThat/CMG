using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class E2eParallelismGuardTests
{
    [Fact]
    public void E2eAssembly_UsesEightFrameworkWorkers()
    {
        var assemblyInfo = File.ReadAllText(Path.Combine(E2ePaths.RepositoryRoot(), "tests", "CMG.E2E.Tests", "AssemblyInfo.cs"));

        Assert.Contains("CollectionBehavior(MaxParallelThreads = 8)", assemblyInfo, StringComparison.Ordinal);
        Assert.DoesNotContain("DisableTestParallelization", assemblyInfo, StringComparison.Ordinal);
    }

    [Fact]
    public void BrowserBackedTests_UsePerClassFixtures()
    {
        var missing = TestFiles()
            .Where(path => File.ReadAllText(path).Contains("CmgBrowserFixture fixture", StringComparison.Ordinal))
            .Where(path => !File.ReadAllText(path).Contains("IClassFixture<CmgBrowserFixture>", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.Empty(missing);
    }

    [Fact]
    public void BrowserBackedTests_DoNotUseSharedCollections()
    {
        var forbidden = TestFiles()
            .Select(path => new { Name = Path.GetFileName(path), Text = File.ReadAllText(path) })
            .Where(file => file.Text.Contains("[Collection(", StringComparison.Ordinal) ||
                           file.Text.Contains("[CollectionDefinition", StringComparison.Ordinal))
            .Select(file => file.Name)
            .ToArray();

        Assert.Empty(forbidden);
    }

    private static IEnumerable<string> TestFiles()
    {
        var testRoot = Path.Combine(E2ePaths.RepositoryRoot(), "tests", "CMG.E2E.Tests");
        return Directory.EnumerateFiles(testRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith("AssemblyInfo.cs", StringComparison.Ordinal))
            .Where(path => !path.EndsWith("E2eParallelismGuardTests.cs", StringComparison.Ordinal));
    }
}
