# `gif`

GIF artifact inspection and utility commands.

```powershell
cmg gif [command] [arguments] [options]
```

## Subcommands

- [`inspect`](inspect.md): Inspect GIF frame, timing, palette, dimension, and size metadata.
- [`presets`](presets.md): List GIF quality, pointer, pulse, and timing presets.

## Behavior

- Does not require a browser.
- Reads an existing GIF artifact from disk.
- Writes parseable stdout lines for agent callers.
- Writes missing-file or invalid-image errors to stderr.
- Exits `0` on success and `1` on failure.
