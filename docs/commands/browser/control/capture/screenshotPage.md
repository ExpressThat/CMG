# `browser control capture screenshotPage`

Runs the scripting `screenshotPage` action once from the command line.

```powershell
cmg browser control capture screenshotPage [--output <path>] [--full-page] [options]
```

## Options

- `--output <path>`: Optional screenshot output path. Without it, stdout includes a data URL.
- `--full-page`: Capture the full scrollable page instead of only the current viewport.
- `--type <png|jpeg|jpg>`: Screenshot type. Default is `png`.
- `--quality <0-100>`: JPEG quality. Valid only with `--type jpeg` or `--type jpg`.
- `--omit-background`: Allow transparent page background when the browser supports it.

## Stdout

```text
PASS 001 screenshotPage
SCREENSHOT 001 C:\Projects\CMG\page.png
```

Without `--output`:

```text
SCREENSHOT 001 data:image/png;base64,<base64-data>
```

For JPEG output without `--output`, the data URL starts with `data:image/jpeg;base64,`.

## Stderr

Writes browser, capture, or option errors. Invalid screenshot options report the specific option, such as `type= must be png, jpeg, or jpg`, `quality= must be between 0 and 100`, or `quality= is only valid when type=jpeg`.

## Exit Codes

- `0`: Screenshot was captured.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control capture screenshotPage --output page.png
cmg browser control capture screenshotPage --full-page --output page-full.png
cmg browser control capture screenshotPage --type jpeg --quality 85 --output page.jpg
```
