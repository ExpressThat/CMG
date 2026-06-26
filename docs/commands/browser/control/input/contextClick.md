# `browser control input contextClick`

Runs the scripting `contextClick` action once from the command line.

```powershell
cmg browser control input contextClick "<selector>" [options]
```

## Arguments

- `<selector>`: CSS selector or supported rich locator.

## Options

- `--modifiers <keys>`: Comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `--x <pixels>`: X offset inside the element.
- `--y <pixels>`: Y offset inside the element.

## Stdout

```text
PASS 001 contextClick #save
MOUSE_EVENT 001 contextmenu #save
```

## Stderr

Writes browser, selector, option, or action errors. Invalid offsets must be zero or greater. Invalid modifiers name the supported modifier keys.

## Exit Codes

- `0`: Context click completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control input contextClick "#save"
cmg browser control input contextClick "#canvas" --modifiers Control --x 12 --y 8
```
