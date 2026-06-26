# `browser control events`

Downloads, dialogs, console, page-error, and generic event wait commands.

```powershell
cmg browser control events [command] [options]
```

## Subcommands

- [`download`](download.md): Click an element and wait for a matching download.
- [`waitForDownload`](waitForDownload.md): Wait for a matching downloaded file.
- [`console`](console/index.md): Console capture and wait commands.
- [`dialogs`](dialogs/index.md): Browser dialog capture, behavior, and wait commands.
- [`pageErrors`](pageErrors/index.md): Page error capture and wait commands.
- [`wait`](wait.md): Wait for any supported browser event.
- [`waitForEvent`](waitForEvent.md): Wait for any supported browser event.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS` and event output lines to stdout.
- Writes browser, argument, timeout, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.

## Examples

```powershell
cmg browser control events dialogs behavior accept --prompt-text "CMG"
cmg browser control events console wait "ready" --level log --timeout 5000
cmg browser control events wait response "/api/profile" --status 200
```
