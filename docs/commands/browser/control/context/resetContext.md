# `browser control context resetContext`

Runs the scripting `resetContext` action once from the command line.

```powershell
cmg browser control context resetContext
```

## Stdout

```text
PASS 001 resetContext
CONTEXT_RESET 001
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Context state was cleared and the page navigated to `about:blank`.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control context resetContext
```
