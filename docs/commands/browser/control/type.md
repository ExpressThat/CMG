# `browser control type`

Runs the scripting `type` action once from the command line.

```powershell
cmg browser control type "<selector>" "<text>"
```

## Arguments

- `<selector>`: CSS selector for the input-like element.
- `<text>`: Text to append to the element value.

## Stdout

```text
PASS 001 type #profileName "CMG Test Profile"
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: Text was typed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control type "#profileName" "CMG Test Profile"
```
