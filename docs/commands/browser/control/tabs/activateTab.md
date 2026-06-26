# `browser control tabs activateTab`

Runs the scripting `activateTab` action once from the command line.

```powershell
cmg browser control tabs activateTab --index <index>
```

## Options

- `--index <index>`: Required zero-based tab index.

## Stdout

```text
PASS 001 activateTab index=1
TAB_ACTIVE 001 index=1
```

## Stderr

Writes browser, invalid-index, parse, or action errors.

## Exit Codes

- `0`: Tab was activated.
- `1`: Browser is not running, the index is invalid, or the action failed.

## Examples

```powershell
cmg browser control tabs activateTab --index 1
```
