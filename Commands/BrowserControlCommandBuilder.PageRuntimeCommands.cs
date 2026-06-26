using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildPageRuntimeGroup(BrowserSelectionOptions browserOptions)
    {
        var command = new Command("runtime", "Element getters, selector evaluation, and page setup commands.");

        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "textContent", "Read element textContent."));
        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "innerText", "Read element innerText."));
        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "inputValue", "Read input-like element value."));
        command.Subcommands.Add(BuildGetAttributeCommand(browserOptions));
        command.Subcommands.Add(BuildNamedElementGetterCommand(
            browserOptions,
            "computedStyle",
            "property",
            "CSS property name, such as display or background-color.",
            "Read a computed CSS property."));
        command.Subcommands.Add(BuildNamedElementGetterCommand(
            browserOptions,
            "property",
            "name",
            "Dot-separated JavaScript property path, such as dataset.state.",
            "Read an element JavaScript property."));
        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "count", "Count matching elements."));
        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "locatorCount", "Count matching elements."));
        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "boundingBox", "Read an element bounding box."));
        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "allTextContents", "Read textContent for all matching elements."));
        command.Subcommands.Add(BuildElementGetterCommand(browserOptions, "allInnerTexts", "Read innerText for all matching elements."));
        command.Subcommands.Add(BuildSelectorEvaluateCommand(browserOptions, "evaluateOnSelector", "Evaluate JavaScript with one selected element."));
        command.Subcommands.Add(BuildSelectorEvaluateCommand(browserOptions, "evalOnSelector", "Evaluate JavaScript with one selected element."));
        command.Subcommands.Add(BuildSelectorEvaluateCommand(browserOptions, "evaluateAll", "Evaluate JavaScript with all matching elements."));
        command.Subcommands.Add(BuildSelectorEvaluateCommand(browserOptions, "evalAll", "Evaluate JavaScript with all matching elements."));
        command.Subcommands.Add(BuildInitScriptCommand(browserOptions, "addInitScript", "addInitScript"));
        command.Subcommands.Add(BuildInitScriptCommand(browserOptions, "evaluateOnNewDocument", "evaluateOnNewDocument"));
        command.Subcommands.Add(BuildTagCommand(browserOptions, "addScriptTag", "Inject a script tag."));
        command.Subcommands.Add(BuildTagCommand(browserOptions, "addStyleTag", "Inject a style tag or stylesheet link."));
        command.Subcommands.Add(BuildExposeCommand(browserOptions, "exposeFunction", "Expose a deterministic page-side function."));
        command.Subcommands.Add(BuildExposeCommand(browserOptions, "exposeBinding", "Expose a deterministic page-side binding."));

        return command;
    }

    private Command BuildElementGetterCommand(BrowserSelectionOptions browserOptions, string action, string description)
    {
        var selector = CreateSelectorArgument();
        var command = new Command(action, description) { selector };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, parseResult.GetValue(selector) ?? string.Empty)));
        return command;
    }

    private Command BuildGetAttributeCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var name = new Argument<string>("name") { Description = "Attribute name." };
        var command = new Command("getAttribute", "Read an element attribute.") { selector, name };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("getAttribute", [parseResult.GetValue(selector) ?? string.Empty, parseResult.GetValue(name) ?? string.Empty], [])));
        return command;
    }

    private Command BuildNamedElementGetterCommand(
        BrowserSelectionOptions browserOptions,
        string action,
        string valueName,
        string valueDescription,
        string description)
    {
        var selector = CreateSelectorArgument();
        var value = new Argument<string>(valueName) { Description = valueDescription };
        var command = new Command(action, description) { selector, value };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(selector) ?? string.Empty, parseResult.GetValue(value) ?? string.Empty], [])));
        return command;
    }

    private Command BuildSelectorEvaluateCommand(BrowserSelectionOptions browserOptions, string action, string description)
    {
        var selector = CreateSelectorArgument();
        var expression = new Argument<string>("expression") { Description = "JavaScript expression or function." };
        var command = new Command(action, description) { selector, expression };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(selector) ?? string.Empty, parseResult.GetValue(expression) ?? string.Empty], [])));
        return command;
    }

    private Command BuildInitScriptCommand(BrowserSelectionOptions browserOptions, string name, string action)
    {
        var source = OptionalTextArgument("source", "Inline JavaScript source.");
        var path = new Option<FileInfo?>("--path") { Description = "JavaScript file to register." };
        var command = new Command(name, "Register JavaScript for future documents.") { source, path };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, OptionalArgument(parseResult, source), CompactOptions([
                StringOption("path", parseResult.GetValue(path)?.FullName)
            ]))));
        return command;
    }

    private Command BuildTagCommand(BrowserSelectionOptions browserOptions, string action, string description)
    {
        var content = OptionalTextArgument("content", "Inline tag content.");
        var url = CliStringOption("--url", "URL to load.");
        var path = new Option<FileInfo?>("--path") { Description = "Local file whose content is injected." };
        var contentOption = CliStringOption("--content", "Inline content option.");
        var command = new Command(action, description) { content, url, path, contentOption };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, OptionalArgument(parseResult, content), CompactOptions([
                StringOption("url", parseResult.GetValue(url)),
                StringOption("path", parseResult.GetValue(path)?.FullName),
                StringOption("content", parseResult.GetValue(contentOption))
            ]))));
        return command;
    }

    private Command BuildExposeCommand(BrowserSelectionOptions browserOptions, string action, string description)
    {
        var name = new Argument<string>("name") { Description = "JavaScript identifier to install on window." };
        var expression = new Argument<string>("expression") { Description = "JavaScript function expression." };
        var command = new Command(action, description) { name, expression };
        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine(action, [parseResult.GetValue(name) ?? string.Empty, parseResult.GetValue(expression) ?? string.Empty], [])));
        return command;
    }
}
