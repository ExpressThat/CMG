# `browser control press`

Runs the scripting `press` action once from the command line.

```powershell
cmg browser control press "<key>"
```

## Arguments

- `<key>`: Key name, such as `Enter` or `Escape`.

## Stdout

```text
PASS 001 press Enter
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Key press completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control press "Escape"
```
