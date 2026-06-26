# `browser control context clearContext`

Runs the scripting `clearContext` action once from the command line.

```powershell
cmg browser control context clearContext
```

## Stdout

```text
PASS 001 clearContext
CONTEXT_CLEARED 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Context state was cleared.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control context clearContext
```
