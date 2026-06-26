# `browser control coverage`

JavaScript and CSS coverage commands.

```powershell
cmg browser control coverage [command] [options]
```

## Subcommands

- [`start`](start.md): Start JavaScript and CSS coverage collection.
- [`stop`](stop.md): Stop coverage collection and print or write JSON.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS`, `COVERAGE_STARTED`, and `COVERAGE` lines to stdout.
- Writes browser, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.
