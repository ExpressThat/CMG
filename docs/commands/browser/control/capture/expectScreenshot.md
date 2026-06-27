# `browser control capture expectScreenshot`

Compares an element or page screenshot to a baseline.

```powershell
cmg browser control capture expectScreenshot [selector] --baseline <file> [options]
```

## Arguments

- `[selector]`: Optional CSS selector or CMG rich locator. Omit to compare the page viewport.

## Options

- `--baseline <file>`: Required baseline PNG path.
- `--output <file>`: Actual PNG output path. Default is `actual.png`.
- `--tolerance <number>`: Allowed normalized diff. Default is `0`.
- `--full-page`: Capture the full scrollable page for page screenshot assertions. Ignored when `[selector]` is provided.
- `--mask <selectors>`: Semicolon-separated selectors or CMG rich locators to cover before comparison.
- `--mask-color <hex>`: Mask fill color. Default is `#ff00ff`.

## Stdout

On pass:

```text
PASS 001 expectScreenshot #dialog baseline=baseline.png
VISUAL 001 diff=0
```

When the baseline is missing, CMG creates it from the actual screenshot and exits `1` with the reason on stderr.

## Exit Codes

- `0`: Screenshot matched within tolerance.
- `1`: Browser is not running, baseline was created, options are invalid, a mask selector was missing, or the assertion failed.

## Example

```powershell
cmg browser control capture expectScreenshot "#dialog" --baseline baselines/dialog.png --output actual-dialog.png
cmg browser control capture expectScreenshot --baseline baselines/page.png --full-page --mask "#clock;#ad" --mask-color "#000000"
```
