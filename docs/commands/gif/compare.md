# `gif compare`

Compare two GIF artifacts without launching or controlling a browser.

```powershell
cmg gif compare <before> <after>
```

## Arguments

- `<before>`: Baseline GIF file.
- `<after>`: GIF file to compare against the baseline.

## Options

No command-specific options.

## Stdout

On success, writes one parseable line:

```text
GIF_COMPARE before="<absolute-path>" after="<absolute-path>" framesDelta=<count> durationMsDelta=<milliseconds> sizeBytesDelta=<bytes> widthDelta=<pixels> heightDelta=<pixels> sameDimensions=<true|false> paletteBefore=<mode> paletteAfter=<mode> paletteColorsBefore=<count-or->256> paletteColorsAfter=<count-or->256> transparentBefore=<true|false> transparentAfter=<true|false> repeatBefore=<count> repeatAfter=<count>
```

Use positive deltas to identify growth in the `after` artifact and negative deltas to identify reductions.

## Stderr

- Missing files write `GIF before file '<path>' was not found.` or `GIF after file '<path>' was not found.`
- Invalid or unreadable image files write `Could not inspect GIF <before|after> file '<path>'. <reason>`

## Exit Codes

- `0`: Both GIFs were inspected and compared.
- `1`: Either file was missing, unreadable, or not a valid supported GIF.

## Examples

```powershell
cmg gif compare demo-output\quality-highest.gif demo-output\quality-medium.gif
cmg gif compare artifacts\gifs\before.gif artifacts\gifs\after.gif
```
