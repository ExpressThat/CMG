# `browser control hover`

Runs the scripting `hover` action once from the command line.

```powershell
cmg browser control hover "<selector>"
```

## Arguments

- `<selector>`: CSS selector.

## Stdout

```text
PASS 001 hover #openProfileDialog
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: Hover completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control hover "#openProfileDialog"
```
