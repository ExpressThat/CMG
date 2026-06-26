# `browser control input dragTo`

Runs the scripting `dragTo` alias once from the command line. It uses the same pointer-aware drag recorder path as `dragAndDrop`.

```powershell
cmg browser control input dragTo "<sourceSelector>" "<targetSelector>"
```

## Arguments

- `<sourceSelector>`: CSS selector for the drag source.
- `<targetSelector>`: CSS selector for the drop target.

## Stdout

```text
PASS 001 dragTo .card #dropZone
```

## Stderr

Writes browser, selector, parse, or drag errors.

## Exit Codes

- `0`: The drag action completed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input dragTo ".card" "#dropZone"
```
