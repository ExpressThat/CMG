# `browser control input dragTo`

Runs the scripting `dragTo` alias once from the command line. It uses the same pointer-aware drag recorder path as `dragAndDrop`.

```powershell
cmg browser control input dragTo "<sourceSelector>" "<targetSelector>" [options]
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
cmg browser control input dragTo ".card" "#dropZone" --source-x 8 --source-y 8 --target-x 24 --target-y 16
```
