# `browser control input dragAndDrop`

Runs the scripting `dragAndDrop` action once from the command line.

```powershell
cmg browser control input dragAndDrop "<sourceSelector>" "<targetSelector>"
```

## Arguments

- `<sourceSelector>`: CSS selector for the drag source.
- `<targetSelector>`: CSS selector for the drop target.

## Stdout

```text
PASS 001 dragAndDrop .card #dropZone
```

## Stderr

Writes browser, selector, or drag errors.

## Exit Codes

- `0`: The drag-and-drop action completed.
- `1`: Browser is not running or the action failed.
