namespace CMG.Tests;

public sealed class SourceFileSizeTests
{
    [Fact]
    public void CSharpFiles_DoNotExceed250Lines()
    {
        var root = FindRepositoryRoot();
        var offenders = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => new { Path = Path.GetRelativePath(root, path), Lines = File.ReadLines(path).Count() })
            .Where(file => file.Lines > 250)
            .ToArray();

        Assert.True(offenders.Length is 0, string.Join(Environment.NewLine, offenders.Select(file => $"{file.Path}: {file.Lines}")));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CMG.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
