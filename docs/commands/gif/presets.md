# `gif presets`

List the built-in GIF recording presets without launching or controlling a browser.

```powershell
cmg gif presets
```

## Arguments

No arguments.

## Options

No command-specific options.

## Stdout

Writes one parseable `GIF_PRESETS` line per preset family:

```text
GIF_PRESETS quality=archival,highest,high,medium,low defaultQuality=highest
GIF_PRESETS dither=none,floyd-steinberg,bayer,atkinson,sierra palette=global,local,adaptive colors=2..256
GIF_PRESETS pointerSpeed=slow,normal,fast,instant,multiplier defaultPointerSpeed=normal multiplierExample=1.5x
GIF_PRESETS pointerEasing=linear,ease-in,ease-out,ease-in-out,spring defaultPointerEasing=ease-in-out
GIF_PRESETS clickPulse=ring,ripple,dot,crosshair,none defaultClickPulse=ring
GIF_PRESETS timing defaultHoldAfterActionMs=350 defaultHoldOnFailureMs=1200
```

These values match the DSL recording options and whole-run CLI defaults used by `browser control script --gif` and `cmg run --gif`.

## Stderr

No stderr output on success.

## Exit Codes

- `0`: Presets were printed.

## Examples

```powershell
cmg gif presets
```
