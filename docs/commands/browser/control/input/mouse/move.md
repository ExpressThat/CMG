# `browser control input mouse move`

Moves the mouse.

```powershell
cmg browser control input mouse move [target] [options]
```

## Options

- `--x <pixels>`: Viewport x coordinate.
- `--y <pixels>`: Viewport y coordinate.
- `--selector <selector>`: Element selector for edge targeting.
- `--edge <edge>`: Element edge target.
- `--inset <pixels>`: Inset from the element edge.

## Stdout

```text
PASS 001 mouseMove center
MOUSE_MOVED 001 400,300
```

## Exit Codes

- `0`: Mouse moved.
- `1`: Browser is not running or the action failed.
