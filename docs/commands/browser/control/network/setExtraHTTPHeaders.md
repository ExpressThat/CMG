# `browser control network setExtraHTTPHeaders`

Runs the scripting `setExtraHTTPHeaders` action once from the command line.

```powershell
cmg browser control network setExtraHTTPHeaders <name> <value> [<name> <value>...]
```

## Arguments

- `<name> <value>`: One or more header name/value pairs.

## Stdout

```text
PASS 001 setExtraHTTPHeaders X-CMG-Agent true
HEADERS_SET 001 1
```

## Stderr

Writes browser, argument, or action errors.

## Exit Codes

- `0`: Headers were set.
- `1`: Browser is not running, arguments are invalid, or the action failed.

## Examples

```powershell
cmg browser control network setExtraHTTPHeaders X-CMG-Agent true Accept application/json
```
