# `browser control input type`

Runs the scripting `type` action once from the command line.

```powershell
cmg browser control input type "<selector>" "<text>" [--delay <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.
- `<text>`: Text to append to the element value.

## Options

- `--delay <milliseconds>`: Optional delay between typed characters. Defaults to the fast native type path when omitted. Must be zero or greater.

## Stdout

```text
PASS 001 type #name "CMG Test"
```

## Stderr

Writes browser, selector, or action errors.

## Exit Codes

- `0`: Text was typed into the element.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input type "#name" "CMG"
cmg browser control input type "#name" "CMG" --delay 25
```
