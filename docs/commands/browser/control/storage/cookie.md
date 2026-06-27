# `browser control storage cookie`

Runs the scripting `cookie` action once from the command line.

```powershell
cmg browser control storage cookie <operation> [key] [value] [options]
```

## Arguments

- `<operation>`: `get`, `set`, `remove`, or `clear`.
- `[key]`: Required for `set` and `remove`; optional for `get`.
- `[value]`: Required for `set`.

## Options

- `--domain <domain>`: Cookie domain for `set`, `remove`, or `clear`.
- `--path <path>`: Cookie path for `set`, `remove`, or `clear`. Defaults to `/`.
- `--expires <date>`: Cookie expiry date string for `set`.
- `--max-age <seconds>`: Cookie `Max-Age` in seconds for `set`.
- `--same-site <value>`: Cookie `SameSite` value for `set`; accepts `Strict`, `Lax`, or `None`.
- `--secure`: Adds the `Secure` attribute for `set`.

`HttpOnly` is not supported by this page-context action because browsers do not allow JavaScript to set `HttpOnly` cookies.

## Stdout

```text
PASS 001 cookie set mode demo
COOKIE 001 set mode
```

For all cookies:

```text
PASS 001 cookie get
COOKIE 001 get mode=demo
```

## Stderr

Writes browser or argument errors. Invalid cookie options name the unsupported option or invalid value.

## Exit Codes

- `0`: The cookie operation completed.
- `1`: Browser is not running or the action failed.

## Example

```powershell
cmg browser control storage cookie set mode demo
cmg browser control storage cookie set mode demo --path /app --same-site Lax --secure
cmg browser control storage cookie remove mode --path /app
```
