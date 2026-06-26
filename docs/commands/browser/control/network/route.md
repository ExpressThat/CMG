# `browser control network route`

Runs the scripting `route` action once from the command line.

```powershell
cmg browser control network route "<pattern>" [options]
```

## Arguments

- `<pattern>`: URL text to match. Default matching is substring matching unless `--match` changes it.

## Options

- `--status <status>`: Mocked response status.
- `--body <body>`: Mocked response body.
- `--content-type <type>`: Mocked response content type.
- `--match <contains|exact|regex>`: URL match mode. Default is `contains`.
- `--ignore-case`: Match the URL without case sensitivity.
- `--method <method>`: HTTP method filter.
- `--times <count>`: Remove route after this many matches.
- `--delay <milliseconds>`: Response delay.
- `--abort`: Abort matching requests.

## Stdout

```text
PASS 001 route /api/profile
ROUTE 001 /api/profile
```

## Stderr

Writes browser, option, or action errors. Invalid match modes report `match= must be contains, exact, or regex`; invalid regex patterns report `Invalid network regex '<pattern>': <reason>`.

## Exit Codes

- `0`: Route was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control network route "/api/profile" --status 200 --body "{\"name\":\"CMG\"}"
cmg browser control network route "/api/profile/\d+" --match regex --ignore-case --status 200
```
