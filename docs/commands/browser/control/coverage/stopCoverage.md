# `browser control coverage stopCoverage`

Runs the scripting `stopCoverage` action once from the command line.

```powershell
cmg browser control coverage stopCoverage [--path <file>]
```

## Options

- `--path <file>`: Write coverage JSON to this path instead of stdout.

## Stdout

Without `--path`, stdout includes the JSON payload:

```text
PASS 001 stopCoverage
COVERAGE 001 {"js":[],"css":[]}
```

With `--path`, stdout includes the written file path.

## Stderr

Writes browser, file, parse, or action errors.

## Exit Codes

- `0`: Coverage collection stopped.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control coverage stopCoverage --path artifacts\coverage.json
```
