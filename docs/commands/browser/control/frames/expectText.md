# `browser control frames expectText`

Provider-style alias for [`frameExpectText`](frameExpectText.md).

```powershell
cmg browser control frames expectText "<frameSelector>" "<selector>" "<text>" [options]
```

Arguments, options, stdout, stderr, exit codes, and GIF behavior match [`frameExpectText`](frameExpectText.md).

## Examples

```powershell
cmg browser control frames expectText "#checkoutFrame" "#status" "Saved"
```
