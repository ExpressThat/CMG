# `gif`

GIF artifact inspection and utility commands.

```powershell
cmg gif [command] [arguments] [options]
```

## Subcommands

- [`inspect`](inspect.md): Inspect GIF frame, timing, palette, dimension, and size metadata.
- [`compare`](compare.md): Compare two GIF artifacts by metadata.
- [`color-diff`](color-diff.md): Measure color drift between a pre-encoding PNG and a GIF frame.
- [`storyboard`](storyboard.md): Export GIF frames to a PNG contact sheet.
- [`optimize`](optimize.md): Coalesce consecutive duplicate frames into a smaller GIF.
- [`presets`](presets.md): List GIF quality, pointer, pulse, and timing presets.

## Behavior

- Does not require a browser.
- Reads an existing GIF artifact from disk.
- Writes parseable stdout lines for agent callers.
- Writes missing-file or invalid-image errors to stderr.
- Exits `0` on success and `1` on failure.
