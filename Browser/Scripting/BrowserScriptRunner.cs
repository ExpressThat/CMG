using System.Text.RegularExpressions;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private readonly BrowserScriptParser parser;

    public BrowserScriptRunner(BrowserScriptParser parser)
    {
        this.parser = parser;
    }

    public ScriptRunResult Run(string file, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif)
    {
        var readResult = ReadScript(file);
        if (!readResult.Success)
        {
            return ScriptRunResult.Fail(readResult.Error ?? "Could not read script.");
        }

        return RunParsedScript(readResult.Script ?? string.Empty, remoteDebuggingUrl, automationClient, gif);
    }

    public ScriptRunResult RunText(string script, string remoteDebuggingUrl, IBrowserAutomationClient automationClient)
    {
        return RunParsedScript(script, remoteDebuggingUrl, automationClient, gif: null);
    }

    private ScriptRunResult RunParsedScript(string script, string remoteDebuggingUrl, IBrowserAutomationClient automationClient, FileInfo? gif)
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
            : new ScriptGifRecorder(automationClient, new ScriptRecordingOptions(gif.FullName));

        recorder?.Start(remoteDebuggingUrl);

        for (var index = 0; index < parseResult.Actions.Count; index++)
        {
            var stepNumber = index + 1;
            var action = ExpandVariables(parseResult.Actions[index], context);

            try
            {
                recorder?.BeforeAction(action);
                var stepOutput = ExecuteAction(remoteDebuggingUrl, automationClient, action, context, recorder);
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
        IBrowserAutomationClient automationClient,
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
            "navigate" => ExecuteNavigate(remoteDebuggingUrl, automationClient, action),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action),
            "click" => ExecuteSelectorAction(action, selector => automationClient.Click(remoteDebuggingUrl, selector)),
            "type" => ExecuteType(remoteDebuggingUrl, automationClient, action, recorder),
            "clear" => ExecuteSelectorAction(action, selector => automationClient.Clear(remoteDebuggingUrl, selector)),
            "press" => ExecutePress(remoteDebuggingUrl, automationClient, action),
            "hover" => ExecuteSelectorAction(action, selector => automationClient.Hover(remoteDebuggingUrl, selector)),
            "scrollintoview" => ExecuteSelectorAction(action, selector => automationClient.ScrollElementIntoView(remoteDebuggingUrl, selector)),
            "select" => ExecuteSelect(remoteDebuggingUrl, automationClient, action),
            "showmessagebar" => ExecuteShowMessageBar(remoteDebuggingUrl, automationClient, action),
            "delay" => ExecuteDelay(action),
            "html" => ExecuteHtml(remoteDebuggingUrl, automationClient, action),
            "screenshot" => ExecuteScreenshot(remoteDebuggingUrl, automationClient, action),
            "screenshotpage" => ExecuteScreenshotPage(remoteDebuggingUrl, automationClient, action),
            "asserttext" => ExecuteAssertText(remoteDebuggingUrl, automationClient, action),
            "evaluate" => ExecuteEvaluate(remoteDebuggingUrl, automationClient, action),
            "setviewport" => ExecuteSetViewport(remoteDebuggingUrl, automationClient, action),
            "movemouse" => ExecuteMoveMouse(action, recorder, dragging: false),
            "draganddrop" => ExecuteDragAndDrop(remoteDebuggingUrl, automationClient, action, recorder),
            "listtabs" => ExecuteListTabs(remoteDebuggingUrl, automationClient, action),
            "activatetab" => ExecuteActivateTab(remoteDebuggingUrl, automationClient, action),
            "closetab" => ExecuteCloseTab(remoteDebuggingUrl, automationClient, action),
            "set" => ExecuteSet(action, context),
            _ => throw new ScriptExecutionException($"Unknown action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> ExecuteNavigate(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var finalUrl = automationClient.Navigate(remoteDebuggingUrl, NormalizeNavigationTarget(action.Arguments[0]));
        return string.IsNullOrWhiteSpace(finalUrl) ? [] : [$"NAVIGATED {action.LineNumber:000} {finalUrl}"];
    }

    private static IReadOnlyList<string> ExecuteWaitForElement(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        automationClient.WaitForElement(remoteDebuggingUrl, action.Arguments[0], timeout);
        return [];
    }

    private static IReadOnlyList<string> ExecuteSelectorAction(BrowserScriptAction action, Action<string> execute)
    {
        RequireArgumentCount(action, 1, 1);
        execute(action.Arguments[0]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteType(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        RequireArgumentCount(action, 2, 2);
        if (recorder is null)
        {
            automationClient.Type(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
            return [];
        }

        recorder.CaptureClickPulse();
        automationClient.TypeProgressively(
            remoteDebuggingUrl,
            action.Arguments[0],
            action.Arguments[1],
            recorder.CaptureTypingFrame);

        return [];
    }

    private static IReadOnlyList<string> ExecutePress(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.Press(remoteDebuggingUrl, action.Arguments[0]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteSelect(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        automationClient.Select(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteShowMessageBar(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        automationClient.ShowMessageBar(remoteDebuggingUrl, action.Arguments[0]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteDelay(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        Thread.Sleep(ParsePositiveInt(action.Arguments[0], "delay"));
        return [];
    }

    private static IReadOnlyList<string> ExecuteHtml(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        return [$"HTML {action.LineNumber:000} {automationClient.GetElementHtml(remoteDebuggingUrl, action.Arguments[0])}"];
    }

    private static IReadOnlyList<string> ExecuteScreenshot(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var bytes = automationClient.GetElementScreenshot(remoteDebuggingUrl, action.Arguments[0]);
        return WriteScreenshotOutput(action, bytes);
    }

    private static IReadOnlyList<string> ExecuteScreenshotPage(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var bytes = automationClient.GetPageScreenshot(remoteDebuggingUrl);
        return WriteScreenshotOutput(action, bytes);
    }

    private static IReadOnlyList<string> ExecuteAssertText(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        var text = automationClient.GetElementText(remoteDebuggingUrl, action.Arguments[0]);
        if (!text.Contains(action.Arguments[1], StringComparison.Ordinal))
        {
            throw new ScriptExecutionException($"Expected text '{action.Arguments[1]}' was not found. Actual text: '{text}'.");
        }

        return [];
    }

    private static IReadOnlyList<string> ExecuteEvaluate(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        return [$"EVALUATE {action.LineNumber:000} {automationClient.Evaluate(remoteDebuggingUrl, action.Arguments[0])}"];
    }

    private static IReadOnlyList<string> ExecuteSetViewport(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var width = GetIntOption(action, "width", required: true);
        var height = GetIntOption(action, "height", required: true);
        automationClient.SetViewport(remoteDebuggingUrl, width, height);
        return [];
    }

    private static IReadOnlyList<string> ExecuteMoveMouse(BrowserScriptAction action, ScriptGifRecorder? recorder, bool dragging)
    {
        if (recorder is null)
        {
            throw new ScriptExecutionException("moveMouse requires script GIF recording. Run the script with --gif <path>.");
        }

        if (action.Children.Count > 0)
        {
            throw new ScriptExecutionException("moveMouse does not accept a block body.");
        }

        recorder.MoveMouse(action, dragging);
        return [];
    }

    private static IReadOnlyList<string> ExecuteDragAndDrop(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        if (action.Children.Count > 0)
        {
            return ExecuteDragAndDropBlock(remoteDebuggingUrl, automationClient, action, recorder);
        }

        RequireArgumentCount(action, 2, 2);
        if (recorder is not null)
        {
            recorder.RecordDragAndDrop(action.Arguments[0], action.Arguments[1], () =>
            {
                automationClient.DragAndDrop(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
            });

            return [];
        }

        automationClient.DragAndDrop(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
        return [];
    }

    private static IReadOnlyList<string> ExecuteDragAndDropBlock(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder? recorder)
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

                output.AddRange(ExecuteDragBlockStep(remoteDebuggingUrl, automationClient, child));
            }

            automationClient.DragAndDrop(remoteDebuggingUrl, sourceSelector, dropAction.Arguments[0]);
            return output;
        }

        recorder.BeginDrag(sourceSelector);
        foreach (var child in action.Children)
        {
            var childName = child.Name.ToLowerInvariant();
            if (childName is "drop")
            {
                recorder.DropDrag(child.Arguments[0]);
                return output;
            }

            output.AddRange(ExecuteDragBlockRecordedStep(remoteDebuggingUrl, automationClient, child, recorder));
        }

        return output;
    }

    private static void ValidateDragBlockStep(BrowserScriptAction action)
    {
        _ = action.Name.ToLowerInvariant() switch
        {
            "delay" => true,
            "hover" => true,
            "movemouse" => true,
            "scrollintoview" => true,
            "waitforelement" => true,
            _ => throw new ScriptExecutionException($"Action '{action.Name}' is not supported inside block dragAndDrop.")
        };
    }

    private static IReadOnlyList<string> ExecuteDragBlockStep(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "delay" => ExecuteDelay(action),
            "hover" => ExecuteSelectorAction(action, selector => automationClient.Hover(remoteDebuggingUrl, selector)),
            "movemouse" => ExecuteMoveMouse(action, recorder: null, dragging: true),
            "scrollintoview" => ExecuteSelectorAction(action, selector => automationClient.ScrollElementIntoView(remoteDebuggingUrl, selector)),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Action '{action.Name}' is not supported inside block dragAndDrop.")
        };
    }

    private static IReadOnlyList<string> ExecuteDragBlockRecordedStep(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder recorder)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "delay" => ExecuteRecordedDragDelay(action, recorder),
            "hover" => ExecuteRecordedDragHover(action, recorder),
            "movemouse" => ExecuteMoveMouse(action, recorder, dragging: true),
            "scrollintoview" => ExecuteRecordedDragHover(action, recorder),
            "waitforelement" => ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action),
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

    private static IReadOnlyList<string> ExecuteListTabs(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        return automationClient
            .ListTabs(remoteDebuggingUrl)
            .Select((tab, index) => $"TAB {index} id={tab.Id} title=\"{tab.Title}\" url=\"{tab.Url}\"")
            .ToArray();
    }

    private static IReadOnlyList<string> ExecuteActivateTab(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.ActivateTab(remoteDebuggingUrl, GetIntOption(action, "index", required: true));
        return [];
    }

    private static IReadOnlyList<string> ExecuteCloseTab(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.CloseTab(remoteDebuggingUrl, GetIntOption(action, "index", required: true));
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

        if (LooksLikeLocalPath(target))
        {
            throw new ScriptExecutionException($"Navigation target path '{target}' was not found.");
        }

        return target;
    }

    private static bool LooksLikeLocalPath(string target) =>
        !target.Contains("://", StringComparison.Ordinal) &&
        (Path.IsPathRooted(target) ||
        target.StartsWith(".", StringComparison.Ordinal) ||
        target.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
        target.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal));

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
