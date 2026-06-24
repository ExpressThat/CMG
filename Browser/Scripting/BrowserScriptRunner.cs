using System.Text.RegularExpressions;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private readonly BrowserScriptParser parser;
    private readonly ChromeDevToolsClient devToolsClient;

    public BrowserScriptRunner(
        BrowserScriptParser parser,
        ChromeDevToolsClient devToolsClient)
    {
        this.parser = parser;
        this.devToolsClient = devToolsClient;
    }

    public ScriptRunResult Run(string file, string remoteDebuggingUrl, FileInfo? gif)
    {
        var readResult = ReadScript(file);
        if (!readResult.Success)
        {
            return ScriptRunResult.Fail(readResult.Error ?? "Could not read script.");
        }

        return RunParsedScript(readResult.Script ?? string.Empty, remoteDebuggingUrl, gif);
    }

    public ScriptRunResult RunText(string script, string remoteDebuggingUrl)
    {
        return RunParsedScript(script, remoteDebuggingUrl, gif: null);
    }

    private ScriptRunResult RunParsedScript(string script, string remoteDebuggingUrl, FileInfo? gif)
    {
        var parseResult = parser.Parse(script);
        if (!parseResult.Success)
        {
            return ScriptRunResult.Fail(parseResult.Error ?? "Could not parse script.");
        }

        var context = new ScriptExecutionContext();
        var output = new List<string>();
        using var recorder = gif is null
            ? null
            : new ScriptGifRecorder(devToolsClient, new ScriptRecordingOptions(gif.FullName));

        recorder?.Start(remoteDebuggingUrl);

        for (var index = 0; index < parseResult.Actions.Count; index++)
        {
            var stepNumber = index + 1;
            var action = ExpandVariables(parseResult.Actions[index], context);

            try
            {
                recorder?.BeforeAction(action);
                var stepOutput = ExecuteAction(remoteDebuggingUrl, action, context, recorder);
                recorder?.AfterAction(action);
                output.Add($"PASS {stepNumber:000} {action.Name} {FormatActionForLog(action)}".TrimEnd());
                output.AddRange(stepOutput);
            }
            catch (Exception exception) when (exception is ScriptExecutionException or ChromeDevToolsException or ElementNotFoundException)
            {
                FinishRecording(recorder, output);

                return ScriptRunResult.Fail(
                    $"Line {action.LineNumber}: {action.Name} failed. {exception.Message}",
                    output);
            }
        }

        FinishRecording(recorder, output);

        return ScriptRunResult.Ok(output);
    }

    private IReadOnlyList<string> ExecuteAction(
        string remoteDebuggingUrl,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? recorder)
    {
        if (action.Children.Count > 0 && !string.Equals(action.Name, "dragAndDrop", StringComparison.OrdinalIgnoreCase))
        {
            throw new ScriptExecutionException($"Action '{action.Name}' does not accept a block body.");
        }

        return action.Name.ToLowerInvariant() switch
        {
            "navigate" => ExecuteNavigate(remoteDebuggingUrl, action),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, action),
            "click" => ExecuteSelectorAction(action, selector => devToolsClient.Click(remoteDebuggingUrl, selector)),
            "type" => ExecuteType(remoteDebuggingUrl, action, recorder),
            "clear" => ExecuteSelectorAction(action, selector => devToolsClient.Clear(remoteDebuggingUrl, selector)),
            "press" => ExecutePress(remoteDebuggingUrl, action),
            "hover" => ExecuteSelectorAction(action, selector => devToolsClient.Hover(remoteDebuggingUrl, selector)),
            "scrollintoview" => ExecuteSelectorAction(action, selector => devToolsClient.ScrollElementIntoView(remoteDebuggingUrl, selector)),
            "select" => ExecuteSelect(remoteDebuggingUrl, action),
            "delay" => ExecuteDelay(action),
            "html" => ExecuteHtml(remoteDebuggingUrl, action),
            "screenshot" => ExecuteScreenshot(remoteDebuggingUrl, action),
            "screenshotpage" => ExecuteScreenshotPage(remoteDebuggingUrl, action),
            "asserttext" => ExecuteAssertText(remoteDebuggingUrl, action),
            "evaluate" => ExecuteEvaluate(remoteDebuggingUrl, action),
            "setviewport" => ExecuteSetViewport(remoteDebuggingUrl, action),
            "draganddrop" => ExecuteDragAndDrop(remoteDebuggingUrl, action, recorder),
            "listtabs" => ExecuteListTabs(remoteDebuggingUrl, action),
            "activatetab" => ExecuteActivateTab(remoteDebuggingUrl, action),
            "closetab" => ExecuteCloseTab(remoteDebuggingUrl, action),
            "set" => ExecuteSet(action, context),
            _ => throw new ScriptExecutionException($"Unknown action '{action.Name}'.")
        };
    }

    private IReadOnlyList<string> ExecuteNavigate(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        devToolsClient.Navigate(remoteDebuggingUrl, NormalizeNavigationTarget(action.Arguments[0]));
        return [];
    }

    private IReadOnlyList<string> ExecuteWaitForElement(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        devToolsClient.WaitForElement(remoteDebuggingUrl, action.Arguments[0], timeout);
        return [];
    }

    private IReadOnlyList<string> ExecuteSelectorAction(BrowserScriptAction action, Action<string> execute)
    {
        RequireArgumentCount(action, 1, 1);
        execute(action.Arguments[0]);
        return [];
    }

    private IReadOnlyList<string> ExecuteType(string remoteDebuggingUrl, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        RequireArgumentCount(action, 2, 2);
        if (recorder is null)
        {
            devToolsClient.Type(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
            return [];
        }

        recorder.CaptureClickPulse();
        devToolsClient.TypeProgressively(
            remoteDebuggingUrl,
            action.Arguments[0],
            action.Arguments[1],
            recorder.CaptureTypingFrame);

        return [];
    }

    private IReadOnlyList<string> ExecutePress(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        devToolsClient.Press(remoteDebuggingUrl, action.Arguments[0]);
        return [];
    }

    private IReadOnlyList<string> ExecuteSelect(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        devToolsClient.Select(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteDelay(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        Thread.Sleep(ParsePositiveInt(action.Arguments[0], "delay"));
        return [];
    }

    private IReadOnlyList<string> ExecuteHtml(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        return [$"HTML {action.LineNumber:000} {devToolsClient.GetElementHtml(remoteDebuggingUrl, action.Arguments[0])}"];
    }

    private IReadOnlyList<string> ExecuteScreenshot(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var bytes = devToolsClient.GetElementScreenshot(remoteDebuggingUrl, action.Arguments[0]);
        return WriteScreenshotOutput(action, bytes);
    }

    private IReadOnlyList<string> ExecuteScreenshotPage(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var bytes = devToolsClient.GetPageScreenshot(remoteDebuggingUrl);
        return WriteScreenshotOutput(action, bytes);
    }

    private IReadOnlyList<string> ExecuteAssertText(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        var text = devToolsClient.GetElementText(remoteDebuggingUrl, action.Arguments[0]);
        if (!text.Contains(action.Arguments[1], StringComparison.Ordinal))
        {
            throw new ScriptExecutionException($"Expected text '{action.Arguments[1]}' was not found. Actual text: '{text}'.");
        }

        return [];
    }

    private IReadOnlyList<string> ExecuteEvaluate(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        return [$"EVALUATE {action.LineNumber:000} {devToolsClient.Evaluate(remoteDebuggingUrl, action.Arguments[0])}"];
    }

    private IReadOnlyList<string> ExecuteSetViewport(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var width = GetIntOption(action, "width", required: true);
        var height = GetIntOption(action, "height", required: true);
        devToolsClient.SetViewport(remoteDebuggingUrl, width, height);
        return [];
    }

    private IReadOnlyList<string> ExecuteDragAndDrop(string remoteDebuggingUrl, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        if (action.Children.Count > 0)
        {
            return ExecuteDragAndDropBlock(remoteDebuggingUrl, action, recorder);
        }

        RequireArgumentCount(action, 2, 2);
        if (recorder is not null)
        {
            recorder.RecordDragAndDrop(action.Arguments[0], action.Arguments[1], () =>
            {
                devToolsClient.DragAndDrop(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
            });

            return [];
        }

        devToolsClient.DragAndDrop(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
        return [];
    }

    private IReadOnlyList<string> ExecuteDragAndDropBlock(string remoteDebuggingUrl, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        RequireArgumentCount(action, 1, 1);
        if (action.Options.Count > 0)
        {
            throw new ScriptExecutionException("Block dragAndDrop does not accept options.");
        }

        var sourceSelector = action.Arguments[0];
        BrowserScriptAction? dropAction = null;
        var output = new List<string>();

        foreach (var child in action.Children)
        {
            var childName = child.Name.ToLowerInvariant();
            if (childName is "drop")
            {
                if (dropAction is not null)
                {
                    throw new ScriptExecutionException("Block dragAndDrop can contain only one drop action.");
                }

                RequireArgumentCount(child, 1, 1);
                dropAction = child;
                continue;
            }

            if (dropAction is not null)
            {
                throw new ScriptExecutionException("No actions are allowed after drop inside block dragAndDrop.");
            }

            ValidateDragBlockStep(child);
        }

        if (dropAction is null)
        {
            throw new ScriptExecutionException("Block dragAndDrop requires a drop action.");
        }

        if (recorder is null)
        {
            foreach (var child in action.Children)
            {
                if (string.Equals(child.Name, "drop", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                output.AddRange(ExecuteDragBlockStep(remoteDebuggingUrl, child));
            }

            devToolsClient.DragAndDrop(remoteDebuggingUrl, sourceSelector, dropAction.Arguments[0]);
            return output;
        }

        recorder.BeginDrag(sourceSelector);
        foreach (var child in action.Children)
        {
            var childName = child.Name.ToLowerInvariant();
            if (childName is "drop")
            {
                recorder.DropDrag(child.Arguments[0]);
                devToolsClient.DragAndDrop(remoteDebuggingUrl, sourceSelector, child.Arguments[0]);
                return output;
            }

            output.AddRange(ExecuteDragBlockRecordedStep(remoteDebuggingUrl, child, recorder));
        }

        return output;
    }

    private static void ValidateDragBlockStep(BrowserScriptAction action)
    {
        _ = action.Name.ToLowerInvariant() switch
        {
            "delay" => true,
            "hover" => true,
            "scrollintoview" => true,
            "waitforelement" => true,
            _ => throw new ScriptExecutionException($"Action '{action.Name}' is not supported inside block dragAndDrop.")
        };
    }

    private IReadOnlyList<string> ExecuteDragBlockStep(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "delay" => ExecuteDelay(action),
            "hover" => ExecuteSelectorAction(action, selector => devToolsClient.Hover(remoteDebuggingUrl, selector)),
            "scrollintoview" => ExecuteSelectorAction(action, selector => devToolsClient.ScrollElementIntoView(remoteDebuggingUrl, selector)),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, action),
            _ => throw new ScriptExecutionException($"Action '{action.Name}' is not supported inside block dragAndDrop.")
        };
    }

    private IReadOnlyList<string> ExecuteDragBlockRecordedStep(string remoteDebuggingUrl, BrowserScriptAction action, ScriptGifRecorder recorder)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "delay" => ExecuteRecordedDragDelay(action, recorder),
            "hover" => ExecuteRecordedDragHover(action, recorder),
            "scrollintoview" => ExecuteRecordedDragHover(action, recorder),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, action),
            _ => throw new ScriptExecutionException($"Action '{action.Name}' is not supported inside block dragAndDrop.")
        };
    }

    private static IReadOnlyList<string> ExecuteRecordedDragDelay(BrowserScriptAction action, ScriptGifRecorder recorder)
    {
        RequireArgumentCount(action, 1, 1);
        var milliseconds = ParsePositiveInt(action.Arguments[0], "delay");
        Thread.Sleep(milliseconds);
        recorder.DragDelay(milliseconds);
        return [];
    }

    private static IReadOnlyList<string> ExecuteRecordedDragHover(BrowserScriptAction action, ScriptGifRecorder recorder)
    {
        RequireArgumentCount(action, 1, 1);
        recorder.DragHover(action.Arguments[0]);
        return [];
    }

    private IReadOnlyList<string> ExecuteListTabs(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        return devToolsClient
            .ListTabs(remoteDebuggingUrl)
            .Select((tab, index) => $"TAB {index} id={tab.Id} title=\"{tab.Title}\" url=\"{tab.Url}\"")
            .ToArray();
    }

    private IReadOnlyList<string> ExecuteActivateTab(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        devToolsClient.ActivateTab(remoteDebuggingUrl, GetIntOption(action, "index", required: true));
        return [];
    }

    private IReadOnlyList<string> ExecuteCloseTab(string remoteDebuggingUrl, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        devToolsClient.CloseTab(remoteDebuggingUrl, GetIntOption(action, "index", required: true));
        return [];
    }

    private static IReadOnlyList<string> ExecuteSet(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 2, 2);
        context.Variables[action.Arguments[0]] = action.Arguments[1];
        return [];
    }

    private static IReadOnlyList<string> WriteScreenshotOutput(BrowserScriptAction action, byte[] bytes)
    {
        if (action.Options.TryGetValue("output", out var outputPath) && !string.IsNullOrWhiteSpace(outputPath))
        {
            var fullPath = Path.GetFullPath(outputPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullPath, bytes);
            return [$"SCREENSHOT {action.LineNumber:000} {fullPath}"];
        }

        return [$"SCREENSHOT {action.LineNumber:000} data:image/png;base64,{Convert.ToBase64String(bytes)}"];
    }

    private static BrowserScriptAction ExpandVariables(BrowserScriptAction action, ScriptExecutionContext context)
    {
        return action with
        {
            Arguments = action.Arguments.Select(argument => ExpandVariables(argument, context)).ToArray(),
            Options = action.Options.ToDictionary(
                pair => pair.Key,
                pair => ExpandVariables(pair.Value, context),
                StringComparer.OrdinalIgnoreCase),
            Children = action.Children.Select(child => ExpandVariables(child, context)).ToArray()
        };
    }

    private static string ExpandVariables(string value, ScriptExecutionContext context)
    {
        return VariableRegex().Replace(value, match =>
        {
            var name = match.Groups[1].Value;
            if (!context.Variables.TryGetValue(name, out var replacement))
            {
                throw new ScriptExecutionException($"Variable '{name}' is not defined.");
            }

            return replacement;
        });
    }

    private static ScriptReadResult ReadScript(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return ScriptReadResult.Fail("Script file was not provided.");
        }

        if (file is "-")
        {
            return ScriptReadResult.Ok(Console.In.ReadToEnd());
        }

        if (!File.Exists(file))
        {
            return ScriptReadResult.Fail($"Script file '{file}' was not found.");
        }

        return ScriptReadResult.Ok(File.ReadAllText(file));
    }

    private static string NormalizeNavigationTarget(string target)
    {
        if (File.Exists(target))
        {
            return new Uri(Path.GetFullPath(target)).AbsoluteUri;
        }

        return target;
    }

    private static void RequireArgumentCount(BrowserScriptAction action, int min, int max)
    {
        if (action.Arguments.Count < min || action.Arguments.Count > max)
        {
            var expected = min == max ? min.ToString() : $"{min}-{max}";
            throw new ScriptExecutionException($"Expected {expected} positional argument(s), got {action.Arguments.Count}.");
        }
    }

    private static int GetIntOption(BrowserScriptAction action, string name, int defaultValue)
    {
        return action.Options.TryGetValue(name, out var value)
            ? ParsePositiveInt(value, name)
            : defaultValue;
    }

    private static int GetIntOption(BrowserScriptAction action, string name, bool required)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            if (required)
            {
                throw new ScriptExecutionException($"Missing required option '{name}'.");
            }

            return 0;
        }

        return ParsePositiveInt(value, name);
    }

    private static int ParsePositiveInt(string value, string name)
    {
        if (!int.TryParse(value, out var number) || number < 0)
        {
            throw new ScriptExecutionException($"'{name}' must be a positive whole number.");
        }

        return number;
    }

    private static string FormatActionForLog(BrowserScriptAction action)
    {
        return string.Join(' ', action.Arguments.Select(QuoteForLog));
    }

    private static string QuoteForLog(string value)
    {
        return value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;
    }

    private static void FinishRecording(ScriptGifRecorder? recorder, List<string> output)
    {
        if (recorder is null)
        {
            return;
        }

        recorder.Finish();
        output.Add($"GIF {recorder.OutputPath}");
    }

    [GeneratedRegex(@"\$\{([A-Za-z_][A-Za-z0-9_]*)\}")]
    private static partial Regex VariableRegex();
}

public sealed record ScriptRunResult(bool Success, IReadOnlyList<string> StdoutLines, string? Error)
{
    public static ScriptRunResult Ok(IReadOnlyList<string> stdoutLines) => new(true, stdoutLines, null);

    public static ScriptRunResult Fail(string error, IReadOnlyList<string>? stdoutLines = null) => new(false, stdoutLines ?? [], error);
}

internal sealed class ScriptExecutionContext
{
    public Dictionary<string, string> Variables { get; } = new(StringComparer.Ordinal);
}

internal sealed record ScriptReadResult(bool Success, string? Script, string? Error)
{
    public static ScriptReadResult Ok(string script) => new(true, script, null);

    public static ScriptReadResult Fail(string error) => new(false, null, error);
}

public sealed class ScriptExecutionException : Exception
{
    public ScriptExecutionException(string message)
        : base(message)
    {
    }
}
