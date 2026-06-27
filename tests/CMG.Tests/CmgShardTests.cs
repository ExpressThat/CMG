using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgShardTests
{
    [Fact]
    public void TryParse_DefaultsToSingleShard()
    {
        Assert.True(CmgShard.TryParse(null, out var index, out var count, out _));
        Assert.Equal(1, index);
        Assert.Equal(1, count);
    }

    [Fact]
    public void TryParse_ReadsIndexAndCount()
    {
        Assert.True(CmgShard.TryParse("2/4", out var index, out var count, out _));
        Assert.Equal(2, index);
        Assert.Equal(4, count);
    }

    [Theory]
    [InlineData("0/2")]
    [InlineData("3/2")]
    [InlineData("x/y")]
    public void TryParse_RejectsInvalidShard(string value)
    {
        Assert.False(CmgShard.TryParse(value, out _, out _, out var error));
        Assert.Contains("index/count", error);
    }
}
