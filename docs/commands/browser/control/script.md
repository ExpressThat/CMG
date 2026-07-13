# `browser control script`

Runs a `.cmgscript` browser automation script against the selected CMG-controlled browser instance. This command is the direct AI/browser-control scripting surface. Use `cmg run` when the same actions should be executed as structured tests with reports, retries, sharding, and per-test traces.

Chrome is the default. Use `--chrome` to select Chrome explicitly, `--edge` for Microsoft Edge, or `--firefox` for Firefox.

```powershell
cmg browser control script --file <path>
cmg browser control script --inline "<script>"
cmg browser --port <port> control script --file <path>
cmg browser control script --file -
cmg browser control script --file <path> --gif <path>
cmg browser control script --file <path> --gif <path> --gif-quality highest
cmg browser control script --file <path> --gif <path> --pointer-duration 600 --pointer-easing ease-in-out
cmg browser control script --file <path> --gif <path> --pointer-theme ring --pointer-color "#dc2626" --pointer-size 44 --pointer-shadow strong
cmg browser control script --file <path> --gif <path> --show-pointer false
cmg browser control script --file <path> --gif <path> --caption-style qa --caption-position bottom --caption-severity success
cmg browser control script --file <path> --gif <path> --click-pulse ripple
cmg browser control script --file <path> --gif <path> --gif-hold-after-action 700
cmg browser control script --file <path> --gif <path> --pointer-pre-click-hold 120 --pointer-post-click-hold 450
cmg browser control script --file <path> --gif <path> --gif-hold-after-navigation 500 --gif-hold-after-assertion 650
cmg browser control script --file <path> --gif <path> --gif-hold-on-failure 1800
cmg browser control script --file <path> --gif <path> --gif-fps 20
cmg browser control script --file <path> --gif <path> --gif-frame-delay 80
cmg browser control script --file <path> --gif <path> --gif-timeline <file-or-directory>
cmg browser control script --file <path> --trace <path>
cmg browser control script --file <path> --timeout 10000 --assertion-timeout 5000
cmg browser control script --file <path> --base-url https://example.test/app/
cmg browser control script --file <path> --var user=Ada --env mode=demo
cmg --chrome browser control script --file <path>
cmg --edge browser control script --file <path>
cmg --firefox browser control script --file <path>
```

## Options

- `--file <path>`: Path to a `.cmgscript` file. Specify exactly one of `--file` or `--inline`.
- `--file -`: Read script text from stdin.
- `--inline <script>`: Run inline `.cmgscript` text. Specify exactly one of `--file` or `--inline`.

For PowerShell automation, prefer `--file <path>` or pipe a here-string to `--file -`. PowerShell parses quotes before CMG receives `--inline`, so nested DSL, CSS, and JavaScript quotes are inherently easier to preserve through a file or stdin.
- `--gif <path>`: Optional output path for an animated GIF recording of the script run.
- `--gif-quality <highest|high|medium|low>`: GIF palette/encoding quality for `--gif`. Defaults to `highest`.
- `--pointer-duration <milliseconds>`: Default virtual pointer movement duration for command-level `--gif` recordings. Must be zero or greater.
- `--pointer-speed <slow|normal|fast|instant|multiplier>`: Default virtual pointer speed for command-level `--gif` recordings. Multipliers use the `1.5x` form. DSL block and action options can still override this.
- `--pointer-easing <linear|ease-in|ease-out|ease-in-out|spring>`: Default virtual pointer easing for command-level `--gif` recordings.
- `--pointer-theme <arrow|hand|dot|ring|branded|touch>`: Default virtual pointer theme for command-level `--gif` recordings.
- `--pointer-color <css-color>`: Default virtual pointer color for command-level `--gif` recordings. Pass one CSS color value, not a CSS declaration.
- `--pointer-size <8..96>`: Default virtual pointer size in CSS pixels for command-level `--gif` recordings.
- `--pointer-shadow <none|light|medium|strong>`: Default virtual pointer shadow strength for command-level `--gif` recordings.
- `--show-pointer <true|false|auto>`: Default virtual pointer visibility for command-level `--gif` recordings. Defaults to `auto`, which currently shows the pointer for pointer-aware frames. Use `false` to capture frames without the DOM pointer; child actions can override with `showPointer=true`.
- `--caption-style <subtle|teaching|qa|bug-report|compact>`: Default caption style for command-level `--gif` recordings.
- `--caption-position <top|bottom|left|right|auto>`: Default caption position for command-level `--gif` recordings.
- `--caption-severity <info|success|warning|error>`: Default caption severity color for command-level `--gif` recordings.
- `--click-pulse <ring|ripple|dot|crosshair|none>`: Default click/tap/drop pulse style for command-level `--gif` recordings. Defaults to `ring`.
- `--gif-hold-after-action <milliseconds>`: Default post-action hold for command-level `--gif` recordings. Defaults to `350`; use `0` to suppress automatic post-action holds.
- `--pointer-pre-click-hold <milliseconds>`: Default hold after pointer movement and before click/tap dispatch in command-level `--gif` recordings. Defaults to `0`.
- `--pointer-post-click-hold <milliseconds>`: Default hold after click/tap pulse frames in command-level `--gif` recordings. Defaults to `350`.
- `--gif-hold-after-navigation <milliseconds>`: Default hold after navigation actions and navigation waits in command-level `--gif` recordings. Defaults to `350`.
- `--gif-hold-after-assertion <milliseconds>`: Default hold after assertion actions in command-level `--gif` recordings. Defaults to `350`.
- `--gif-hold-on-failure <milliseconds>`: Final failure-state hold for command-level `--gif` recordings. Defaults to `1200`; use `0` to suppress the extra failure hold.
- `--gif-fps <1..100>`: Frame rate for command-level `--gif` recordings. Defaults to `10` FPS.
- `--gif-frame-delay <milliseconds>`: Frame delay for command-level `--gif` recordings. Must be `10..10000`; overrides `--gif-fps`.
- `--gif-timeline <file-or-directory>`: Optional JSON metadata sidecar for command-level `--gif`. When a directory is provided, CMG writes `<gif-name>.timeline.json` inside it.
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
- `--file -` fails with `No script text was provided on stdin for --file -.` when stdin is empty.
- Executes actions in file order and stops on the first failed action.
- `skip "reason"` stops the script as skipped, writes `SKIP <line> <reason>` to stdout, and exits `0`.
- Writes step logs and action outputs to stdout.
- Writes validation, parse, browser, and action errors to stderr.
- CDP HTTP, WebSocket, cancellation, and timeout failures are converted into a browser connection error on stderr and exit code `1`; a cold attach cannot fail without a diagnostic result.
- Supports line-level `import "path"` statements. Relative imports resolve from the script file's directory.
- Supports the shared CMG action surface documented in the [action index](../../../scripting/action-index.md) and [action reference](../../../scripting/actions.md).
- Supports control flow, scoped variables, `set` block capture, macros, loops, `try`/`catch`/`finally`, `within`, frame blocks, `step`, `recording` / `withRecording` scoped defaults, and `gif` blocks.
- Relative navigation targets are resolved against `--base-url` before the browser is asked to navigate.
- Initial `--var` and `--env` values are available as `${name}` before the first action, macro call, condition, or `set` block runs.
- `set` is a script action for scoped variables and command-result capture. It is intentionally not a CLI command because it only has meaning inside a script scope.
- Uses the selected browser automation protocol through the active CMG endpoint: Chrome DevTools Protocol for Chrome and Edge, WebDriver BiDi for Firefox.
- Browser diagnostics are armed automatically when CMG launches or attaches to a browser/app. Console messages and page errors accumulate in page-side buffers between CMG commands from that arming point forward. Use `listConsole`, `listPageErrors`, `expectNoConsole`, and `expectNoPageError` after risky interactions. Events that occurred before launch/attach/diagnostics arming cannot be recovered.
- Browser JavaScript dialogs are handled explicitly. CMG does not silently remove, accept, or dismiss dialogs through the browser protocol. Add `captureDialogs` or `setDialogBehavior` before the action that opens an `alert`, `confirm`, or `prompt`.

## GIF Behavior

- When `--gif` is provided, captures the visible page viewport after visual actions and writes an animated GIF.
- GIF quality defaults to `highest`, which uses CMG's most color-faithful palette matching and dithering. Use `high`, `medium`, or `low` only when smaller/faster GIF artifacts matter more than color fidelity.
- The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- Script-level `gif "name" { ... }`, `recordVideo "name" { ... }`, and `screencast "name" { ... }` blocks record only the wrapped actions when `--gif` is not provided.
- When `--gif` is provided, the whole script is recorded and nested block recordings are suppressed.
- GIF recording adds a virtual pointer in the browser page. The pointer is visible live during recording and is captured in the GIF frames.
- Without `--gif` or an active script-level recording block, CMG does not inject the virtual pointer. Recording-only actions such as `pauseGif`, `moveMouse`, `recordCheckpoint`, `showPointer`, and `hidePointer` are skipped instead of creating pointer frames or timeline entries.
- Pointer-aware actions resolve rich locators to the same target used by browser dispatch, so pointer movement, pointer events, hover state, drag ghosts, screenshots, and captions stay aligned.
- DSL recording scopes and blocks can set `autoCaptions=true` and `captionTemplate=`. Automatic captions use privacy-safe text-entry defaults and target-aware `captionPosition=auto`; without an active GIF they do not modify the page.
- DSL recording scopes and blocks can set `intro=`, `outro=`, `introDuration=`, and `outroDuration=`. Explicit `intro` and `outro` actions capture chapter cards. All title-card forms are recording-only and never create a pointer or overlay in non-GIF runs.
- `hideFromGif` / `cutGif` execute child actions without recording frames or pointer UI. `speedUpGif factor=` and `slowDownGif factor=` scale encoded delays locally. These blocks are nestable and execute normally with no pointer when GIF recording is inactive.
- Whole-run pointer, caption, and timing defaults from `--pointer-duration`, `--pointer-speed`, `--pointer-easing`, `--pointer-theme`, `--pointer-color`, `--pointer-size`, `--pointer-shadow`, `--show-pointer`, `--caption-style`, `--caption-position`, `--caption-severity`, `--pointer-pre-click-hold`, `--pointer-post-click-hold`, `--gif-hold-after-action`, `--gif-hold-after-navigation`, `--gif-hold-after-assertion`, `--gif-hold-on-failure`, `--gif-fps`, and `--gif-frame-delay` apply when `--gif` is active. DSL `recording` / `withRecording`, `gif`, `recordVideo`, and `screencast` blocks can set `pointerDuration=`, `pointerSpeed=`, `pointerEasing=`, `pointerTheme=`, `pointerColor=`, `pointerSize=`, `pointerShadow=`, `showPointer=`, `captionStyle=`, `captionPosition=`, `captionSeverity=`, `clickPulse=`, `preClickHold=`, `postClickHold=`, `holdAfterAction=`, `holdAfterNavigation=`, `holdAfterAssertion=`, `holdOnFailure=`, `fps=`, and `frameDelay=` as scoped defaults for child actions; child actions can override action options locally.
- If the script fails, CMG still writes a partial GIF containing frames captured before the failure.
- On failure, command-level GIF recording captures one extra final-state hold frame before writing the partial GIF unless `--gif-hold-on-failure 0` is used.
- `--gif-timeline` writes a JSON sidecar after the GIF is saved and emits `GIF_TIMELINE <path>` on stdout. The sidecar includes the GIF path, file size, dimensions, frame count, frame delays, total duration, quality, and recorder timing settings.

## Trace Behavior

When `--trace` is provided, CMG writes a whole-run trace JSON file even when the script fails after tracing has started. Nested `startTracing` / `stopTracing` actions are suppressed during command-level tracing and emit `TRACE_BLOCK_SUPPRESSED`.

## Stdout

Successful actions write parseable lines:

```text
PASS 001 line=1 action=navigate C:\Projects\CMG\index.html
NAVIGATED 001 file:///C:/Projects/CMG/index.html
PASS 002 line=2 action=waitForElement #openProfileDialog
PASS 003 line=3 action=step Open dialog
PASS 004 line=4 context="step Open dialog" action=click #openProfileDialog
PASS 005 line=5 action=setDefaultTimeout 10000
DEFAULT_TIMEOUT 005 10000
PASS 006 line=6 action=screenshot #profileDialog
SCREENSHOT 006 C:\Projects\CMG\profile-dialog.png
PASS 007 line=7 action=evaluate document.title
EVALUATE 007 CMG Browser Control Test Page
GIF C:\Projects\CMG\demo-output\dialog-flow.gif
GIF_TIMELINE C:\Projects\CMG\demo-output\dialog-flow.timeline.json
GIF_PAUSE 008 milliseconds=800 status=captured
TRACE C:\Projects\CMG\demo-output\dialog-flow.trace.json
SKIP 007 Feature flag disabled
```

`PASS` sequence numbers increase globally for the whole script and include `line=<line> action=<action>`. Nested output from a macro, loop, `step`, retry, or handled branch also includes `context="..."`. Payload lines stay compact at top level and use the same sequence as their action when nested context metadata is present. Runner JSON/HTML reports expose sequence, source line, context, and action as structured fields for every step.

Action-specific payload lines are documented in the [action reference](../../../scripting/actions.md).

## Stderr

Failure output includes the script line number, action, and reason:

```text
Line 4: waitForElement failed. No element matched selector '#missing'.
Line 8: click failed in macro login > repeat[2/3]. No element matched selector '#save'.
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
cmg browser control script --file demo-scripts\148-gif-quality.cmgscript --gif demo-output\quality.gif --gif-quality highest
cmg browser control script --file demo-scripts\149-gif-pointer-choreography.cmgscript --gif demo-output\pointer-choreography.gif --pointer-duration 500 --pointer-pre-click-hold 120 --pointer-post-click-hold 450
cmg browser control script --file demo-scripts\156-gif-pointer-styles.cmgscript --gif demo-output\pointer-styles-whole-run.gif --pointer-theme branded --pointer-color "#2563eb"
cmg browser control script --file demo-scripts\157-gif-caption-styles.cmgscript --gif demo-output\caption-styles-whole-run.gif --caption-style bug-report --caption-position bottom
cmg browser control script --file demo-scripts\150-gif-failure-hold.cmgscript --gif demo-output\failure-hold.gif --gif-hold-on-failure 1800 --gif-timeline demo-output\timelines
cmg browser control script --inline "listConsole level=error"
```

More syntax and action details are documented in the [scripting guide](../../../scripting/index.md). Style guidance is in the [CMG script style guide](../../../scripting/style-guide.md).
