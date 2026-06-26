# `browser control context bypassCSP`

Enables or disables page-side CSP bypass.

```powershell
cmg browser control context bypassCSP <enabled>
```

## Arguments

- `<enabled>`: `true` to remove page CSP meta tags, `false` to stop doing so.

## Stdout

```text
PASS 001 bypassCSP true
CSP_BYPASS 001 true
```

## Exit Codes

- `0`: CSP bypass behavior was updated.
- `1`: Browser is not running or the action failed.
