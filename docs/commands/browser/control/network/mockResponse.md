# `browser control network mockResponse`

Runs the scripting `mockResponse` action once from the command line. This is a route alias for explicitly mocked fetch/XHR responses.

```powershell
cmg browser control network mockResponse "<pattern>" [options]
```

## Arguments

- `<pattern>`: URL text to match. Default matching is substring matching unless `--match` changes it.

## Options

- `--status <status>`: Mocked response status.
- `--body <body>`: Mocked response body.
- `--body-file <file>`: Read mocked response body from a local file.
- `--content-type <type>`: Mocked response content type.
- `--match <contains|exact|regex>`: URL match mode. Default is `contains`.
- `--ignore-case`: Match the URL without case sensitivity.
- `--method <method>`: HTTP method filter.
- `--times <count>`: Remove route after this many matches.
- `--delay <milliseconds>`: Response delay.
- `--abort`: Abort matching requests.
- `--header <name: value>`: Mocked response header.
- `--headers <headers>`: Mocked response headers separated by semicolons.
- `--header-name <name>`: Mocked response header name.
- `--header-value <value>`: Mocked response header value for `--header-name`.

## Stdout

```text
PASS 001 mockResponse /api/profile
ROUTE 001 /api/profile
```

## Stderr

Writes browser, option, or action errors. Invalid match modes report `match= must be contains, exact, or regex`; invalid regex patterns report `Invalid network regex '<pattern>': <reason>`. Invalid headers report `headers must be formatted as Name: value`.

## Exit Codes

- `0`: Mock response route was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control network mockResponse "/api/profile" --status 200 --body "{\"name\":\"CMG\"}" --content-type application/json
cmg browser control network mockResponse "/api/profile" --body-file fixtures/profile.json --content-type application/json
cmg browser control network mockResponse "/api/profile/\d+" --match regex --ignore-case --status 200
cmg browser control network mockResponse "/api/profile" --header-name X-Trace --header-value demo
cmg browser control network mockResponse "/api/slow" --status 200 --delay 250
```
