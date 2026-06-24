# `browser control dragAndDrop`

Runs the simple scripting `dragAndDrop` action once from the command line.

```powershell
cmg browser control dragAndDrop "<sourceSelector>" "<targetSelector>"
```

## Arguments

- `<sourceSelector>`: CSS selector for the drag source.
- `<targetSelector>`: CSS selector for the drop target.

## Stdout

```text
PASS 001 dragAndDrop "[data-command='browser launch']" #dropQueue
```

## Stderr

Writes browser or missing-element errors.

## Exit Codes

- `0`: Drag-and-drop completed.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control dragAndDrop "[data-command='browser launch']" "#dropQueue"
```

## Complex Drags

The block form is available in `.cmgscript` files:

```text
dragAndDrop "[data-command='browser launch']" {
  delay 200
  hover "#lastDialogAction"
  drop "#dropQueue"
}
```

Use [`script`](script.md) for block drags.
