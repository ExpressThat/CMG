# `browser control input dblclick`

Runs the scripting `dblclick` action once from the command line.

```powershell
cmg browser control input dblclick "<selector>" [options]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--modifiers <keys>`: Comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `--x <pixels>`: X offset inside the element.
- `--y <pixels>`: Y offset inside the element.

## Stdout

```text
PASS 001 dblclick #save
MOUSE_EVENT 001 dblclick #save
```

## Stderr

Writes browser, selector, option, or action errors. Invalid offsets must be zero or greater. Invalid modifiers name the supported modifier keys.

## Exit Codes

- `0`: Double-click completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input dblclick "#save"
cmg browser control input dblclick "#canvas" --modifiers Shift --x 12 --y 8
```
