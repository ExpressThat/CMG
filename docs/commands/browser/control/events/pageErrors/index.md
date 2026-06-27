# `browser control events pageErrors`

Page-error diagnostics list, wait, and absence assertion commands.

```powershell
cmg browser control events pageErrors [command] [options]
```

## Subcommands

- [`capture`](capture.md): Deprecated compatibility alias that ensures page-error diagnostics capture is installed.
- [`capturePageErrors`](capturePageErrors.md): Deprecated compatibility alias that ensures page-error diagnostics capture is installed.
- [`list`](list.md): List captured page errors.
- [`listPageErrors`](listPageErrors.md): List captured page errors.
- [`wait`](wait.md): Wait for a matching page error.
- [`waitForPageError`](waitForPageError.md): Wait for a matching page error.
- [`expectNoPageError`](expectNoPageError.md): Assert that no matching page error was captured.
- [`toHaveNoPageError`](toHaveNoPageError.md): Provider-style alias for `expectNoPageError`.

## Diagnostic Workflow

CMG arms page-error diagnostics automatically when `cmg browser launch`, `cmg browser app launch`, or `cmg browser app attach` succeeds. Captured `error` and `unhandledrejection` events are stored in `window.__cmgPageErrors`, so they continue accumulating while CMG is not connected between commands. Capture is forward-only from launch/attach/arming time; events from before that point cannot be recovered.

`capture` and `capturePageErrors` are deprecated compatibility aliases for "ensure capture is installed." They are idempotent and do not clear existing entries. Agents should prefer automatic launch/attach capture plus list/assert commands because the old "remember to arm before the risky click" workflow is easy to miss and loses errors that occur between commands.

For visual testing, pair page-error capture with console capture so browser exceptions and console errors are both checked:

```text
click "#risky"
screenshotPage output="artifacts/after-click.png"
listPageErrors
listConsole level=error
expectNoPageError timeout=250
expectNoConsole level=error timeout=250
```
