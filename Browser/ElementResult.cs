namespace CMG.Browser;

public enum ElementOutputMode
{
    Html,
    Screenshot
}

public sealed record ElementResult(bool Success, string? Html, byte[]? ScreenshotPng, string? Error)
{
    public static ElementResult ForHtml(string html) => new(true, html, null, null);

    public static ElementResult ForScreenshot(byte[] screenshotPng) => new(true, null, screenshotPng, null);

    public static ElementResult Fail(string error) => new(false, null, null, error);
}
