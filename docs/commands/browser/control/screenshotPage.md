# `browser control screenshotPage`

Runs the scripting `screenshotPage` action once from the command line.

```powershell
cmg browser control screenshotPage [--output <path>] [--full-page]
```

## Options

- `--output <path>`: Optional PNG output path. Without it, stdout includes a `data:image/png;base64,...` result.
- `--full-page`: Capture the full scrollable page instead of only the current viewport.

## Stdout

```text
PASS 001 screenshotPage
SCREENSHOT 001 C:\Projects\CMG\page.png
```

Without `--output`:

```text
SCREENSHOT 001 data:image/png;base64,<base64-png-data>
```

## Stderr

Writes browser or capture errors.

## Exit Codes

- `0`: Screenshot was captured.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control screenshotPage --output page.png
cmg browser control screenshotPage --full-page --output page-full.png
```
