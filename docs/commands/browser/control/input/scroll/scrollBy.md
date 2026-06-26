# `browser control input scroll scrollBy`

Runs the scripting `scrollBy` action once from the command line.

```powershell
cmg browser control input scroll scrollBy <x> <y> [--selector <selector>]
```

## Arguments

- `<x>`: Horizontal delta.
- `<y>`: Vertical delta.

## Options

- `--selector <selector>`: Optional element selector to scroll.

## Stdout

```text
PASS 001 scrollBy 0 250
SCROLL_BY 001 0,250
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
```
