# `browser control events download`

Clicks an element and waits for a matching downloaded file.

```powershell
cmg browser control events download "<selector>" [options]
```

## Arguments

- `<selector>`: CSS selector or CMG rich locator to click.

## Options

- `--directory <path>`: Directory to watch. Default is the current directory.
- `--pattern <glob>`: File glob to match. Default is `*`.
- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `5000`.

## Stdout

```text
PASS 001 download #export
DOWNLOAD 001 C:\path\file.csv
```

## Stderr

Writes browser, selector, file, or timeout errors.

## Exit Codes

- `0`: The click ran and a matching download was found.
- `1`: Browser is not running or the action failed.
