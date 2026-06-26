# `browser control clock restoreClock`

Runs the scripting `restoreClock` action once from the command line.

```powershell
cmg browser control clock restoreClock
```

## Stdout

```text
PASS 001 restoreClock
CLOCK_RESTORED 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Native clock APIs were restored.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control clock restoreClock
```
