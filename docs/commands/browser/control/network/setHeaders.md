# `browser control network setHeaders`

Runs the scripting `setExtraHTTPHeaders` action once from the command line.

```powershell
cmg browser control network setHeaders <name> <value> [<name> <value>...]
```

## Arguments

- `<name> <value>`: One or more header name/value pairs.

## Stdout

```text
PASS 001 setExtraHTTPHeaders X-CMG-Agent true Accept application/json
HEADERS_SET 001 2
```

## Exit Codes

- `0`: Headers were installed for current and future page requests.
- `1`: Browser is not running or the action failed.
