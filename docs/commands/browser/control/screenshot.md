# `browser control screenshot`

Runs the scripting `screenshot` action once from the command line.

```powershell
cmg browser control screenshot "<selector>" [--output <path>]
```

## Arguments

- `<selector>`: CSS selector.

## Options

- `--output <path>`: Optional PNG output path. Without it, stdout includes a `data:image/png;base64,...` result.

## Stdout

```text
PASS 001 screenshot #profileDialog
SCREENSHOT 001 C:\Projects\CMG\profile-dialog.png
```

Without `--output`:

```text
SCREENSHOT 001 data:image/png;base64,<base64-png-data>
```

## Stderr

Writes browser or missing-element errors.

Unlike user-like actions such as `click`, `type`, and `dragAndDrop`, `screenshot` scrolls the selected element into view before capture.

## Exit Codes

- `0`: Screenshot was captured.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control screenshot "#profileDialog" --output profile-dialog.png
```
