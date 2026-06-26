# `browser control coverage startCoverage`

Runs the scripting `startCoverage` action once from the command line.

```powershell
cmg browser control coverage startCoverage [--js <true|false>] [--css <true|false>]
```

## Options

- `--js <true|false>`: Collect JavaScript coverage. Default is `true`.
- `--css <true|false>`: Collect CSS coverage. Default is `true`.

## Stdout

```text
PASS 001 startCoverage js=true css=true
COVERAGE_STARTED 001 js=true css=true
```

## Stderr

Writes browser, parse, or action errors.

## Exit Codes

- `0`: Coverage collection started.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control coverage startCoverage --js true --css false
```
