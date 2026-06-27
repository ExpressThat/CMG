# `browser control input hover`

Runs the scripting `hover` action once from the command line.

```powershell
cmg browser control input hover "<selector>" [options]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Options

- `--modifiers <keys>`: Comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `--x <pixels>`: X offset inside the target element.
- `--y <pixels>`: Y offset inside the target element.

## Stdout

```text
PASS 001 hover #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Hover events were dispatched.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input hover "#save"
cmg browser control input hover "#canvas" --modifiers Control+Shift --x 12 --y 8
```
