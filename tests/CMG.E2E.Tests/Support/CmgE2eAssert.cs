namespace CMG.E2E.Tests.Support;

public static class CmgE2eAssert
{
    public static void StdoutContains(this CmgResult result, string expected)
    {
        Assert.Contains(expected, result.Stdout);
    }

    public static void StderrContains(this CmgResult result, string expected)
    {
        Assert.Contains(expected, result.Stderr);
    }

    public static void FileExists(string path)
    {
        Assert.True(File.Exists(path), $"Expected file to exist: {path}");
        Assert.True(new FileInfo(path).Length > 0, $"Expected file to be non-empty: {path}");
    }

    public static void DirectoryHasFiles(string path, string pattern)
    {
        Assert.True(Directory.Exists(path), $"Expected directory to exist: {path}");
        Assert.NotEmpty(Directory.GetFiles(path, pattern));
    }
}
