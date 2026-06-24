# `browser control evaluate`

Runs the scripting `evaluate` action once from the command line.

```powershell
cmg browser control evaluate "<expression>"
```

## Arguments

- `<expression>`: JavaScript expression to evaluate in the primary page target.

## Stdout

Prints a `PASS` line and an `EVALUATE` result line:

```text
PASS 001 evaluate document.title
EVALUATE 001 CMG Browser Control Test Page
```

## Stderr

Writes browser or JavaScript evaluation errors.

## Exit Codes

- `0`: Expression was evaluated.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control evaluate "document.title"
```
