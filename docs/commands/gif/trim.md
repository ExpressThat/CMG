# `gif trim`

Trim an existing GIF without launching a browser.

```powershell
cmg gif trim <file> --output <gif> [--start-frame <index>] [--end-frame <index>]
cmg gif trim <file> --output <gif> [--start-time <ms>] [--end-time <ms>]
```

## Arguments And Options

- `<file>`: Existing input GIF.
- `--output <gif>`: Required output path.
- `--start-frame <index>`: Inclusive zero-based first frame. Defaults to `0`.
- `--end-frame <index>`: Inclusive zero-based last frame. Defaults to the final frame.
- `--start-time <ms>`: Inclusive start time. Defaults to `0`; boundary-frame delay is shortened precisely.
- `--end-time <ms>`: Exclusive end time. Defaults to the GIF duration; boundary-frame delay is shortened precisely.

Frame and time options cannot be mixed. Values must be non-negative and start must precede end.

## Output And Exit Codes

Success writes one stdout line:

```text
GIF_TRIM input="<path>" output="<path>" framesBefore=<count> framesAfter=<count> durationBeforeMs=<ms> durationAfterMs=<ms>
```

Failures write a specific reason to stderr, including missing input/output, invalid ranges, mixed modes, or invalid image data.

- `0`: Trimmed GIF written.
- `1`: Validation or processing failed; no success line is written.

```powershell
cmg gif trim artifacts\flow.gif --output artifacts\flow-short.gif --start-frame 2 --end-frame 8
cmg gif trim artifacts\flow.gif --output artifacts\flow-middle.gif --start-time 500 --end-time 3200
```
