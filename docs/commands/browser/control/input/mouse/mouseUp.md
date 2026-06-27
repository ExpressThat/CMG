# `browser control input mouse mouseUp`

Runs the scripting `mouseUp` action once from the command line.

```powershell
cmg browser control input mouse mouseUp [target] [options]
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
PASS 001 mouseUp center
MOUSE_UP 001 400,300
```

## Stderr

Writes browser, targeting, parse, or action errors.

## Exit Codes

- `0`: Mouse button was released.
- `1`: Browser is not running, targeting is invalid, or the action failed.

## Examples

```powershell
cmg browser control input mouse mouseUp --x 10 --y 20
```
