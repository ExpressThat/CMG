# `gif color-diff`

Measure RGB color drift between a source PNG captured before encoding and one decoded GIF frame. This command does not launch or require a browser.

```powershell
cmg gif color-diff <source> <gif> [--frame <number>]
```

## Arguments

- `<source>`: Required source PNG path, normally a `frame-NNNN.png` written by DSL `keepFrames=`.
- `<gif>`: Required encoded GIF path.

## Options

- `--frame <number>`: One-based GIF frame to compare. Defaults to `1`.

The source and selected GIF frame must have identical dimensions.

## Stdout

Success writes one parseable line:

```text
GIF_COLOR_DIFF source="<absolute-path>" gif="<absolute-path>" frame=<number> width=<pixels> height=<pixels> meanAbsoluteError=<decimal> rootMeanSquareError=<decimal> maximumChannelError=<0..255> changedPixels=<count> transparencyChangedPixels=<count> pixelCount=<count> changedPercent=<decimal>
```

Decimal values always use `.` and four fractional digits. RGB errors are measured after compositing both inputs over white, preventing invisible RGB beneath transparent pixels from being counted as visible drift. `transparencyChangedPixels` independently reports pixels whose alpha differs.

## Stderr

- A missing input writes `<source PNG|GIF> file '<path>' was not found.`
- An invalid frame, unreadable image, or dimension mismatch writes `Could not compare GIF color fidelity. <reason>`

## Exit Codes

- `0`: Inputs were decoded and compared.
- `1`: An input was missing or invalid, the frame did not exist, or dimensions differed.

## Examples

```powershell
cmg gif color-diff demo-output\quality.frames\frame-0001.png demo-output\quality.gif
cmg gif color-diff demo-output\quality.frames\frame-0012.png demo-output\quality.gif --frame 12
```
