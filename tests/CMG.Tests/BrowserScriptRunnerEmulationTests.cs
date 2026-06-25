using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerEmulationTests
{
    [Fact]
    public void RunText_EmulateSetsViewportAndPageOverrides()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText(
            "emulate width=390 height=844 deviceScaleFactor=2 isMobile=true hasTouch=true userAgent=\"CMG Mobile\" locale=en-GB colorScheme=dark reducedMotion=reduce timezone=Europe/London geolocation=\"51.5,-0.1\" permissions=geolocation",
            "debug",
            client);

        Assert.True(result.Success);
        Assert.Equal(new(390, 844), client.LastViewport);
        Assert.Equal(2, client.LastViewportOptions?.DeviceScaleFactor);
        Assert.True(client.LastViewportOptions?.IsMobile);
        Assert.True(client.LastViewportOptions?.HasTouch);
        Assert.Contains("userAgent", client.LastExpression);
        Assert.Contains("matchMedia", client.LastExpression);
        Assert.Contains("geolocation", client.LastExpression);
        Assert.Contains("permissions.query", client.LastExpression);
    }

    [Fact]
    public void RunText_SetViewportPassesAdvancedOptions()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("setViewport width=800 height=600 deviceScaleFactor=1.5 isMobile=true hasTouch=true", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(new(800, 600), client.LastViewport);
        Assert.Equal(1.5, client.LastViewportOptions?.DeviceScaleFactor);
        Assert.True(client.LastViewportOptions?.IsMobile);
        Assert.True(client.LastViewportOptions?.HasTouch);
    }

    [Fact]
    public void RunText_SetViewportRejectsInvalidAdvancedOptions()
    {
        var result = Runner().RunText("setViewport width=800 height=600 hasTouch=maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("hasTouch= must be true or false", result.Error);
    }

    [Fact]
    public void RunText_EmulateRequiresCompleteViewport()
    {
        var result = Runner().RunText("emulate width=390", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("both width and height", result.Error);
    }

    [Fact]
    public void Build_RejectsInvalidGeolocation()
    {
        var options = new Dictionary<string, string> { ["geolocation"] = "north" };

        Assert.Throws<ScriptExecutionException>(() => BrowserEmulationScript.Build(options));
    }

    [Fact]
    public void RunText_SetGeolocationInstallsPageOverride()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("setGeolocation \"51.5,-0.1\" accuracy=10", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("geolocation", client.LastExpression);
        Assert.Contains("accuracy: 10", client.LastExpression);
        Assert.Contains("GEOLOCATION", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_GrantAndClearPermissionsInstallPermissionOverride()
    {
        var client = new FakeAutomationClient();

        var grant = Runner().RunText("grantPermissions \"geolocation\" \"notifications\"", "debug", client);
        var clear = Runner().RunText("clearPermissions", "debug", client);

        Assert.True(grant.Success);
        Assert.True(clear.Success);
        Assert.Contains("permissions.query", client.LastExpression);
        Assert.Contains("geolocation,notifications", string.Join('\n', grant.StdoutLines));
        Assert.Contains("PERMISSIONS_CLEARED", string.Join('\n', clear.StdoutLines));
    }

    [Fact]
    public void RunText_GrantPermissionsRequiresPermission()
    {
        var result = Runner().RunText("grantPermissions", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("requires at least one permission", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
