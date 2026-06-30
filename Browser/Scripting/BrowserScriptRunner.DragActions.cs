using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteDragAndDrop(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        if (action.Children.Count > 0)
        {
            return ExecuteDragAndDropBlock(remoteDebuggingUrl, automationClient, action, recorder);
        }

        RequireArgumentCount(action, 2, 2);
        if (HasSimpleDragOptions(action))
        {
            ExecuteOptionedSimpleDrag(remoteDebuggingUrl, automationClient, action, recorder);
            return [];
        }

        if (recorder is not null)
        {
            recorder.RecordDragAndDrop(action);

            return [];
        }

        automationClient.DragAndDrop(remoteDebuggingUrl, action.Arguments[0], action.Arguments[1]);
        return [];
    }

    private static bool HasSimpleDragOptions(BrowserScriptAction action) =>
        action.Options.Keys.Any(key => key is "sourceX" or "sourceY" or "targetX" or "targetY");

    private static void ExecuteOptionedSimpleDrag(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptGifRecorder? recorder)
    {
        var source = ResolveSelector(remoteDebuggingUrl, automationClient, action, 0);
        var target = ResolveSelector(remoteDebuggingUrl, automationClient, action, 1);
        var start = ResolveDragPoint(remoteDebuggingUrl, automationClient, action, source, "source");
        var end = ResolveDragPoint(remoteDebuggingUrl, automationClient, action, target, "target");
        if (recorder is not null)
        {
            recorder.RecordDragAndDrop(action with { Arguments = [source, target] }, start, end);
            return;
        }

        automationClient.BeginPageDrag(remoteDebuggingUrl, source, start);
        automationClient.MovePageDrag(remoteDebuggingUrl, end);
        automationClient.EndPageDrag(remoteDebuggingUrl, end);
    }

    private static ElementPoint ResolveDragPoint(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string selector,
        string prefix)
    {
        var box = automationClient.GetElementBox(remoteDebuggingUrl, selector);
        var x = DragOffset(action, $"{prefix}X", box.Width / 2);
        var y = DragOffset(action, $"{prefix}Y", box.Height / 2);
        return new ElementPoint(box.X + x, box.Y + y);
    }

    private static double DragOffset(BrowserScriptAction action, string option, double fallback) =>
        action.Options.TryGetValue(option, out var value)
            ? ParseDragOffset(value, $"{action.Name} option {option}=")
            : fallback;

    private static double ParseDragOffset(string value, string optionName) =>
        double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) && number >= 0
            ? number
            : throw new ScriptExecutionException($"{optionName} must be zero or greater.");

    private static IReadOnlyList<string> ExecuteDragAndDropBlock(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, ScriptGifRecorder? recorder)
    {
        RequireArgumentCount(action, 1, 1);
        if (action.Options.Keys.Any(key => !IsDragRecordingOption(key)))
        {
            throw new ScriptExecutionException("Block dragAndDrop accepts only recording choreography options.");
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
                var childName = child.Name.ToLowerInvariant();
                if (childName is "drop")
                {
                    break;
                }

                output.AddRange(ExecuteUnrecordedDragBlockStep(remoteDebuggingUrl, automationClient, child));
            }

            automationClient.DragAndDrop(remoteDebuggingUrl, sourceSelector, dropAction.Arguments[0]);
            return output;
        }

        recorder.BeginDrag(action);
        foreach (var child in action.Children)
        {
            var childName = child.Name.ToLowerInvariant();
            if (childName is "drop")
            {
                recorder.DropDrag(MergeDragChildOptions(action, child));
                return output;
            }

            output.AddRange(ExecuteDragBlockRecordedStep(remoteDebuggingUrl, automationClient, MergeDragChildOptions(action, child), recorder));
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
            "pausegif" => true,
            "recordcheckpoint" => true,
            "scrollintoview" => true,
            "waitforelement" => true,
            _ => throw new ScriptExecutionException($"Action '{action.Name}' is not supported inside block dragAndDrop.")
        };
    }

    private static IReadOnlyList<string> ExecuteUnrecordedDragBlockStep(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "delay" => [$"GIF_DRAG_DELAY {action.LineNumber:000} status=skipped reason=no-active-recording"],
            "hover" => [$"GIF_DRAG_HOVER {action.LineNumber:000} status=skipped reason=no-active-recording"],
            "movemouse" => ExecuteMoveMouse(action, recorder: null, dragging: true),
            "pausegif" => ExecutePauseGif(action, recorder: null),
            "recordcheckpoint" => ExecuteRecordCheckpoint(action, recorder: null),
            "scrollintoview" => ExecuteSelectorAction(remoteDebuggingUrl, automationClient, action, selector => automationClient.ScrollElementIntoView(remoteDebuggingUrl, selector)),
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
            "pausegif" => ExecutePauseGif(action, recorder),
            "recordcheckpoint" => ExecuteRecordCheckpoint(action, recorder),
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
        recorder.DragHover(action);
        return [];
    }

    private static bool IsDragRecordingOption(string key) =>
        key is "pointerDuration" or "pointerSpeed" or "pointerEasing" or
            "pointerPath" or "dragPath" or
            "sourcePointerDuration" or "targetPointerDuration" or "dragEasing" or
            "preDragHold" or "dragHold" or "postDropHold" or
            "quality" or "clickPulse" or "pulse" or "holdAfterAction" or "holdOnFailure" or "holdAfterMove" or
            "preClickHold" or "postClickHold" or "holdAfterNavigation" or "holdAfterAssertion" or
            "fps" or "frameDelay" or "timeline";

    private static BrowserScriptAction MergeDragChildOptions(BrowserScriptAction parent, BrowserScriptAction child)
    {
        if (parent.Options.Count is 0)
        {
            return child;
        }

        var options = new Dictionary<string, string>(parent.Options, StringComparer.OrdinalIgnoreCase);
        foreach (var option in child.Options)
        {
            options[option.Key] = option.Value;
        }

        return child with { Options = options };
    }
}
