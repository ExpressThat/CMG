# `browser control workers workerIntercept`

Runs the scripting `workerIntercept` action once from the command line.

```powershell
cmg browser control workers workerIntercept "<pattern>" [options]
```

## Arguments

- `<pattern>`: Worker fetch URL text to intercept. Default matching is substring matching unless `--match` changes it.

## Options

- `--status <status>`: Mocked response status. Default is `200`.
- `--body <body>`: Mocked response body.
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
PASS 001 workerIntercept /api
WORKER_INTERCEPT 001 /api
```

## Stderr

Writes browser, worker, parse, or action errors. Invalid match modes report `match= must be contains, exact, or regex`; invalid regex patterns report `Invalid network regex '<pattern>': <reason>`. Invalid headers report `headers must be formatted as Name: value`.

## Exit Codes

- `0`: Worker fetch interception was installed.
- `1`: Browser is not running, the worker is missing, or the action failed.

## Examples

```powershell
cmg browser control workers workerIntercept "/api/profile" --status 200 --body "{}"
cmg browser control workers workerIntercept "/api/profile/\d+" --match regex --ignore-case --body-file fixtures/profile.json --header "X-Trace: worker"
```
