# `browser control input click`

Runs the scripting `click` action once from the command line.

```powershell
cmg browser control input click "<selector>" [options]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Options

- `--button <left|right|middle>`: Mouse button. Defaults to the browser-native left click path when no click options are provided.
- `--click-count <count>`: Number of clicks to dispatch. Must be at least `1`.
- `--delay <milliseconds>`: Delay between repeated clicks. Must be zero or greater.
- `--modifiers <keys>`: Comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `--x <pixels>`: X offset inside the target element.
- `--y <pixels>`: Y offset inside the target element.

## Stdout

```text
PASS 001 click #save
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: The element was clicked.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input click "#save"
cmg browser control input click "#canvas" --button middle --click-count 2 --delay 50 --modifiers Control+Shift --x 12 --y 8
```
