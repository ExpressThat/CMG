# `gif`

GIF artifact inspection and utility commands.

```powershell
cmg gif [command] [arguments] [options]
```

## Subcommands

- [`inspect`](inspect.md): Inspect GIF frame, timing, palette, dimension, and size metadata.

## Behavior

- Does not require a browser.
- Reads an existing GIF artifact from disk.
- Writes one parseable `GIF_INSPECT` line to stdout on success.
- Writes missing-file or invalid-image errors to stderr.
- Exits `0` on success and `1` on failure.
