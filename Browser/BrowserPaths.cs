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

    public static void EnsureAppDataDirectory()
    {
        Directory.CreateDirectory(AppDataDirectory);
    }
}
