# `browser control tabs closeTab`

Runs the scripting `closeTab` action once from the command line.

```powershell
cmg browser control tabs closeTab --index <index>
```

## Options

- `--index <index>`: Required zero-based tab index.

## Stdout

```text
PASS 001 closeTab index=1
TAB_CLOSED 001 index=1
```

## Stderr

Writes browser, invalid-index, parse, or action errors.

## Exit Codes

- `0`: Tab was closed.
- `1`: Browser is not running, the index is invalid, or the action failed.

## Examples

```powershell
cmg browser control tabs closeTab --index 1
```
