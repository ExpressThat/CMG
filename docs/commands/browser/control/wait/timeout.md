# `browser control wait timeout`

Runs the scripting `waitForTimeout` action once from the command line.

```powershell
cmg browser control wait timeout <milliseconds>
```

## Arguments

- `<milliseconds>`: Delay duration in milliseconds.

## Stdout

```text
PASS 001 waitForTimeout 250
WAIT_TIMEOUT 001 250
```

## Stderr

Writes invalid-duration or action errors.

## Exit Codes

- `0`: Delay completed.
- `1`: Duration was invalid or the action failed.

## Example

```powershell
cmg browser control wait timeout 250
```
