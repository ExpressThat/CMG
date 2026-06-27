# `browser control context setServiceWorkers`

Runs the scripting `setServiceWorkers` action once from the command line.

```powershell
cmg browser control context setServiceWorkers <mode>
```

## Arguments

- `<mode>`: `allow` or `block`.

## Stdout

```text
PASS 001 setServiceWorkers block
SERVICE_WORKERS 001 block
```

## Stderr

Writes browser or invalid-mode errors.

## Exit Codes

- `0`: Service worker behavior was configured.
- `1`: Browser is not running, mode was invalid, or the action failed.

## Example

```powershell
cmg browser control context setServiceWorkers block
```
