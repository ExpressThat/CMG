using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class E2eFileSizeGuardTests
{
    private const int MaxCSharpLines = 250;

    [Fact]
    public void E2eCSharpFiles_StayUnderLineLimit()
    {
        var oversized = Directory
            .EnumerateFiles(E2eTestRoot(), "*.cs", SearchOption.AllDirectories)
            .Select(path => new { Path = path, Lines = File.ReadLines(path).Count() })
            .Where(file => file.Lines > MaxCSharpLines)
            .Select(file => $"{Relative(file.Path)} has {file.Lines} lines")
            .ToArray();

        Assert.Empty(oversized);
    }

    private static string E2eTestRoot() =>
        Path.Combine(E2ePaths.RepositoryRoot(), "tests", "CMG.E2E.Tests");

    private static string Relative(string path) =>
        Path.GetRelativePath(E2eTestRoot(), path).Replace('\\', '/');
}
