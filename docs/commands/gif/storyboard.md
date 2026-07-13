# `gif storyboard`

Export an existing GIF artifact to a PNG contact sheet without launching or controlling a browser.

Frames are alpha-composited onto a white review background, so transparent pixels do not appear as black or transparent evidence gaps.

```powershell
cmg gif storyboard <file> --output <png> [--columns <count>] [--max-frames <count>]
```

## Arguments

- `<file>`: GIF file to export.

## Options

- `--output <png>`: Required PNG output path for the storyboard contact sheet.
- `--columns <count>`: Optional number of frame columns. Default is `4`. Must be at least `1`.
- `--max-frames <count>`: Optional maximum number of frames to sample evenly from the GIF. Must be at least `1`.

## Stdout

On success, writes one parseable line:

```text
GIF_STORYBOARD input="<absolute-gif-path>" output="<png-path>" frames=<exported>/<total> columns=<count> width=<pixels> height=<pixels>
```

Fields:

- `frames`: Exported frame count followed by total GIF frame count.
- `columns`: Contact sheet column count.
- `width` / `height`: Final PNG dimensions.

## Stderr

- Missing files write `GIF file '<path>' was not found.`
- Missing output writes `gif storyboard requires --output <png>.`
- Invalid images or options write `Could not export GIF storyboard '<path>'. <reason>`

## Exit Codes

- `0`: Storyboard PNG was written.
- `1`: File was missing, output was missing, options were invalid, or the input was not a valid GIF.

## Examples

```powershell
cmg gif storyboard demo-output\dialog-flow.gif --output demo-output\dialog-flow-storyboard.png
cmg gif storyboard artifacts\gifs\checkout.gif --output artifacts\checkout-storyboard.png --columns 5 --max-frames 20
```
