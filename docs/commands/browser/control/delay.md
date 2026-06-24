# `browser control delay`

Runs the scripting `delay` action once from the command line.

```powershell
cmg browser control delay <milliseconds>
```

## Arguments

- `<milliseconds>`: Delay duration.

## Stdout

```text
PASS 001 delay 1000
```

## Stderr

Writes browser or validation errors. Like other control actions, this command requires a running CMG-controlled browser.

## Exit Codes

- `0`: Delay completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control delay 1000
```
