# `api request`

Sends an HTTP request.

```powershell
cmg api request <method> <url> [options]
```

## Arguments

- `<method>`: HTTP method.
- `<url>`: Absolute request URL.

## Options

- `--body <text>`: Raw request body.
- `--json <json>`: JSON request body. Sets `Content-Type: application/json` unless overridden.
- `--content-type <type>`: Request content type.
- `--timeout <milliseconds>`: Timeout in milliseconds. Default is `30000`.
- `--status <status>`: Expected response status.
- `--status-match <status>`: Expected status list or range, for example `200,201` or `200-299`.
- `--ok`: Require a successful 2xx response status.
- `--contains <text>`: Expected response body text.
- `--not-contains <text>`: Text that must not appear in the response body.
- `--auth <username:password>`: Basic auth credentials.
- `--output <file>`: Write the response body to a file instead of emitting `API_BODY`.
- `--header <name=value>`: Header. Repeatable.
- `--query <name=value>`: Query parameter. Repeatable.
- `--form <name=value>`: Form field. Repeatable. Sends `application/x-www-form-urlencoded`.
- `--expect-header <name=value>`: Expected response header substring. Repeatable.

## Stdout

```text
API 001 200 https://example.test/status
API_BODY 001 ok
```

With `--output`, stdout contains:

```text
API 001 200 https://example.test/status
API_BODY_FILE 001 C:\Projects\CMG\body.txt
```

## Exit Codes

- `0`: Request succeeded and validations passed.
- `1`: Request failed or a validation failed.

## Examples

```powershell
cmg api request GET https://example.test/status --ok --contains ok
cmg api request POST https://example.test/login --form user=agent --form mode=test --auth user:pass
cmg api request GET https://example.test/items --status-match 200-299 --expect-header Content-Type=json --output body.json
```
