# `browser control context setJavaScriptEnabled`

Enables or disables dynamic JavaScript execution through CMG's page-side blocker.

```powershell
cmg browser control context setJavaScriptEnabled <enabled>
```

## Arguments

- `<enabled>`: `true` to enable JavaScript, `false` to block dynamic script execution.

## Stdout

```text
PASS 001 setJavaScriptEnabled false
JAVASCRIPT_ENABLED 001 false
```

## Exit Codes

- `0`: JavaScript behavior was updated.
- `1`: Browser is not running or the action failed.
