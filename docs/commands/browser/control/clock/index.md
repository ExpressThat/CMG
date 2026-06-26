# `browser control clock`

Deterministic page-side time commands.

```powershell
cmg browser control clock [command] [options]
```

## Subcommands

- [`install`](install.md): Install deterministic page-side time control.
- [`tick`](tick.md): Advance deterministic page-side time.
- [`restore`](restore.md): Restore native page clock APIs.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Patches `Date`, `Date.now`, timers, and intervals in the current page context.
- Writes `PASS`, `CLOCK`, `TICK`, or `CLOCK_RESTORED` lines to stdout.
- Writes browser, argument, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.
