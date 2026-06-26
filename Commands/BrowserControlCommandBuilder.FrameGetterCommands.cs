using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private void AddFrameGetterCommands(Command command, BrowserSelectionOptions browserOptions)
    {
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "textContent", "frameTextContent", "Read iframe element textContent."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameTextContent", "frameTextContent", "Read iframe element textContent."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "innerText", "frameInnerText", "Read iframe element innerText."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameInnerText", "frameInnerText", "Read iframe element innerText."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "inputValue", "frameInputValue", "Read iframe input-like element value."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameInputValue", "frameInputValue", "Read iframe input-like element value."));
        command.Subcommands.Add(BuildFrameNamedGetterCommand(browserOptions, "getAttribute", "frameGetAttribute", "name", "Attribute name.", "Read iframe element attribute."));
        command.Subcommands.Add(BuildFrameNamedGetterCommand(browserOptions, "frameGetAttribute", "frameGetAttribute", "name", "Attribute name.", "Read iframe element attribute."));
        command.Subcommands.Add(BuildFrameNamedGetterCommand(browserOptions, "computedStyle", "frameComputedStyle", "property", "CSS property name.", "Read iframe element computed CSS property."));
        command.Subcommands.Add(BuildFrameNamedGetterCommand(browserOptions, "frameComputedStyle", "frameComputedStyle", "property", "CSS property name.", "Read iframe element computed CSS property."));
        command.Subcommands.Add(BuildFrameNamedGetterCommand(browserOptions, "property", "frameProperty", "name", "Dot-separated JavaScript property path.", "Read iframe element JavaScript property."));
        command.Subcommands.Add(BuildFrameNamedGetterCommand(browserOptions, "frameProperty", "frameProperty", "name", "Dot-separated JavaScript property path.", "Read iframe element JavaScript property."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "count", "frameCount", "Count iframe elements."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameCount", "frameCount", "Count iframe elements."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "locatorCount", "frameLocatorCount", "Count iframe elements."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameLocatorCount", "frameLocatorCount", "Count iframe elements."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "boundingBox", "frameBoundingBox", "Read iframe element bounding box."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameBoundingBox", "frameBoundingBox", "Read iframe element bounding box."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "allTextContents", "frameAllTextContents", "Read iframe textContent values."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameAllTextContents", "frameAllTextContents", "Read iframe textContent values."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "allInnerTexts", "frameAllInnerTexts", "Read iframe innerText values."));
        command.Subcommands.Add(BuildFrameGetterCommand(browserOptions, "frameAllInnerTexts", "frameAllInnerTexts", "Read iframe innerText values."));
    }

    private Command BuildFrameGetterCommand(BrowserSelectionOptions browserOptions, string name, string action, string description)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var command = new Command(name, description) { frame, selector };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(frame) ?? string.Empty, parseResult.GetValue(selector) ?? string.Empty], [])));
        return command;
    }

    private Command BuildFrameNamedGetterCommand(
        BrowserSelectionOptions browserOptions,
        string name,
        string action,
        string valueName,
        string valueDescription,
        string description)
    {
        var frame = FrameArgument();
        var selector = CreateSelectorArgument();
        var value = new Argument<string>(valueName) { Description = valueDescription };
        var command = new Command(name, description) { frame, selector, value };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [
                parseResult.GetValue(frame) ?? string.Empty,
                parseResult.GetValue(selector) ?? string.Empty,
                parseResult.GetValue(value) ?? string.Empty
            ], [])));
        return command;
    }
}
