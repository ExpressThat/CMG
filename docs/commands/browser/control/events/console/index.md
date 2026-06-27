# `browser control events console`

Console diagnostics list, wait, and absence assertion commands.

```powershell
cmg browser control events console [command] [options]
```

## Subcommands

- [`capture`](capture.md): Deprecated compatibility alias that ensures console diagnostics capture is installed.
- [`captureConsole`](captureConsole.md): Deprecated compatibility alias that ensures console diagnostics capture is installed.
- [`list`](list.md): List captured console messages.
- [`listConsole`](listConsole.md): List captured console messages.
- [`wait`](wait.md): Wait for a matching console message.
- [`waitForConsole`](waitForConsole.md): Wait for a matching console message.
- [`expectNoConsole`](expectNoConsole.md): Assert that no matching console message was captured.
- [`toHaveNoConsole`](toHaveNoConsole.md): Provider-style alias for `expectNoConsole`.

## Diagnostic Workflow

CMG arms console diagnostics automatically when `cmg browser launch`, `cmg browser app launch`, or `cmg browser app attach` succeeds. Captured entries are stored in `window.__cmgConsole`, so they continue accumulating while CMG is not connected between commands. Capture is forward-only from launch/attach/arming time; events from before that point cannot be recovered.

`capture` and `captureConsole` are deprecated compatibility aliases for "ensure capture is installed." They are idempotent and do not clear existing entries. Agents should prefer automatic launch/attach capture plus list/assert commands because the old "remember to arm before the risky click" workflow is easy to miss and loses errors that occur between commands.

For visual testing, pair console capture with page-error capture so browser exceptions and console errors are both checked:

```text
click "#risky"
screenshotPage output="artifacts/after-click.png"
listPageErrors
listConsole level=error
expectNoPageError timeout=250
expectNoConsole level=error timeout=250
```
