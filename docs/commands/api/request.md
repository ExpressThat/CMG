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
- `--contains <text>`: Expected response body text.
- `--header <name=value>`: Header. Repeatable.
- `--query <name=value>`: Query parameter. Repeatable.

## Stdout

```text
API 001 200 https://example.test/status
API_BODY 001 ok
```

## Exit Codes

- `0`: Request succeeded and validations passed.
- `1`: Request failed or a validation failed.
