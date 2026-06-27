# `browser control input mouse mouseDown`

Runs the scripting `mouseDown` action once from the command line.

```powershell
cmg browser control input mouse mouseDown [target] [options]
```

## Arguments

- `[target]`: Alias target such as `center`, `top`, `bottom`, `left`, `right`, `topLeft`, `topRight`, `bottomLeft`, or `bottomRight`.

## Options

- `--x <pixels>`: Viewport x coordinate.
- `--y <pixels>`: Viewport y coordinate.
- `--selector <selector>`: Element selector for edge targeting.
- `--edge <edge>`: Element edge target.
- `--inset <pixels>`: Inset from the element edge.

## Stdout

```text
PASS 001 mouseDown center
MOUSE_DOWN 001 400,300
```

## Stderr

Writes browser, targeting, parse, or action errors.

## Exit Codes

- `0`: Mouse button was pressed.
- `1`: Browser is not running, targeting is invalid, or the action failed.

## Examples

```powershell
cmg browser control input mouse mouseDown --x 10 --y 20
```
