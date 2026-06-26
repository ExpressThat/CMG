namespace CMG.Browser;

public static class BrowserPaths
{
    private static readonly string AppDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CMG");

    public static string StateFile => Path.Combine(AppDataDirectory, "browser.state");

    public static string UserDataDirectory => Path.Combine(AppDataDirectory, "chrome-profile");

    public static string GetStateFile(BrowserKind browserKind) =>
        browserKind is BrowserKind.Chrome
            ? StateFile
            : Path.Combine(AppDataDirectory, $"{browserKind.StateName()}.browser.state");

    public static string GetUserDataDirectory(BrowserKind browserKind) =>
        browserKind is BrowserKind.Chrome
            ? UserDataDirectory
            : Path.Combine(AppDataDirectory, $"{browserKind.StateName()}-profile");

    public static string GetActiveTargetFile(string key) =>
        Path.Combine(AppDataDirectory, $"active-target-{Sanitize(key)}.state");

    public static void EnsureAppDataDirectory()
    {
        Directory.CreateDirectory(AppDataDirectory);
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(character => invalid.Contains(character) ? '_' : character));
    }
}
