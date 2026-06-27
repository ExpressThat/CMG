# `browser control coverage start`

Starts JavaScript and CSS coverage collection.

```powershell
cmg browser control coverage start [--js <true|false>] [--css <true|false>]
```

## Options

- `--js <true|false>`: Collect JavaScript coverage. Default is `true`.
- `--css <true|false>`: Collect CSS coverage. Default is `true`.

## Stdout

```text
PASS 001 startCoverage js=true css=true
COVERAGE_STARTED 001 js=true css=true
```

## Exit Codes

- `0`: Coverage collection started.
- `1`: Browser is not running or the action failed.
