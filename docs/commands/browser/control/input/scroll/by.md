# `browser control input scroll by`

Scrolls by a delta.

```powershell
cmg browser control input scroll by <x> <y> [--selector <selector>]
```

## Arguments

- `<x>`: Horizontal delta.
- `<y>`: Vertical delta.

## Options

- `--selector <selector>`: Optional element selector to scroll.

## Stdout

```text
PASS 001 scrollBy 0 160
SCROLL_BY 001 0,160
```

## Exit Codes

- `0`: Scroll completed.
- `1`: Browser is not running or the action failed.
