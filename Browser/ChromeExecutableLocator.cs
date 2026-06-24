namespace CMG.Browser;

public static class ChromeExecutableLocator
{
    public static string? Find()
    {
        foreach (var path in GetCandidatePaths())
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return FindOnPath();
    }

    private static IEnumerable<string> GetCandidatePaths()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            yield return Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe");
        }

        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            yield return Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe");
        }

        if (!string.IsNullOrWhiteSpace(localAppData))
        {
            yield return Path.Combine(localAppData, "Google", "Chrome", "Application", "chrome.exe");
        }
    }

    private static string? FindOnPath()
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (var directory in pathValue.Split(Path.PathSeparator))
        {
            var chromePath = Path.Combine(directory, "chrome.exe");
            if (File.Exists(chromePath))
            {
                return chromePath;
            }
        }

        return null;
    }
}
