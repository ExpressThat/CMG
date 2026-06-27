namespace CMG.Browser.Scripting;

public sealed record BrowserDeviceProfile(
    string Name,
    int Width,
    int Height,
    double DeviceScaleFactor,
    bool IsMobile,
    bool HasTouch,
    string UserAgent);

public static class BrowserDeviceProfiles
{
    private static readonly IReadOnlyList<BrowserDeviceProfile> Profiles =
    [
        new("iPhone 13", 390, 844, 3, true, true, "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1"),
        new("iPhone SE", 375, 667, 2, true, true, "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1"),
        new("Pixel 5", 393, 851, 2.75, true, true, "Mozilla/5.0 (Linux; Android 11; Pixel 5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36"),
        new("Pixel 7", 412, 915, 2.625, true, true, "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36"),
        new("Galaxy S9+", 360, 740, 4, true, true, "Mozilla/5.0 (Linux; Android 10; SM-G965F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36"),
        new("iPad", 768, 1024, 2, true, true, "Mozilla/5.0 (iPad; CPU OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1"),
        new("iPad Pro", 1024, 1366, 2, true, true, "Mozilla/5.0 (iPad; CPU OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1"),
        new("Desktop Chrome", 1280, 720, 1, false, false, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36")
    ];

    public static BrowserDeviceProfile Resolve(string name)
    {
        var profile = Profiles.FirstOrDefault(profile =>
            string.Equals(Normalize(profile.Name), Normalize(name), StringComparison.OrdinalIgnoreCase));
        return profile ?? throw new ScriptExecutionException(
            $"Unknown device '{name}'. Known devices: {string.Join(", ", Profiles.Select(profile => profile.Name))}.");
    }

    private static string Normalize(string value) =>
        string.Concat(value.Where(char.IsLetterOrDigit)).ToLowerInvariant();
}
