using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererClickVariantTests
{
    [Theory]
    [InlineData("doubleClick", "doubleClick \"#target\" modifiers=\"Shift\" x=\"8\"")]
    [InlineData("contextClick", "contextClick \"#target\" modifiers=\"Shift\" x=\"8\"")]
    public void Lower_ClickVariantsPreservePageFacingAction(string name, string expected)
    {
        var lines = new CmgActionLowerer().Lower(new CmgNode(
            1,
            name,
            name,
            ["#target"],
            new Dictionary<string, string>
            {
                ["modifiers"] = "Shift",
                ["x"] = "8"
            },
            []));

        Assert.Equal(expected, lines.Last());
    }
}
