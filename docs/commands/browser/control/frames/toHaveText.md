# `browser control frames toHaveText`

Provider-style alias for [`frameToHaveText`](frameToHaveText.md).

```powershell
cmg browser control frames toHaveText "<frameSelector>" "<selector>" "<text>" [options]
```

Arguments, options, stdout, stderr, exit codes, and GIF behavior match [`frameToHaveText`](frameToHaveText.md).

## Examples

```powershell
cmg browser control frames toHaveText "#checkoutFrame" "#status" "Saved" --match exact
```
