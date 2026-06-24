# `browser control clear`

Runs the scripting `clear` action once from the command line.

```powershell
cmg browser control clear "<selector>"
```

## Arguments

- `<selector>`: CSS selector for the input-like element.

## Stdout

```text
PASS 001 clear #profileName
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: Element value was cleared.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control clear "#profileName"
```
