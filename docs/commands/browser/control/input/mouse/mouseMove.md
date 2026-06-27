# `browser control input mouse mouseMove`

Runs the scripting `mouseMove` action once from the command line.

```powershell
cmg browser control input mouse mouseMove [target] [options]
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
PASS 001 mouseMove center
MOUSE_MOVED 001 400,300
```

## Stderr

Writes browser, targeting, parse, or action errors.

## Exit Codes

- `0`: Mouse moved.
- `1`: Browser is not running, targeting is invalid, or the action failed.

## Examples

```powershell
cmg browser control input mouse mouseMove center
cmg browser control input mouse mouseMove --selector "#save" --edge center
```
