# `browser control setViewport`

Runs the scripting `setViewport` action once from the command line.

```powershell
cmg browser control setViewport --width <pixels> --height <pixels>
```

## Options

- `--width <pixels>`: Required viewport width in CSS pixels.
- `--height <pixels>`: Required viewport height in CSS pixels.

## Stdout

```text
PASS 001 setViewport
```

## Stderr

Writes browser or validation errors.

## Exit Codes

- `0`: Viewport was set.
- `1`: Browser is not running, required options are missing, or the action failed.

## Example

```powershell
cmg browser control setViewport --width 1280 --height 720
```
