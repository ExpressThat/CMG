# `browser control network proxy`

Runs the scripting `proxy` alias once from the command line. It maps to the same behavior as `setProxy`.

```powershell
cmg browser control network proxy "<prefix>"
```

## Arguments

- `<prefix>`: Proxy URL prefix prepended to matching fetch/XHR URLs.

## Stdout

```text
PASS 001 setProxy https://proxy.local/
PROXY_SET 001 https://proxy.local/
```

## Stderr

Writes browser, argument, or action errors.

## Exit Codes

- `0`: Proxy rewrite was set.
- `1`: Browser is not running, arguments are invalid, or the action failed.

## Examples

```powershell
cmg browser control network proxy "https://proxy.local/"
```
