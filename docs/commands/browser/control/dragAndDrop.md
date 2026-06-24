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

Writes browser, missing-element, or offscreen-element errors. `dragAndDrop` does not scroll automatically; the source and target centers must both already be inside the current viewport.

## Exit Codes

- `0`: Drag-and-drop completed.
- `1`: Browser is not running, no element matched, either endpoint is outside the current viewport, or the action failed.

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
