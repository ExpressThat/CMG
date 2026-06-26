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

## Stdout

On pass:

```text
PASS 001 expectScreenshot #dialog baseline=baseline.png
VISUAL 001 diff=0
```

When the baseline is missing, CMG creates it from the actual screenshot and exits `1` with the reason on stderr.

## Exit Codes

- `0`: Screenshot matched within tolerance.
- `1`: Browser is not running, baseline was created, or the assertion failed.
