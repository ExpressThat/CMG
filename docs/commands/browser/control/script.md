# `browser control script`

Runs a `.cmgscript` browser automation script against the selected CMG-controlled browser instance. This command is the direct AI/browser-control scripting surface. Use `cmg run` when the same actions should be executed as structured tests with reports, retries, sharding, and per-test traces.

Chrome is the default. Use `--chrome` to select Chrome explicitly, `--edge` for Microsoft Edge, or `--firefox` for Firefox.

```powershell
cmg browser control script --file <path>
cmg browser --port <port> control script --file <path>
cmg browser control script --file -
cmg browser control script --file <path> --gif <path>
cmg browser control script --file <path> --trace <path>
cmg browser control script --file <path> --timeout 10000 --assertion-timeout 5000
cmg browser control script --file <path> --base-url https://example.test/app/
cmg browser control script --file <path> --var user=Ada --env mode=demo
cmg --chrome browser control script --file <path>
cmg --edge browser control script --file <path>
cmg --firefox browser control script --file <path>
```

## Options

- `--file <path>`: Path to a `.cmgscript` file.
- `--file -`: Read script text from stdin.
- `--gif <path>`: Optional output path for an animated GIF recording of the script run.
- `--trace <path>`: Optional output path for a CMG script trace JSON file. The trace includes step names, line numbers, stdout lines, and failure reasons.
- `--timeout <milliseconds>`: Default timeout for timeout-capable waits, event waits, downloads, network waits, worker waits, tab waits, API requests, and assertions that do not set `timeout=`.
- `--navigation-timeout <milliseconds>`: Default timeout for navigation actions and navigation waits.
- `--assertion-timeout <milliseconds>`: Default timeout for assertions. Overrides `--timeout` for assertion actions.
- `--base-url <url>`: Absolute base URL used to resolve relative `navigate`, `goto`, `visit`, `openTab`, and `newContext url=` targets.
- `--var <name=value>`: Initial script variable. Can be repeated. Later entries with the same name replace earlier entries.
- `--env <name=value>`: Alias for `--var`, intended for CI and agent-provided environment values.

## Behavior

- Requires a browser started with [`browser launch`](../launch.md). For Edge, use `cmg --edge browser launch`. For Firefox, use `cmg --firefox browser launch`.
- Use `cmg browser --port <port> control script --file <path>` when the target browser was launched with `cmg browser --port <port> launch`.
- Use [`validateScript`](validateScript.md) to check imports and syntax before connecting to a browser.
- Executes actions in file order and stops on the first failed action.
- `skip "reason"` stops the script as skipped, writes `SKIP <line> <reason>` to stdout, and exits `0`.
- Writes step logs and action outputs to stdout.
- Writes validation, parse, browser, and action errors to stderr.
- Supports line-level `import "path"` statements. Relative imports resolve from the script file's directory.
- Supports the shared CMG action surface documented in the [action index](../../../scripting/action-index.md) and [action reference](../../../scripting/actions.md).
- Supports control flow, scoped variables, `set` block capture, macros, loops, `try`/`catch`/`finally`, `within`, frame blocks, `step`, and `gif` blocks.
- Relative navigation targets are resolved against `--base-url` before the browser is asked to navigate.
- Initial `--var` and `--env` values are available as `${name}` before the first action, macro call, condition, or `set` block runs.
- `set` is a script action for scoped variables and command-result capture. It is intentionally not a CLI command because it only has meaning inside a script scope.
- Uses the selected browser automation protocol through the active CMG endpoint: Chrome DevTools Protocol for Chrome and Edge, WebDriver BiDi for Firefox.
- Browser JavaScript dialogs are handled explicitly. CMG does not silently remove, accept, or dismiss dialogs through the browser protocol. Add `captureDialogs` or `setDialogBehavior` before the action that opens an `alert`, `confirm`, or `prompt`.

## GIF Behavior

- When `--gif` is provided, captures the visible page viewport after visual actions and writes an animated GIF.
- The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- Script-level `gif "name" { ... }`, `recordVideo "name" { ... }`, and `screencast "name" { ... }` blocks record only the wrapped actions when `--gif` is not provided.
- When `--gif` is provided, the whole script is recorded and nested block recordings are suppressed.
- GIF recording adds a virtual pointer in the browser page. The pointer is visible live during recording and is captured in the GIF frames.
- Pointer-aware actions resolve rich locators to the same target used by browser dispatch, so pointer movement, pointer events, hover state, drag ghosts, screenshots, and captions stay aligned.
- If the script fails, CMG still writes a partial GIF containing frames captured before the failure.

## Trace Behavior

When `--trace` is provided, CMG writes a whole-run trace JSON file even when the script fails after tracing has started. Nested `startTracing` / `stopTracing` actions are suppressed during command-level tracing and emit `TRACE_BLOCK_SUPPRESSED`.

## Stdout

Successful actions write parseable lines:

```text
PASS 001 navigate C:\Projects\CMG\index.html
NAVIGATED 001 file:///C:/Projects/CMG/index.html
PASS 002 waitForElement #openProfileDialog
PASS 003 step Open dialog
PASS 004 setDefaultTimeout 10000
DEFAULT_TIMEOUT 004 10000
PASS 005 screenshot #profileDialog
SCREENSHOT 005 C:\Projects\CMG\profile-dialog.png
PASS 006 evaluate document.title
EVALUATE 006 CMG Browser Control Test Page
GIF C:\Projects\CMG\demo-output\dialog-flow.gif
TRACE C:\Projects\CMG\demo-output\dialog-flow.trace.json
SKIP 007 Feature flag disabled
```

Action-specific payload lines are documented in the [action reference](../../../scripting/actions.md).

## Stderr

Failure output includes the script line number, action, and reason:

```text
Line 4: waitForElement failed. No element matched selector '#missing'.
```

## Exit Codes

- `0`: Script completed successfully.
- `0`: Script stopped with `skip "reason"`.
- `1`: Browser is not running, script cannot be read, script syntax is invalid, or an action fails.

## Example

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
setDefaultAssertionTimeout 5000

step "Open the profile dialog" {
  click "#openProfileDialog"
  waitForElement "#profileDialog[open]"
}

type "#profileName" "CMG Test Profile"
screenshot "#profileDialog" output="profile-dialog.png"
assertText "#lastDialogAction" "None"
```

Run a script with initial variables:

```powershell
cmg browser control script --file demo-scripts\139-cli-variables.cmgscript --var user=Ada
cmg browser control script --file demo-scripts\141-base-url.cmgscript --base-url https://example.test/app/
```

More syntax and action details are documented in the [scripting guide](../../../scripting/index.md). Style guidance is in the [CMG script style guide](../../../scripting/style-guide.md).
