# `browser control page highlight`

Runs the scripting `highlight` action once from the command line.

```powershell
cmg browser control page highlight "<selector>" [--message <text>] [--color <css-color>] [--duration <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Options

- `--message <text>`: Optional message shown above the highlighted element.
- `--color <css-color>`: Optional highlight border and message tag color. Default is `#f59e0b`.
- `--duration <milliseconds>`: Optional duration in milliseconds. Default is `1200`.

## Stdout

```text
PASS 001 highlight #save
HIGHLIGHT 001 #save duration=1200
```

## Stderr

Writes browser, selector, locator, or option errors.

## Exit Codes

- `0`: Highlight was drawn.
- `1`: Browser is not running, no element matched, or the action failed.

## Examples

```powershell
cmg browser control page highlight "#save"
cmg browser control page highlight "getByRole=button|Save" --message Save --color "#2563eb" --duration 1500
```
