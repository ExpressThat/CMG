# `gif optimize`

Optimize an existing GIF artifact without launching or controlling a browser.

The current optimizer coalesces consecutive duplicate decoded frames and preserves their timing by adding duplicate frame delays to the previous kept frame.

```powershell
cmg gif optimize <file> --output <gif>
```

## Arguments

- `<file>`: GIF file to optimize.

## Options

- `--output <gif>`: Required output path for the optimized GIF.

## Stdout

On success, writes one parseable line:

```text
GIF_OPTIMIZE input="<absolute-gif-path>" output="<gif-path>" framesBefore=<count> framesAfter=<count> duplicateFramesRemoved=<count> durationMs=<milliseconds> sizeBeforeBytes=<bytes> sizeAfterBytes=<bytes>
```

Fields:

- `framesBefore`: Original frame count.
- `framesAfter`: Optimized frame count.
- `duplicateFramesRemoved`: Consecutive duplicate decoded frames removed.
- `durationMs`: Optimized GIF duration. It should match the original duration when only duplicate frames were coalesced.
- `sizeBeforeBytes` / `sizeAfterBytes`: File sizes before and after optimization.

## Stderr

- Missing files write `GIF file '<path>' was not found.`
- Missing output writes `gif optimize requires --output <gif>.`
- Invalid images or options write `Could not optimize GIF '<path>'. <reason>`

## Exit Codes

- `0`: Optimized GIF was written.
- `1`: File was missing, output was missing, or the input was not a valid GIF.

## Examples

```powershell
cmg gif optimize demo-output\dialog-flow.gif --output demo-output\dialog-flow.optimized.gif
cmg gif compare demo-output\dialog-flow.gif demo-output\dialog-flow.optimized.gif
```
