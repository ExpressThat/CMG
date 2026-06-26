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
- `--style <css>`: Temporary CSS applied only while the screenshot artifact is captured.
- `--style-path <file>`: CSS file applied only while the screenshot artifact is captured. Cannot be combined with `--style`.
- `--mask <selectors>`: Semicolon-separated CSS selectors or CMG rich locators to cover only during screenshot artifact capture.
- `--mask-color <css-color>`: CSS color used for masks. Default is `#ff00ff`.
- `--animations <disabled|allow>`: Use `disabled` to stop CSS animations and transitions only while the artifact is captured.
- `--caret <hide|initial>`: Use `hide` to make text carets transparent only while the artifact is captured.
- `--clip-x <pixels>`: Optional page or viewport clip X coordinate in CSS pixels.
- `--clip-y <pixels>`: Optional page or viewport clip Y coordinate in CSS pixels.
- `--clip-width <pixels>`: Optional clip width in CSS pixels. Must be greater than `0` when any clip option is used.
- `--clip-height <pixels>`: Optional clip height in CSS pixels. Must be greater than `0` when any clip option is used.

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

Writes browser, capture, style file, missing-mask-selector, or option errors. Invalid screenshot options report the specific option, such as `type= must be png, jpeg, or jpg`, `quality= must be between 0 and 100`, `quality= is only valid when type=jpeg`, `animations= must be disabled or allow`, `caret= must be hide or initial`, or `style= and stylePath= cannot be used together`.

Clip options map to scripting `clipX`, `clipY`, `clipWidth`, and `clipHeight`. With `--full-page`, the clip is relative to the page document; otherwise it is relative to the current viewport. Masks, disabled animations, and hidden carets are temporary changes for the screenshot artifact only; GIF frames keep showing the real page state.

## Exit Codes

- `0`: Screenshot was captured.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control capture screenshotPage --output page.png
cmg browser control capture screenshotPage --full-page --output page-full.png
cmg browser control capture screenshotPage --type jpeg --quality 85 --output page.jpg
cmg browser control capture screenshotPage --clip-x 40 --clip-y 120 --clip-width 640 --clip-height 360 --output crop.png
cmg browser control capture screenshotPage --style-path fixtures\screenshot.css --output stable-page.png
cmg browser control capture screenshotPage --mask "#clock;hasText=.ad|Sponsored" --mask-color "#000000" --output stable-page.png
cmg browser control capture screenshotPage --animations disabled --caret hide --output deterministic-page.png
```
