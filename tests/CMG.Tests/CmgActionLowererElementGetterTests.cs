using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererElementGetterTests
{
    [Theory]
    [InlineData("textContent")]
    [InlineData("innerText")]
    [InlineData("inputValue")]
    [InlineData("getAttribute")]
    public void Lower_ElementGetterPassesThrough(string name)
    {
        string[] args = name == "getAttribute" ? ["#target", "href"] : ["#target"];
        var line = Assert.Single(new CmgActionLowerer().Lower(
            new CmgNode(1, name, name, args, new Dictionary<string, string>(), [])));

        Assert.Equal($"{name} {string.Join(' ', args.Select(arg => $"\"{arg}\""))}", line);
    }
}
