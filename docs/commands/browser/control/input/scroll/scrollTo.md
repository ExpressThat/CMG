# `browser control input scroll scrollTo`

Runs the scripting `scrollTo` action once from the command line.

```powershell
cmg browser control input scroll scrollTo [target] [options]
```

## Arguments

- `[target]`: Alias target: `top`, `bottom`, `left`, or `right`.

## Options

- `--x <pixels>`: Horizontal position.
- `--y <pixels>`: Vertical position.
- `--selector <selector>`: Optional element selector to scroll.

## Stdout

```text
PASS 001 scrollTo bottom
SCROLL_TO 001 0,2147483647
```

## Stderr

Writes browser, targeting, parse, or action errors.

## Exit Codes

- `0`: Scroll completed.
- `1`: Browser is not running, targeting is invalid, or the action failed.

## Examples

```powershell
cmg browser control input scroll scrollTo bottom
cmg browser control input scroll scrollTo --selector "#pane" --y 500
```
