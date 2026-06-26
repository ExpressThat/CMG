# `browser control accessibility`

Accessibility snapshot and assertion commands.

```powershell
cmg browser control accessibility [command] [options]
```

## Subcommands

- [`snapshot`](snapshot.md): Create an accessibility snapshot.
- [`accessibilitySnapshot`](accessibilitySnapshot.md): Create an accessibility snapshot.
- [`expect`](expect.md): Assert that an accessible element exists.
- [`expectAccessible`](expectAccessible.md): Assert that an accessible element exists.

## Behavior

- Requires a browser started with [`browser launch`](../../launch.md).
- Runs the same underlying scripting actions as `browser control script`.
- Writes `PASS`, `ACCESSIBILITY`, or `ACCESSIBLE` lines to stdout.
- Writes browser, selector, accessibility, parse, or action errors to stderr.
- Exits `0` on success and `1` on failure.
