# `browser control input scroll by`

Scrolls by a delta.

```powershell
cmg browser control input scroll by <x> <y> [--selector <selector>]
cmg browser control input scroll by --x <pixels> --y <pixels> [--selector <selector>]
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
PASS 001 scrollBy 0 160
SCROLL_BY 001 0,160
PASS 001 scrollBy x=0 y=-80 selector=text=Panel
SCROLL_BY 001 0,-80
```

## Exit Codes

- `0`: Scroll completed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input scroll by 0 160
cmg browser control input scroll by --x 0 --y -80 --selector "text=Panel"
```
