# `browser control page setViewportSize`

Runs the scripting `setViewportSize` alias once from the command line. It maps to the same behavior as `setViewport`.

```powershell
cmg browser control page setViewportSize --width <pixels> --height <pixels> [--device-scale-factor <number>] [--mobile] [--touch]
```

## Options

- `--width <pixels>`: Required viewport width in CSS pixels.
- `--height <pixels>`: Required viewport height in CSS pixels.
- `--device-scale-factor <number>`: Optional device scale factor. Default is `1`.
- `--mobile`: Use mobile viewport metrics.
- `--touch`: Enable touch viewport hints.

## Stdout

```text
PASS 001 setViewport
```

## Stderr

Writes browser, parse, validation, or action errors.

## Exit Codes

- `0`: Viewport was set.
- `1`: Browser is not running, required options are missing, or the action failed.

## Examples

```powershell
cmg browser control page setViewportSize --width 1280 --height 720
cmg browser control page setViewportSize --width 390 --height 844 --device-scale-factor 2 --mobile --touch
```
