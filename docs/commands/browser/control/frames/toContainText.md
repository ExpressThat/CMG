# `browser control frames toContainText`

Provider-style alias for [`frameToContainText`](frameToContainText.md).

```powershell
cmg browser control frames toContainText "<frameSelector>" "<selector>" "<text>" [options]
```

Arguments, options, stdout, stderr, exit codes, and GIF behavior match [`frameToContainText`](frameToContainText.md).

## Examples

```powershell
cmg browser control frames toContainText "#checkoutFrame" "#status" "Saved"
```
