# `browser control network setProxy`

Runs the scripting `setProxy` action once from the command line.

```powershell
cmg browser control network setProxy "<prefix>"
```

## Arguments

- `<prefix>`: Proxy URL prefix used to rewrite fetch/XHR URLs.

## Stdout

```text
PASS 001 setProxy https://proxy.local/?url=
PROXY_SET 001 https://proxy.local/?url=
```

## Exit Codes

- `0`: Proxy rewrite was installed.
- `1`: Browser is not running or the action failed.
