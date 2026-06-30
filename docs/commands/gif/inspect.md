# `gif inspect`

Inspect a GIF artifact without launching or controlling a browser.

```powershell
cmg gif inspect <file>
```

## Arguments

- `<file>`: Path to the GIF file to inspect.

## Options

No command-specific options.

## Stdout

On success, writes one parseable line:

```text
GIF_INSPECT path="<absolute-path>" frames=<count> durationMs=<milliseconds> width=<pixels> height=<pixels> sizeBytes=<bytes> palette=<mode> paletteColors=<count-or->256> transparent=<true|false> repeat=<count>
```

Fields:

- `frames`: Number of GIF frames.
- `durationMs`: Sum of frame delays in milliseconds.
- `width` / `height`: Decoded GIF dimensions.
- `sizeBytes`: File size on disk.
- `palette`: GIF color table mode reported by frame metadata, or `mixed` when frames differ.
- `paletteColors`: Unique decoded colors found across frames, capped at `>256` to flag palette pressure.
- `transparent`: Whether any frame declares GIF transparency.
- `repeat`: GIF repeat count metadata.

## Stderr

- Missing files write `GIF file '<path>' was not found.`
- Invalid or unreadable image files write `Could not inspect GIF '<path>'. <reason>`

## Exit Codes

- `0`: GIF inspected successfully.
- `1`: File was missing, unreadable, or not a valid supported image.

## Examples

```powershell
cmg gif inspect demo-output\dialog-flow.gif
cmg gif inspect artifacts\gifs\chrome-checkout.gif
```
