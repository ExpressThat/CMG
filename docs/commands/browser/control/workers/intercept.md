# `browser control workers intercept`

Patches worker `fetch()` responses.

```powershell
cmg browser control workers intercept "<pattern>" [options]
```

## Arguments

- `<pattern>`: Worker fetch URL text to intercept. Default matching is substring matching unless `--match` changes it.

## Options

- `--status <status>`: Mocked response status. Default is `200`.
- `--body <text>`: Mocked response body.
- `--body-file <file>`: Read mocked response body from a local file.
- `--content-type <type>`: Mocked response content type. Default is `text/plain`.
- `--match <contains|exact|regex>`: Worker fetch URL match mode. Default is `contains`.
- `--ignore-case`: Match the worker fetch URL without case sensitivity.
- `--header <name: value>`: Mocked response header.
- `--headers <headers>`: Mocked response headers separated by semicolons.
- `--header-name <name>`: Mocked response header name.
- `--header-value <value>`: Mocked response header value for `--header-name`.
- `--target <id-or-url>`: Worker id or URL substring. Defaults to the first worker.

## Stdout

```text
PASS 001 workerIntercept /api/profile status=200
WORKER_INTERCEPT 001 routes=1 /api/profile
```

## Exit Codes

- `0`: Worker fetch interception was installed.
- `1`: Browser is not running or the action failed.

## Examples

```powershell
cmg browser control workers intercept "/api/profile/\d+" --match regex --ignore-case --body-file fixtures/profile.json --content-type application/json --header "X-Trace: worker"
```
