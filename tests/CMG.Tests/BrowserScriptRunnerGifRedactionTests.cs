using System.Text.Json;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifRedactionTests
{
    [Fact]
    public void RedactionActions_SkipWithoutRecordingOrVirtualPointer()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("maskGif \"${missing}\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_REDACT", StringComparison.Ordinal) && line.Contains("status=skipped", StringComparison.Ordinal));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
        Assert.Empty(client.EvaluatedExpressions);
    }

    [Fact]
    public void MaskAndUnmask_CreateFrameOnlyOverlaysAndTimelineAudit()
    {
        using var artifact = new TempGifArtifact();
        var client = new FakeAutomationClient();
        var script = """
            maskGif "#secret" style=replacement replacement="Hidden" padding=4
            caption "masked"
            unmaskGif "#secret"
            caption "visible"
            """;

        var result = Runner().RunText(script, "debug", client, artifact.Gif, gifTimelinePath: artifact.Timeline);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("#secret", StringComparison.Ordinal) && expression.Contains("Hidden", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("data-cmg-gif-redaction", StringComparison.Ordinal) && expression.Contains("remove", StringComparison.Ordinal));
        using var timeline = JsonDocument.Parse(File.ReadAllText(artifact.Timeline));
        var audit = timeline.RootElement.GetProperty("redactions").GetProperty("audit").EnumerateArray().ToArray();
        Assert.Equal(["add", "remove"], audit.Select(entry => entry.GetProperty("operation").GetString() ?? string.Empty).ToArray());
    }

    [Fact]
    public void GifBlock_RedactOptionAppliesInheritedReplacement()
    {
        using var artifact = new TempGifArtifact();
        var client = new FakeAutomationClient();
        var output = artifact.Gif.FullName.Replace('\\', '/');
        var script = $$"""
            gif "privacy" output="{{output}}" redact="#token" redactStyle=replacement redactReplacement="Protected" {
              caption "evidence"
            }
            """;

        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("#token", StringComparison.Ordinal) && expression.Contains("Protected", StringComparison.Ordinal));
    }

    [Fact]
    public void AutomaticAndStrictModesInstallPrivacyChecks()
    {
        using var artifact = new TempGifArtifact();
        var client = new FakeAutomationClient();

        var result = Runner().RunText(
            "recording autoRedact=sensitive redactionSafety=strict { caption \"safe\" }",
            "debug",
            client,
            artifact.Gif);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("github_pat", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("GIF redaction safety blocked capture", StringComparison.Ordinal));
    }

    [Fact]
    public void Recording_DefaultsToAutomaticPasswordMasking()
    {
        using var artifact = new TempGifArtifact();
        var client = new FakeAutomationClient();

        var result = Runner().RunText("caption \"safe\"", "debug", client, artifact.Gif);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("input[type=\"password\"]", StringComparison.Ordinal));
    }

    [Fact]
    public void EncodingDefaults_ApplyWholeRunRedactionRules()
    {
        using var artifact = new TempGifArtifact();
        var client = new FakeAutomationClient();
        var redaction = new GifRedactionOptions([
            new GifRedactionRule("cli-blur-1", "#secret", GifRedactionStyle.Blur, "#111827", "[redacted]", 0)
        ], GifAutoRedactionMode.None, Strict: true);

        var result = Runner().RunText("caption \"safe\"", "debug", client, artifact.Gif,
            gifEncoding: new GifEncodingOptions(Redaction: redaction));

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("#secret", StringComparison.Ordinal) && expression.Contains("blur", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("GIF redaction safety blocked capture", StringComparison.Ordinal));
    }

    [Fact]
    public void InvalidRedactionStyleExplainsAcceptedValues()
    {
        using var artifact = new TempGifArtifact();

        var result = Runner().RunText("maskGif \"#secret\" style=hidden", "debug", new FakeAutomationClient(), artifact.Gif);

        Assert.False(result.Success);
        Assert.Contains("solid, blur, or replacement", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void RedactionOptions_ParseMultipleRulesAndSafetyDefaults()
    {
        var options = GifRedactionOptions.FromOptions(new Dictionary<string, string>
        {
            ["redact"] = "#email; getByLabel=Account",
            ["redactStyle"] = "blur",
            ["redactPadding"] = "6",
            ["autoRedact"] = "sensitive",
            ["redactionSafety"] = "strict"
        }, "gif option");

        Assert.Equal(GifAutoRedactionMode.Sensitive, options.Auto);
        Assert.True(options.Strict);
        Assert.Equal(["#email", "getByLabel=Account"], options.EffectiveRules.Select(rule => rule.Locator).ToArray());
        Assert.All(options.EffectiveRules, rule =>
        {
            Assert.Equal(GifRedactionStyle.Blur, rule.Style);
            Assert.Equal(6, rule.Padding);
        });
    }

    [Theory]
    [InlineData("autoRedact", "sometimes", "passwords, sensitive, or none")]
    [InlineData("redactionSafety", "paranoid", "standard or strict")]
    [InlineData("redactPadding", "101", "between 0 and 100")]
    public void InvalidPrivacyOptions_ExplainAcceptedValues(string name, string value, string expected)
    {
        var error = Assert.Throws<ScriptExecutionException>(() =>
            GifRedactionOptions.FromOptions(new Dictionary<string, string> { [name] = value }, "gif option"));

        Assert.Contains(expected, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UnmaskWithoutSelector_ClearsAllPersistentRules()
    {
        using var artifact = new TempGifArtifact();
        var script = "maskGif \"#one\"\nmaskGif \"#two\"\nunmaskGif\ncaption \"clear\"";

        var result = Runner().RunText(script, "debug", new FakeAutomationClient(), artifact.Gif, gifTimelinePath: artifact.Timeline);

        Assert.True(result.Success, result.Error);
        using var timeline = JsonDocument.Parse(File.ReadAllText(artifact.Timeline));
        var operations = timeline.RootElement.GetProperty("redactions").GetProperty("audit")
            .EnumerateArray().Select(entry => entry.GetProperty("operation").GetString() ?? string.Empty).ToArray();
        Assert.Equal(["add", "add", "clear", "clear"], operations);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGifArtifact : IDisposable
    {
        public TempGifArtifact()
        {
            Gif = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
            Timeline = Path.ChangeExtension(Gif.FullName, ".timeline.json");
        }

        public FileInfo Gif { get; }

        public string Timeline { get; }

        public void Dispose()
        {
            if (Gif.Exists) Gif.Delete();
            if (File.Exists(Timeline)) File.Delete(Timeline);
        }
    }
}
