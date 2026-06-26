# `browser control input scroll scrollBy`

Runs the scripting `scrollBy` action once from the command line.

```powershell
cmg browser control input scroll scrollBy <x> <y> [--selector <selector>]
cmg browser control input scroll scrollBy --x <pixels> --y <pixels> [--selector <selector>]
```

## Arguments

- `<x>`: Optional horizontal delta. Omit when using `--x`.
- `<y>`: Optional vertical delta. Omit when using `--y`.

## Options

- `--x <pixels>`: Horizontal delta. Negative values scroll left.
- `--y <pixels>`: Vertical delta. Negative values scroll up.
- `--selector <selector>`: Optional CSS selector or CMG rich locator to scroll.

## Stdout

```text
PASS 001 scrollBy 0 250
SCROLL_BY 001 0,250
PASS 001 scrollBy x=0 y=-80 selector=text=Panel
SCROLL_BY 001 0,-80
```

## Stderr

Writes browser, targeting, parse, or action errors.

## Exit Codes

- `0`: Scroll completed.
- `1`: Browser is not running, targeting is invalid, or the action failed.

## Examples

```powershell
cmg browser control input scroll scrollBy 0 250
cmg browser control input scroll scrollBy 0 250 --selector "#pane"
cmg browser control input scroll scrollBy --x 0 --y -80 --selector "text=Panel"
```
