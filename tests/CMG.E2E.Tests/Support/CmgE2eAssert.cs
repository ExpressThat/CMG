namespace CMG.E2E.Tests.Support;

public static class CmgE2eAssert
{
    public static void StdoutContains(this CmgResult result, string expected)
    {
        Assert.True(result.Stdout.Contains(expected, StringComparison.Ordinal), Message("stdout", expected, result));
    }

    public static void StderrContains(this CmgResult result, string expected)
    {
        Assert.True(result.Stderr.Contains(expected, StringComparison.Ordinal), Message("stderr", expected, result));
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

    private static string Message(string stream, string expected, CmgResult result) =>
        $"Expected {stream} to contain '{expected}'.{Environment.NewLine}" +
        $"Command: {string.Join(' ', result.Arguments)}{Environment.NewLine}" +
        $"STDOUT:{Environment.NewLine}{result.Stdout}{Environment.NewLine}" +
        $"STDERR:{Environment.NewLine}{result.Stderr}";
}
