using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class GifSizeBudgetOptionsTests
{
    [Theory]
    [InlineData("500KB", 512000)]
    [InlineData("1.5MB", 1572864)]
    [InlineData("42", 42)]
    public void ParseSize_AcceptsDocumentedUnits(string value, long expected) =>
        Assert.Equal(expected, GifSizeBudgetOptions.ParseSize(value, "gif"));

    [Fact]
    public void FromOptions_InheritsAndOverridesFallbacks()
    {
        var result = GifSizeBudgetOptions.FromOptions(new Dictionary<string, string>
        {
            ["sizeBudget"] = "2MB",
            ["budgetDownscaleFallback"] = "false"
        }, "gif", new GifSizeBudgetOptions(10, false, true));

        Assert.Equal(2 * 1024 * 1024, result.Bytes);
        Assert.False(result.QualityFallback);
        Assert.False(result.DownscaleFallback);
    }

    [Fact]
    public void ParseSize_RejectsInvalidValue() =>
        Assert.Throws<ScriptExecutionException>(() => GifSizeBudgetOptions.ParseSize("lots", "gif"));
}
