using System.CommandLine;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildScreenshotCommand(BrowserSelectionOptions browserOptions)
    {
        var selector = CreateSelectorArgument();
        var output = new Option<FileInfo?>("--output") { Description = "Write the screenshot to this file instead of stdout data URL." };
        var type = CliStringOption("--type", "Screenshot type: png, jpeg, or jpg. Default is png.");
        var quality = new Option<int?>("--quality") { Description = "JPEG quality from 0 to 100." };
        var omitBackground = new Option<bool>("--omit-background") { Description = "Allow transparent background when supported." };
        var style = CliStringOption("--style", "Temporary CSS applied only during screenshot capture.");
        var stylePath = new Option<FileInfo?>("--style-path") { Description = "CSS file applied only during screenshot capture." };
        var mask = CliStringOption("--mask", "Semicolon-separated selectors to cover during screenshot capture.");
        var maskColor = CliStringOption("--mask-color", "CSS color used for screenshot masks. Default is #ff00ff.");
        var animations = CliStringOption("--animations", "Animation handling for screenshot artifacts: disabled or allow.");
        var caret = CliStringOption("--caret", "Caret handling for screenshot artifacts: hide or initial.");
        var command = new Command("screenshot", "Capture an element screenshot.")
            { selector, output, type, quality, omitBackground, style, stylePath, mask, maskColor, animations, caret };

        command.SetAction(parseResult =>
        {
            var options = ScreenshotCliOptions(
                parseResult,
                output,
                type,
                quality,
                omitBackground,
                style,
                stylePath,
                mask,
                maskColor,
                animations,
                caret);
            return browserControlCommandHandler.RunScriptAction(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                ToScriptLine("screenshot", [parseResult.GetValue(selector) ?? string.Empty], options));
        });

        return command;
    }

    private Command BuildScreenshotPageCommand(BrowserSelectionOptions browserOptions)
    {
        var output = new Option<FileInfo?>("--output") { Description = "Write the screenshot to this file instead of stdout data URL." };
        var fullPage = new Option<bool>("--full-page") { Description = "Capture the full scrollable page instead of only the current viewport." };
        var type = CliStringOption("--type", "Screenshot type: png, jpeg, or jpg. Default is png.");
        var quality = new Option<int?>("--quality") { Description = "JPEG quality from 0 to 100." };
        var omitBackground = new Option<bool>("--omit-background") { Description = "Allow transparent background when supported." };
        var style = CliStringOption("--style", "Temporary CSS applied only during screenshot capture.");
        var stylePath = new Option<FileInfo?>("--style-path") { Description = "CSS file applied only during screenshot capture." };
        var mask = CliStringOption("--mask", "Semicolon-separated selectors to cover during screenshot capture.");
        var maskColor = CliStringOption("--mask-color", "CSS color used for screenshot masks. Default is #ff00ff.");
        var animations = CliStringOption("--animations", "Animation handling for screenshot artifacts: disabled or allow.");
        var caret = CliStringOption("--caret", "Caret handling for screenshot artifacts: hide or initial.");
        var clipX = new Option<double?>("--clip-x") { Description = "Viewport/document clip X coordinate in CSS pixels." };
        var clipY = new Option<double?>("--clip-y") { Description = "Viewport/document clip Y coordinate in CSS pixels." };
        var clipWidth = new Option<double?>("--clip-width") { Description = "Clip width in CSS pixels." };
        var clipHeight = new Option<double?>("--clip-height") { Description = "Clip height in CSS pixels." };
        var command = new Command("screenshotPage", "Capture a page screenshot.")
            { output, fullPage, type, quality, omitBackground, style, stylePath, mask, maskColor, animations, caret, clipX, clipY, clipWidth, clipHeight };

        command.SetAction(parseResult =>
        {
            var options = ScreenshotCliOptions(
                parseResult,
                output,
                type,
                quality,
                omitBackground,
                style,
                stylePath,
                mask,
                maskColor,
                animations,
                caret,
                clipX,
                clipY,
                clipWidth,
                clipHeight).ToList();
            if (parseResult.GetValue(fullPage))
            {
                options.Add(("fullPage", "true"));
            }

            return browserControlCommandHandler.RunScriptAction(
                CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                ToScriptLine("screenshotPage", [], options));
        });

        return command;
    }
}
