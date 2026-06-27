# `browser control input mouse down`

Moves to the target and presses the mouse button.

```powershell
cmg browser control input mouse down [target] [options]
```

## Options

Accepts the same target options as [`mouse move`](move.md).

## Stdout

```text
PASS 001 mouseDown center
MOUSE_DOWN 001 400,300
```

## Exit Codes

- `0`: Mouse button was pressed.
- `1`: Browser is not running or the action failed.
