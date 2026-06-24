namespace CMG.Browser;

public static class BrowserPaths
{
    private static readonly string AppDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CMG");

    public static string StateFile => Path.Combine(AppDataDirectory, "browser.state");

    public static string UserDataDirectory => Path.Combine(AppDataDirectory, "chrome-profile");

    public static void EnsureAppDataDirectory()
    {
        Directory.CreateDirectory(AppDataDirectory);
    }
}
