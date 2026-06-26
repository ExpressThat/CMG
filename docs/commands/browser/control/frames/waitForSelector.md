# `browser control frames waitForSelector`

Provider-style alias for [`frameWaitForSelector`](frameWaitForSelector.md).

```powershell
cmg browser control frames waitForSelector "<frameSelector>" "<selector>" [--timeout <milliseconds>]
```

Arguments, options, stdout, stderr, exit codes, and GIF behavior match [`frameWaitForSelector`](frameWaitForSelector.md).

## Examples

```powershell
cmg browser control frames waitForSelector "#checkoutFrame" "#ready" --timeout 5000
```
