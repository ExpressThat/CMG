using System.CommandLine;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private static bool TryParseEncoding(
        ParseResult result,
        GifEncodingCliOptions options,
        out GifEncodingOptions encoding)
    {
        if (options.TryParse(result, out encoding, out var error)) return true;
        Console.Error.WriteLine(error);
        return false;
    }
}
