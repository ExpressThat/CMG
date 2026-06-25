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
        if (recorder is not null)
        {
            recorder.RecordDragAndDrop(action.Arguments[0], action.Arguments[1]);

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
}
