# `browser control input scroll wheel`

Dispatches a wheel event and scrolls.

```powershell
cmg browser control input scroll wheel [target] [options]
```

## Arguments

- `[target]`: Alias or selector target.

## Options

- `--delta-x <pixels>`: Horizontal wheel delta.
- `--delta-y <pixels>`: Vertical wheel delta. Default is `100`.
- `--x <pixels>`: Viewport x coordinate.
- `--y <pixels>`: Viewport y coordinate.
- `--selector <selector>`: Element selector.
- `--edge <edge>`: Element edge target for pointer placement.
- `--inset <pixels>`: Inset from the element edge.

## Stdout

```text
PASS 001 wheel #pane deltaY=120
WHEEL 001 0,120
```

## Exit Codes

- `0`: Wheel event was dispatched.
- `1`: Browser is not running or the action failed.
