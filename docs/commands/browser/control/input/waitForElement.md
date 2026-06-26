# `browser control input waitForElement`

Runs the scripting `waitForElement` action once from the command line.

```powershell
cmg browser control input waitForElement "<selector>" [--timeout <milliseconds>]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator.

## Options

- `--timeout <milliseconds>`: Maximum wait time. Default is `5000`.

## Stdout

```text
PASS 001 waitForElement #ready
```

## Stderr

Writes browser, timeout, or selector errors.

## Exit Codes

- `0`: The element was found.
- `1`: Browser is not running or the action failed.
