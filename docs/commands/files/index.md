# `files`

Local file utility commands.

```powershell
cmg files [command] [options]
```

## Subcommands

- [`read`](read.md): Read a local file.
- [`write`](write.md): Write text to a local file.
- [`append`](append.md): Append text to a local file.
- [`expect`](expect.md): Assert that a local file exists and optionally contains text.

## Behavior

- Does not require a browser.
- Uses parseable file output lines for agent callers.
- Writes file assertion or missing-file errors to stderr.
- Exits `0` on success and `1` on failure.
