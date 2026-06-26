using System.CommandLine;
using CMG.Browser;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildEmulateMediaCommand(BrowserSelectionOptions browserOptions)
    {
        var media = CliStringOption("--media", "CSS media type: screen or print.");
        var color = CliStringOption("--color-scheme", "Preferred color scheme: light, dark, or no-preference.");
        var motion = CliStringOption("--reduced-motion", "Reduced motion value: reduce or no-preference.");
        var forced = CliStringOption("--forced-colors", "Forced colors value: active or none.");
        var contrast = CliStringOption("--contrast", "Preferred contrast: more, less, custom, or no-preference.");
        var command = new Command("emulateMedia", "Apply page media emulation.")
        {
            media,
            color,
            motion,
            forced,
            contrast
        };

        command.SetAction(parseResult => browserControlCommandHandler.RunScriptAction(
            CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
            ToScriptLine("emulateMedia", [], CompactOptions([
                StringOption("media", parseResult.GetValue(media)),
                StringOption("colorScheme", parseResult.GetValue(color)),
                StringOption("reducedMotion", parseResult.GetValue(motion)),
                StringOption("forcedColors", parseResult.GetValue(forced)),
                StringOption("contrast", parseResult.GetValue(contrast))
            ]))));

        return command;
    }
}
