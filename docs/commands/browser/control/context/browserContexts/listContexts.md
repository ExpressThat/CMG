# `browser control context browserContexts listContexts`

Runs the scripting `listContexts` action once from the command line.

```powershell
cmg browser control context browserContexts listContexts
```

## Stdout

```text
PASS 001 listContexts
CONTEXT 0 id=<context-id> target=<target-id> active=true url="about:blank"
```

## Stderr

Writes browser or action errors.

## Exit Codes

- `0`: Contexts were listed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control context browserContexts listContexts
```
