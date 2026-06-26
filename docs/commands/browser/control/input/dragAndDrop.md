# `browser control input dragAndDrop`

Runs the scripting `dragAndDrop` action once from the command line.

```powershell
cmg browser control input dragAndDrop "<sourceSelector>" "<targetSelector>" [options]
```

## Arguments

- `<sourceSelector>`: CSS selector for the drag source.
- `<targetSelector>`: CSS selector for the drop target.

## Options

- `--source-x <pixels>`: X offset inside the source element. Defaults to the source center.
- `--source-y <pixels>`: Y offset inside the source element. Defaults to the source center.
- `--target-x <pixels>`: X offset inside the target element. Defaults to the target center.
- `--target-y <pixels>`: Y offset inside the target element. Defaults to the target center.

## Stdout

```text
PASS 001 dragAndDrop .card #dropZone
```

## Stderr

Writes browser, selector, or drag errors.

## Exit Codes

- `0`: The drag-and-drop action completed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control input dragAndDrop ".card" "#dropZone"
cmg browser control input dragAndDrop ".card" "#dropZone" --source-x 8 --source-y 8 --target-x 24 --target-y 16
```
