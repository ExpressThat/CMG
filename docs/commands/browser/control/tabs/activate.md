# `browser control tabs activate`

Runs the scripting `activateTab` action once from the command line.

```powershell
cmg browser control tabs activate --index <index>
```

## Options

- `--index <index>`: Required zero-based tab index from `tabs list`.

## Stdout

```text
PASS 001 activateTab
```

## Stderr

Writes browser or invalid-index errors.

## Exit Codes

- `0`: Tab was activated.
- `1`: Browser is not running, the index is invalid, or the action failed.

## Example

```powershell
cmg browser control tabs activate --index 0
```
