# `browser control reload`

Runs the scripting `reload` action once from the command line.

```powershell
cmg browser control reload
```

## Stdout

```text
PASS 001 reload
RELOADED 001 https://example.com/
```

## Stderr

Writes browser or reload errors.

## Exit Codes

- `0`: Page reload was requested.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control reload
```
