# `browser control input doubleClick`

Runs the scripting `doubleClick` action once from the command line.

```powershell
cmg browser control input doubleClick "<selector>" [options]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Options

- `--modifiers <keys>`: Comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `--x <pixels>`: X offset inside the element.
- `--y <pixels>`: Y offset inside the element.

## Stdout

```text
PASS 001 doubleClick #save
MOUSE_EVENT 001 dblclick #save
```

## Stderr

Writes browser, selector, option, or action errors. Invalid offsets must be zero or greater. Invalid modifiers name the supported modifier keys.

## Exit Codes

- `0`: The double-click event was dispatched.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control input doubleClick "#save"
cmg browser control input doubleClick "#canvas" --modifiers Control+Shift --x 12 --y 8
```
