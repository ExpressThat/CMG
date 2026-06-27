# `browser control input scroll to`

Scrolls to an absolute position or alias.

```powershell
cmg browser control input scroll to [target] [options]
```

## Arguments

- `[target]`: `top`, `bottom`, `left`, or `right`.

## Options

- `--x <pixels>`: Horizontal position.
- `--y <pixels>`: Vertical position.
- `--selector <selector>`: Optional element selector to scroll.

## Stdout

```text
PASS 001 scrollTo bottom
SCROLL_TO 001 0,2147483647
```

## Exit Codes

- `0`: Scroll completed.
- `1`: Browser is not running or the action failed.
