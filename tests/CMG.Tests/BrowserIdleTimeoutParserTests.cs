using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserIdleTimeoutParserTests
{
    [Theory]
    [InlineData("5000", 5000)]
    [InlineData("30s", 30000)]
    [InlineData("15m", 900000)]
    [InlineData("2h", 7200000)]
    public void TryParse_AcceptsAgentScaleDurations(string value, int expected)
    {
        Assert.True(BrowserIdleTimeoutParser.TryParse(value, out var actual, out var error), error);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("soon")]
    [InlineData("2d")]
    public void TryParse_RejectsUnsafeOrUnknownDurations(string value)
    {
        Assert.False(BrowserIdleTimeoutParser.TryParse(value, out _, out var error));
        Assert.Contains("--idle-timeout", error, StringComparison.Ordinal);
    }
}
