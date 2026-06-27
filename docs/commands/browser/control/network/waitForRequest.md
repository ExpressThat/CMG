# `browser control network waitForRequest`

Runs the scripting `waitForRequest` action once from the command line.

```powershell
cmg browser control network waitForRequest "<pattern>" [options]
```

## Arguments

- `<pattern>`: URL text to match. Default matching is substring matching unless `--match` changes it.

## Options

- `--timeout <milliseconds>`: Timeout.
- `--match <contains|exact|regex>`: URL match mode. Default is `contains`.
- `--ignore-case`: Match the URL without case sensitivity.
- `--method <method>`: HTTP method filter.
- `--status <status>`: HTTP status filter.
- `--contains <text>`: Body, response, or error text filter.
- `--mocked <true|false>`: Match mocked or real traffic.
- `--header <name[: value]>`: Header presence or value-substring filter. Header names are case-insensitive.
- `--header-name <name>`: Header name filter.
- `--header-value <text>`: Header value substring filter. Requires `--header` or `--header-name`.

## Stdout

```text
PASS 001 waitForRequest /api/profile
REQUEST 001 {"method":"GET","url":"/api/profile"}
```

## Stderr

Writes browser, timeout, option, or action errors. Invalid match modes report `match= must be contains, exact, or regex`; invalid regex patterns report `Invalid network regex '<pattern>': <reason>`.

## Exit Codes

- `0`: A matching request was found.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control network waitForRequest "/api/profile" --method POST --header "Authorization: Bearer"
cmg browser control network waitForRequest "/api/profile" --header-name X-CMG-Agent --header-value true
cmg browser control network waitForRequest "/api/profile/\d+" --match regex --ignore-case
```
