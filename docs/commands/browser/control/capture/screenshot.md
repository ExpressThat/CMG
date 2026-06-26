# `browser control capture screenshot`

Runs the scripting `screenshot` action once from the command line.

```powershell
cmg browser control capture screenshot "<selector>" [--output <path>] [options]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Options

- `--output <path>`: Optional screenshot output path. Without it, stdout includes a data URL.
- `--type <png|jpeg|jpg>`: Screenshot type. Default is `png`.
- `--quality <0-100>`: JPEG quality. Valid only with `--type jpeg` or `--type jpg`.
- `--omit-background`: Allow transparent page background when the browser supports it.
- `--style <css>`: Temporary CSS applied only while the screenshot artifact is captured.
- `--style-path <file>`: CSS file applied only while the screenshot artifact is captured. Cannot be combined with `--style`.

## Stdout

```text
PASS 001 screenshot #profileDialog
SCREENSHOT 001 C:\Projects\CMG\profile-dialog.png
```

Without `--output`:

```text
SCREENSHOT 001 data:image/png;base64,<base64-data>
```

For JPEG output without `--output`, the data URL starts with `data:image/jpeg;base64,`.

## Stderr

Writes browser, option, style file, or missing-element errors. Invalid screenshot options report the specific option, such as `type= must be png, jpeg, or jpg`, `quality= must be between 0 and 100`, `quality= is only valid when type=jpeg`, or `style= and stylePath= cannot be used together`.

Unlike user-like actions such as `click`, `type`, and `dragAndDrop`, `screenshot` scrolls the selected element into view before capture.

## Exit Codes

- `0`: Screenshot was captured.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control capture screenshot "#profileDialog" --output profile-dialog.png
cmg browser control capture screenshot "#profileDialog" --type jpeg --quality 80 --output profile-dialog.jpg
cmg browser control capture screenshot "#profileDialog" --style ".clock{visibility:hidden}" --output stable-dialog.png
```
