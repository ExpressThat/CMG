# `browser control context browserContexts useContext`

Runs the scripting `useContext` action once from the command line.

```powershell
cmg browser control context browserContexts useContext <id>
```

## Arguments

- `<id>`: Context id or target id.

## Stdout

```text
PASS 001 useContext ctx-1
CONTEXT_ACTIVE 001 ctx-1
```

## Stderr

Writes browser, context id, parse, or action errors.

## Exit Codes

- `0`: Context was activated.
- `1`: Browser is not running, the context is missing, or the action failed.

## Examples

```powershell
cmg browser control context browserContexts useContext ctx-1
```
