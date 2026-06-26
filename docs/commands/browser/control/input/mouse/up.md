# `browser control input mouse up`

Moves to the target and releases the mouse button.

```powershell
cmg browser control input mouse up [target] [options]
```

## Options

Accepts the same target options as [`mouse move`](move.md).

## Stdout

```text
PASS 001 mouseUp center
MOUSE_UP 001 400,300
```

## Exit Codes

- `0`: Mouse button was released.
- `1`: Browser is not running or the action failed.
