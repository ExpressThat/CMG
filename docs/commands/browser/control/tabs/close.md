# `browser control tabs close`

Runs the scripting `closeTab` action once from the command line.

```powershell
cmg browser control tabs close --index <index>
```

## Options

- `--index <index>`: Required zero-based tab index from `tabs list`.

## Stdout

```text
PASS 001 closeTab
```

## Stderr

Writes browser or invalid-index errors.

## Exit Codes

- `0`: Tab was closed.
- `1`: Browser is not running, the index is invalid, or the action failed.

## Example

```powershell
cmg browser control tabs close --index 1
```
