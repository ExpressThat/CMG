# `browser control select`

Runs the scripting `select` action once from the command line.

```powershell
cmg browser control select "<selector>" "<value>"
```

## Arguments

- `<selector>`: CSS selector for the select-like element.
- `<value>`: Value to select.

## Stdout

```text
PASS 001 select #environment prod
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: Value was selected.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control select "#environment" "prod"
```
