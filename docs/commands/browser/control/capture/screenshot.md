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
- `--mask <selectors>`: Semicolon-separated CSS selectors or CMG rich locators to cover only during screenshot artifact capture.
- `--mask-color <css-color>`: CSS color used for masks. Default is `#ff00ff`.

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

Writes browser, option, style file, missing-element, or missing-mask-selector errors. Invalid screenshot options report the specific option, such as `type= must be png, jpeg, or jpg`, `quality= must be between 0 and 100`, `quality= is only valid when type=jpeg`, or `style= and stylePath= cannot be used together`.

Unlike user-like actions such as `click`, `type`, and `dragAndDrop`, `screenshot` scrolls the selected element into view before capture. Masks are temporary page overlays for the screenshot artifact only; GIF frames keep showing the real page state.

## Exit Codes

- `0`: Screenshot was captured.
- `1`: Browser is not running, no element matched, or the action failed.

## Example

```powershell
cmg browser control capture screenshot "#profileDialog" --output profile-dialog.png
cmg browser control capture screenshot "#profileDialog" --type jpeg --quality 80 --output profile-dialog.jpg
cmg browser control capture screenshot "#profileDialog" --style ".clock{visibility:hidden}" --output stable-dialog.png
cmg browser control capture screenshot "#profileDialog" --mask "#clock;#ad" --mask-color "#000000" --output masked-dialog.png
```
