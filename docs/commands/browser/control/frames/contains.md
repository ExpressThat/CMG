# `browser control frames contains`

Cypress-style alias for [`frameContains`](frameContains.md).

```powershell
cmg browser control frames contains "<frameSelector>" "<selector>" "<text>" [options]
```

Arguments, options, stdout, stderr, exit codes, and GIF behavior match [`frameContains`](frameContains.md).

## Examples

```powershell
cmg browser control frames contains "#checkoutFrame" "#status" "Saved"
```
