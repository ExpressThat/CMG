# `browser control input rightClick`

Runs the scripting `rightClick` action once from the command line.

```powershell
cmg browser control input rightClick "<selector>" [options]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Options

- `--modifiers <keys>`: Comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `--x <pixels>`: X offset inside the element.
- `--y <pixels>`: Y offset inside the element.

## Stdout

```text
PASS 001 rightClick #menu
MOUSE_EVENT 001 contextmenu #menu
```

## Stderr

Writes browser, selector, option, or action errors. Invalid offsets must be zero or greater. Invalid modifiers name the supported modifier keys.

## Exit Codes

- `0`: The context-menu event was dispatched.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control input rightClick "#menu"
cmg browser control input rightClick "#canvas" --modifiers Alt --x 12 --y 8
```
