# `.cmgscript` Errors

Scripts stop on the first error.

When `--gif <path>` is used, CMG still writes a partial GIF with the frames captured before the error, as long as recording had started.

Browser JavaScript dialogs are handled explicitly. Add `captureDialogs` or `setDialogBehavior` before the action that opens an `alert`, `confirm`, or `prompt`; otherwise a real browser dialog can block the page until the browser or protocol reports an error.

## Browser Not Running

```text
No CMG-controlled Chrome instance is running. Run 'cmg browser launch' first.
```

For Firefox, the equivalent error is:

```text
No CMG-controlled Firefox instance is running. Run 'cmg --firefox browser launch' first.
```

For Edge, the equivalent error is:

```text
No CMG-controlled Edge instance is running. Run 'cmg --edge browser launch' first.
```

Start the selected browser first:

```powershell
cmg browser launch
cmg --edge browser launch
cmg --firefox browser launch
```

## Missing Script File

```text
Script file 'flow.cmgscript' was not found.
```

Check the path passed to `--file`.

## Navigation Failed

Invalid URLs and missing path-like local targets fail the `navigate`, `goto`, and `visit` actions:

```text
Line 1: navigate failed. Cannot navigate to invalid URL
```

```text
Line 1: navigate failed. Navigation target path 'C:\Projects\CMG\missing.html' was not found.
```

For local files, pass an existing path. For web pages, pass a valid URL including the scheme, such as `https://example.com`.

## Invalid Syntax

```text
Line 3: unterminated quoted string.
```

Common causes:

- Missing closing quote.
- Starting an inline comment before required tokens on the same physical line.
- Forgetting to quote arguments with spaces.
- Opening a block without a closing `}`.
- Closing a block with `}` when no block is open.

## Invalid Block Action

```text
Line 4: dragAndDrop failed. Block dragAndDrop requires a drop action.
```

Complex `dragAndDrop` blocks must contain exactly one `drop` action and no actions after it.

Unsupported child actions fail clearly:

```text
Line 4: dragAndDrop failed. Action 'type' is not supported inside block dragAndDrop.
```

## Unknown Action

```text
Line 2: doThing failed. Unknown action 'doThing'.
```

Check [actions.md](actions.md) for the supported action list.

## Unsupported Runner Action

Unknown `cmg run` actions fail explicitly:

```text
Line 4: evaluate failed. Unsupported CMG action 'futureAction'. See docs/scripting/actions.md for supported actions.
```

The action failed because the DSL action name is unsupported, not because the page behaved incorrectly.

Invalid locator matches fail before the pointer action runs:

```text
STEP FAIL line=7 action=click reason=Line 7: evaluate failed. No element matched locator role=button.
STEP FAIL line=8 action=click reason=Line 8: evaluate failed. Locator nth= requires <selector>|<index>.
STEP FAIL line=9 action=click reason=Line 9: evaluate failed. Locator hasText= requires <selector>|<text>.
STEP FAIL line=10 action=click reason=Line 10: evaluate failed. Locator has= requires <selector>|<child-selector>.
STEP FAIL line=11 action=click reason=Line 11: evaluate failed. Locator hasNot= requires <selector>|<child-selector>.
STEP FAIL line=12 action=click reason=Line 12: evaluate failed. Locator hasNotText= requires <selector>|<text>.
STEP FAIL line=13 action=click reason=Line 13: evaluate failed. Locator roleRegex= requires <role>|<name-regex>.
STEP FAIL line=14 action=click reason=Line 14: evaluate failed. Locator shadow= requires <host-selector>|<inner-selector>.
STEP FAIL line=15 action=click reason=Line 15: evaluate failed. Locator or= requires <selector>|<selector>.
STEP FAIL line=16 action=click reason=Line 16: evaluate failed. Locator strict= expected exactly one match for .item, got 2.
STEP FAIL line=17 action=click reason=Line 17: evaluate failed. Locator inside= requires <container-selector>|<target-selector>.
STEP FAIL line=18 action=click reason=Line 18: evaluate failed. Locator closest= requires <child-selector>|<ancestor-selector>.
STEP FAIL line=19 action=click reason=Line 19: evaluate failed. Locator parent= requires <child-selector>.
```

`within` resolves its container before running child actions. Missing or unmatched containers fail before any scoped child action runs:

```text
Line 2: within failed. Expected 1 positional argument(s), got 0.
Line 2: within failed. No element matched locator text=Dialog
```

## Top-Level Action Script Passed To `cmg run`

```text
TEST FAIL 01-dialog-flow.cmgscript
TEST ERROR 01-dialog-flow.cmgscript reason=Line 1: cmg run requires structured tests with test/it/specify or suite/describe/context blocks. Direct browser-control scripts should use browser control script --file; see docs/scripting/migration.md.
```

The parser error itself is:

```text
Line 1: cmg run requires structured tests with test/it/specify or suite/describe/context blocks. Direct browser-control scripts should use browser control script --file; see docs/scripting/migration.md.
```

Use `browser control script --file <path>` for direct browser-control scripts, or migrate the file to the structured test format for `cmg run`.

Use `cmg run <path> --list` to validate structured script syntax and inspect selected tests without connecting to a browser or running actions. Parse/import errors still fail the command.

## Runner Step Failure

`cmg run` prints step diagnostics on stderr:

```text
STEP FAIL line=8 action=click reason=Line 8: click failed. No element matched selector '#missing'.
RUN STOP maxFailures=1
```

JSON and HTML reports include the same reason, the test name, output lines, and any GIF path connected to the failure.

Use `cmg run --trace <directory>` to write per-test trace JSON with every recorded step and failure reason.

`RUN STOP maxFailures=<count>` means `cmg run --max-failures <count>` reached its failure threshold and stopped scheduling more tests. Reports, traces, and GIF output include tests that ran before the stop.

## Invalid Dialog Behavior

```text
Line 3: handleDialog failed. setDialogBehavior expects accept or dismiss.
```

`setDialogBehavior`, `onDialog`, `handleDialog`, and `dialogBehavior` all require `accept` or `dismiss`. Add `promptText=<text>` when accepting a prompt should return custom text.

## Invalid Shard

```text
--shard must use index/count with 1 <= index <= count.
```

Examples of valid values: `1/2`, `2/2`, `3/5`.

## Missing Element

```text
Line 4: click failed. No element matched selector '#missing'.
```

Use `waitForElement` before actions that require an element.

## `set` Block Without Output

```text
Line 3: set failed. set 'title' block did not produce output.
```

The block form stores the payload from the last output-producing action in the block. Wrap an action such as `evaluate`, `url`, `title`, `content`, `html`, `readClipboard`, `waitForResponse`, or another action that writes a parseable output line.

## Explicit Failure

```text
Line 4: fail failed. Missing required setup
```

`fail "message"` intentionally stops the current direct script or `cmg run` test with the supplied reason. Inside `try`, it triggers the matching `catch` block and the catch variable receives the full failure text.

## Element Outside Viewport

User-like actions such as `click`, `tap`, `touchTap`, `type`, `clear`, `hover`, `select`, and `dragAndDrop` do not scroll automatically:

```text
Line 3: click failed. Element '#dropQueue' is outside the current viewport. Run scrollIntoView first if this movement should scroll the page.
```

Add an explicit `scrollIntoView "<selector>"` step before the action. For drag-and-drop, make sure the source and target can both fit in the current viewport, or use `setViewport` before scrolling.

## Not Actionable

```text
STEP FAIL line=8 action=click reason=Line 8: evaluate failed. Element #save was not actionable before timeout.
```

Selector actions wait for visibility, enabled state, and stable layout before the pointer action runs. Increase `timeout=` or wait for the page state that enables the element.

## Invalid `tap` Target

```text
Line 3: tap failed. No element matched selector '#missing'.
Line 4: tap failed. No element at tap target.
```

Use an existing selector, a rich locator that resolves to one element, or viewport coordinates that land on a page element:

```text
tap "#save"
tap text=Save
tap x=120 y=240
```

## Invalid Clipboard Action

```text
Line 3: setClipboard failed. Expected 1 argument but got 0.
Line 4: readClipboard failed. Expected 0 arguments but got 1.
```

`setClipboard` and `writeClipboard` require exactly one text argument. `readClipboard` and `clearClipboard` do not accept arguments.

## Recording-Only Action Without Active Recording

```text
GIF_MOVE_MOUSE 003 status=skipped reason=no-active-recording
GIF_PAUSE 004 status=skipped reason=no-active-recording
GIF_CHECKPOINT 005 status=skipped reason=no-active-recording
GIF_SHOW_POINTER 006 status=skipped reason=no-active-recording
GIF_HIDE_POINTER 007 status=skipped reason=no-active-recording
GIF_REDACT 008 status=skipped reason=no-active-recording
GIF_UNREDACT 009 status=skipped reason=no-active-recording
```

`moveMouse`, `pauseGif`, `recordCheckpoint`, `showPointer`, `hidePointer`, `maskGif` / `redactGif` / `redactText`, and `unmaskGif` / `unredactGif` are recording-only actions. When command-level `--gif` is not active and the action is outside a `gif`, `recordVideo`, or `screencast` block, CMG skips it instead of injecting a virtual pointer or writing timeline metadata. This is not a failure. In that skipped state, recording-only arguments, variables, scoped selectors, options, and child bodies are ignored because there is no active recording to apply them to.

## GIF Redaction Safety Failure

```text
Line 2: gif failed. Error: GIF redaction safety blocked capture: 1 visible password field(s) are not masked.
```

`redactionSafety=strict` refuses to capture a frame if a visible password input is not covered. Keep the default `autoRedact=passwords`, use a broader `tokens`, `emails`, `payment`, or `privacy` preset, or add an explicit `redact=` / `maskGif` rule. CMG does not attempt an unredacted failure screenshot after this error and writes no GIF when no safe frame exists.

## Invalid `moveMouse` Target

```text
Line 3: moveMouse failed. Unknown moveMouse alias 'lower'. Supported aliases: center, top, bottom, left, right, topLeft, topRight, bottomLeft, bottomRight.
```

Use a supported alias or pass viewport-relative coordinates:

```text
moveMouse "bottom"
moveMouse x=100 y=200
moveMouse selector=".content-area" edge=bottom
```

Coordinates outside the visible viewport fail:

```text
Line 3: moveMouse failed. moveMouse target (2000, 2000) is outside the current viewport 1280x720.
```

Element-edge targeting requires both a selector and an edge:

```text
Line 3: moveMouse failed. moveMouse element-edge targeting requires edge=<top|bottom|left|right|center|topLeft|topRight|bottomLeft|bottomRight>.
```

## Invalid Pointer Evidence Option

```text
Line 3: click failed. click option pointerContrast= must be one of: auto, fixed.
Line 4: hover failed. hover option targetCallout= must be one of: auto, always, none.
Line 5: pauseGif failed. pauseGif option pointerIdleThreshold= must be an integer from 100 to 60000.
Line 6: mouseDown failed. mouseDown option mouseDownHold= must be an integer from 0 to 60000.
```

Pointer-evidence options are validated when an active GIF recorder applies them. Recording scopes and blocks use the same bounds as action overrides and whole-run CLI flags. Without an active recorder, recorder-only visual settings remain inert and do not inject overlays.

## Invalid Emulation

```text
Line 3: emulate failed. emulate requires both width and height when overriding viewport.
Line 4: emulate failed. geolocation must be '<latitude>,<longitude>'.
Line 5: emulateMedia failed. emulateMedia option media= must be screen, print.
Line 6: setGeolocation failed. setGeolocation requires '<latitude>,<longitude>' or latitude=<value> longitude=<value>.
Line 7: grantPermissions failed. grantPermissions requires at least one permission name.
Line 8: setViewport failed. setViewport option hasTouch= must be true or false.
Line 9: viewport failed. viewport expects width=<pixels> height=<pixels> or '<width>' '<height>'.
```

Provide both viewport dimensions together, pass media options from the documented value sets, pass boolean viewport options as `true` or `false`, pass geolocation as a comma-separated latitude/longitude pair or latitude/longitude options, and grant at least one permission name.

## Download Timeout

```text
Line 6: download failed. No download matching '*.csv' appeared in 'C:\Projects\CMG\demo-output' within 10000ms.
```

Check the browser download directory, file pattern, and timeout. `download` ignores in-progress Chrome `.crdownload` files.

## Screenshot Failure

```text
Line 4: screenshotPage failed. screenshotPage option fullPage= must be true or false.
Line 5: screenshotPage failed. screenshotPage option type= must be png, jpeg, or jpg.
Line 6: screenshot failed. screenshot option quality= must be between 0 and 100.
Line 7: screenshot failed. screenshot option quality= is only valid when type=jpeg.
Line 8: screenshotPage failed. Firefox did not return screenshot image data.
```

Use `fullPage=true` for a full scrollable page capture. Use `type=jpeg quality=80` for a JPEG capture. Use `fullPage=false` or leave it unset for the current viewport.

## Console Timeout

```text
Line 7: waitForConsole failed. Console message 'settings saved' was not seen within 5000ms.
Line 8: waitForConsole failed. Invalid text regex '[': ...
```

CMG arms console diagnostics automatically when it launches or attaches to a controlled browser/app. Check that the message happened after launch/attach/diagnostics arming, and check the optional `level=` filter. Console text matching supports `match=contains|exact|regex` and `ignoreCase=true`.

For visual diagnostics, interact with the page, then run `listConsole level=error`, `listPageErrors`, `expectNoConsole level=error timeout=250`, and `expectNoPageError timeout=250`. A matching console error fails the script and includes the captured text in the failure reason. Use `waitForConsole "." level=error match=regex timeout=250` when the caller wants to retrieve one matching error line as `CONSOLE <line> error: <text>`. Capture is forward-only; events before launch/attach/arming cannot be recovered.

## Unexpected Console Output

```text
Line 8: expectNoConsole failed. Unexpected console error: Save failed
```

Use `listConsole level=error` to inspect captured errors, then use `level=`, optional text, `match=`, `ignoreCase=`, and `timeout=` to narrow the rejected messages. `captureConsole` is a deprecated compatibility action for ensuring capture is installed; it does not clear existing entries.

Pair console checks with page-error checks when diagnosing browser UI failures:

```text
click "#risky"
screenshotPage output="artifacts/after-click.png"
listPageErrors
listConsole level=error
expectNoPageError timeout=250
expectNoConsole level=error timeout=250
```

## Page Error Timeout

```text
Line 8: waitForPageError failed. Page error 'Cannot read' was not seen within 5000ms.
Line 9: waitForPageError failed. waitForPageError option match= must be contains, exact, or regex.
```

CMG arms page-error diagnostics automatically when it launches or attaches to a controlled browser/app. Check that the error happened after launch/attach/diagnostics arming, and match text from the page error or rejected value. Page-error matching supports `match=contains|exact|regex` and `ignoreCase=true`.

For visual diagnostics, interact with the page, then run `listPageErrors`, `listConsole level=error`, `expectNoPageError timeout=250`, and `expectNoConsole level=error timeout=250`. A matching page error fails the script and includes the captured text in the failure reason. Use `waitForPageError "." match=regex timeout=250` when the caller wants to retrieve one matching error line as `PAGE_ERROR <line> <type>: <text>`. Capture is forward-only; events before launch/attach/arming cannot be recovered.

## Unexpected Page Error

```text
Line 9: expectNoPageError failed. Unexpected page error: Cannot read properties of null
```

Use `listPageErrors` to inspect captured errors, then use optional text, `match=`, `ignoreCase=`, and `timeout=` to narrow the rejected page errors. `capturePageErrors` is a deprecated compatibility action for ensuring capture is installed; it does not clear existing entries.

## Init Script Failure

```text
Line 2: addInitScript failed. addInitScript requires inline script text or path=<file>.
Line 3: addInitScript failed. Init script file 'C:\Projects\CMG\fixtures\init.js' was not found.
```

Pass one inline script argument or `path=<file>`. Register init scripts before navigating to the page that should receive them.

## Frame Failure

```text
Line 8: frameClick failed. Iframe #checkoutFrame is not same-origin or is not ready.
Line 9: frameAssertText failed. Expected frame text to match ^Saved$ using regex, got Saving.
Line 10: frameAssertText failed. Invalid text regex '[': ...
Line 11: frame failed. Expected 1 positional argument(s), got 0.
```

Frame actions and `frame { ... }` / `frameLocator { ... }` blocks require a same-origin iframe because they run through page JavaScript. Frame text assertions support `match=contains|exact|regex` and `ignoreCase=true`. Use tab or browser-context actions for cross-origin flows that open in their own page target.

## Clock Failure

```text
Line 5: tick failed. Clock is not installed. Run clock now=<epoch-ms> first.
Line 6: tick failed. 'tick' must be a positive whole number.
```

Install the fake clock before advancing time and pass non-negative millisecond values to `tick`.

## Context Cleanup Failure

```text
Line 2: resetContext failed. JavaScript evaluation failed.
```

Context cleanup runs in the current page. If the page blocks same-origin storage APIs or browser APIs are unavailable, the failing browser evaluation reason is reported in stderr, JSON reports, HTML reports, and traces.

## Browser Context Failure

```text
Line 3: useContext failed. Browser context 'ctx-2' was not found.
Line 4: closeContext failed. Browser context 'ctx-2' was not found.
```

Use a context id or target id from `newContext`, `listContexts`, or `browser control context browserContexts list`. In scripts, prefer storing the id with `newContext ctx` and referencing it through `${ctx}`.

## Accessibility Failure

```text
Line 6: expectAccessible failed. expectAccessible requires role=<role>.
Line 7: expectAccessible failed. No accessible element matched role=button name=Save.
```

Use `accessibilitySnapshot output="..."` to inspect the derived role/name tree when an assertion fails.

## Failed Assertion

```text
Line 5: assertText failed. Expected text 'Ready' was not found. Actual text: 'Loading'.
Line 6: expectNoText failed. Expected text 'Error' was still found. Actual text: 'Ready Error'.
```

Positive text assertions check whether the element text matches the expected text. Negative text assertions check that matching text is absent. `match=contains|exact|regex` and `ignoreCase=true` are supported. Timeout failures include the timeout and the last text observed.

## API Request Failure

```text
STEP FAIL line=3 action=apiRequest reason=Expected status 200, got 500.
STEP FAIL line=4 action=apiRequest reason=apiRequest option timeout= must be a positive number of milliseconds.
STEP FAIL line=5 action=apiRequest reason=Expected ok=true, got status 500.
STEP FAIL line=6 action=apiRequest reason=Expected response body not to contain 'secret'.
STEP FAIL line=7 action=apiRequest reason=Expected response header 'Content-Type' to contain 'json'.
STEP FAIL line=8 action=apiRequest reason=apiRequest option ok= must be true or false.
```

`apiRequest` failures include the reason in stderr and reports. The step output includes `API` and `API_BODY` lines when a response was received, or `API_BODY_FILE` when `output=` wrote the response body to disk.

## Network Wait Failure

```text
STEP FAIL line=6 action=waitForResponse reason=Line 6: evaluate failed. Timed out waiting for response /api/profile.
```

Install a matching `route` or make sure the page performs a matching `fetch()` before the timeout expires.

## Network Environment Failure

```text
Line 4: setExtraHTTPHeaders failed. setExtraHTTPHeaders requires one or more <name> <value> header pairs.
Line 5: setOffline failed. setOffline expects true or false.
```

Pass headers as quoted name/value pairs and pass `true` or `false` to `setOffline`.

## Navigation Failure

```text
Line 2: waitForUrl failed. URL did not match /checkout within 5000ms using contains match. Last URL: https://example.com/cart
Line 2: toHaveURL failed. Expected URL to match /checkout using exact match, got https://example.com/cart.
Line 2: toHaveTitle failed. Expected title to match Checkout using exact match, got Cart.
Line 2: expectUrl failed. expectUrl option match= must be contains, exact, or regex.
Line 2: expectTitle failed. Invalid navigation regex '[': Invalid pattern '[' at offset 1. Unterminated [] set.
Line 3: goto failed. goto waitUntil= expects load, domcontentloaded, networkidle, or commit.
Line 4: reload failed. reload waitUntil= expects load, domcontentloaded, networkidle, or commit.
Line 5: goForward failed. goForward waitUntil= expects load, domcontentloaded, networkidle, or commit.
Line 6: waitForLoadState failed. waitForLoadState expects loading, interactive, complete, load, or networkidle.
Line 7: waitForNavigation failed. waitForNavigation waitUntil= expects load, domcontentloaded, networkidle, or commit.
Line 8: waitForNavigation failed. Navigation did not reach load within 5000ms. Last URL: https://example.com/cart; state: loading
```

Navigation wait failures include the expected state or URL, timeout, and the last observed browser state when available.

## Storage Failure

```text
Line 4: localStorage failed. localStorage set expects a key and value.
Line 5: cookie failed. cookie expects get, set, remove, or clear.
Line 6: cookie failed. cookie sameSite expects Strict, Lax, or None.
```

Storage actions validate the operation, required key/value arguments, and supported cookie options before mutating browser state.

## Wait Failure

```text
Line 6: waitForFunction failed. waitForFunction did not become truthy within 5000ms.
Line 7: waitForTimeout failed. delay must be a positive integer.
Line 8: setDefaultTimeout failed. setDefaultTimeout= must be zero or greater.
```

Wait failures include whether the selector, expression, or timeout argument caused the failure.

## Emulation Failure

```text
Line 2: emulate failed. Unknown device 'Pocket Fridge'. Known devices: iPhone 13, iPhone SE, Pixel 5, Pixel 7, Galaxy S9+, iPad, iPad Pro, Desktop Chrome.
```

Emulation failures include invalid viewport shapes, invalid page-environment values, and unknown device preset names.

## Trace Failure

```text
Line 8: stopTracing failed. Tracing is not active.
Line 9: stopTracing failed. stopTracing requires path= or output= when startTracing did not set one.
```

Trace failures explain whether tracing was missing, already active, or missing an output path. If `startTracing` included `path` or `output`, CMG writes a partial trace when a later action fails.

## Mouse Failure

```text
Line 4: mouseMove failed. mouseMove requires either one alias argument or x=<pixels> y=<pixels> options.
Line 5: mouseDown failed. mouseDown target (900, 900) is outside the current viewport 800x600.
Line 6: scrollBy failed. x must be a whole number.
Line 7: scrollTo failed. scrollTo alias must be top, bottom, left, or right.
```

Mouse actions validate target shape and viewport bounds before sending pointer events.

Scroll actions validate whole-number coordinates and known aliases before changing the page. `scrollTo`, `scrollBy`, and `wheel` selector targets also report the rich-locator or CSS selector that could not be matched.

## Keyboard Failure

```text
Line 3: keyDown failed. Expected 1 positional argument(s), got 0.
Line 4: insertText failed. Expected 1 positional argument(s), got 0.
Line 5: keyboardShortcut failed. keyboardShortcut expects a key chord such as Control+S.
```

Keyboard actions require one key, text, or shortcut chord argument and operate on the current browser focus. Chords use `+`, for example `Control+S` or `Control+Shift+P`.

## Dialog Failure

```text
Line 4: setDialogBehavior failed. setDialogBehavior expects accept or dismiss.
Line 5: waitForDialog failed. Timed out waiting for dialog Saved
Line 6: waitForDialog failed. Invalid text regex '[': ...
```

Dialog waits match against captured `alert`, `confirm`, and `prompt` messages. Install `captureDialogs` before triggering the dialog. Dialog matching supports `match=contains|exact|regex` and `ignoreCase=true`.

## Event Wait Failure

```text
Line 3: waitForEvent failed. waitForEvent supports popup, page, tab, download, dialog, console, pageError, request, requestFinished, requestFailed, and response.
Line 4: waitForEvent failed. waitForEvent console requires a matcher argument or pattern= option.
```

`waitForEvent` maps provider-style event names to CMG's explicit waits. Matcher-based events require either a second argument or one of `pattern=`, `text=`, `message=`, or `url=`.

## Exposed Function Failure

```text
Line 2: exposeFunction failed. exposeFunction requires a valid JavaScript identifier name.
Line 3: exposeBinding failed. CMG exposed function cmgBinding must evaluate to a function.
```

Exposed page functions require a valid `window` property identifier and a JavaScript function expression.

## Network Wait Failure

```text
Line 5: waitForRequest failed. Timed out waiting for request /api/profile
Line 6: waitForRequestFinished failed. Timed out waiting for finished request /api/profile
Line 7: waitForRequestFailed failed. Timed out waiting for failed request /api/profile
Line 8: waitForResponse failed. Timed out waiting for response /api/profile
Line 9: route failed. route option times= must be a positive integer.
Line 10: intercept failed. intercept option delay= must be a non-negative integer.
Line 11: waitForResponse failed. waitForResponse option match= must be contains, exact, or regex.
Line 12: waitForRequest failed. Invalid network regex '[': Invalid pattern '[' at offset 1. Unterminated [] set.
Line 13: route failed. route option ignoreCase= must be true or false.
Line 14: route failed. route option header= headers must be formatted as Name: value.
Line 15: route failed. route body file 'fixtures/profile.json' was not found.
```

Routes and network waits match page `fetch()` and `XMLHttpRequest` URLs recorded by CMG's page-side network patch. Use `match=contains|exact|regex` and `ignoreCase=true` when substring matching is not specific enough. `waitForRequestFinished` waits for completed responses. `waitForRequestFailed` waits for rejected `fetch()` calls or XHR `error`, `abort`, or `timeout` events.

## WebSocket Failure

```text
Line 4: routeWebSocket failed. routeWebSocket option close= must be true or false.
Line 5: waitForWebSocketMessage failed. Timed out waiting for websocket message ready
Line 6: waitForWebSocket failed. Invalid network regex '[': ...
```

WebSocket waits use `match=contains|exact|regex` against recorded socket URLs and message data. Install `routeWebSocket` before creating sockets that should receive synthetic route behavior.

## Network Environment Failure

```text
Line 4: setHttpCredentials failed. setHttpCredentials username cannot be empty.
Line 5: setExtraHTTPHeaders failed. setExtraHTTPHeaders requires one or more <name> <value> header pairs.
Line 6: setProxy failed. setProxy proxy prefix cannot be empty.
```

Network environment actions patch page-originated `fetch()` and `XMLHttpRequest` calls. Browser-level navigation requests are not rewritten by these actions.

## Browser Environment Failure

```text
Line 3: setJavaScriptEnabled failed. setJavaScriptEnabled expects true or false.
Line 4: bypassCSP failed. bypassCSP expects true or false.
Line 5: serviceWorkers failed. serviceWorkers expects allow or block.
```

Browser environment actions are page-side hooks. They do not alter the already-running browser process or browser launch flags.

## Worker Failure

```text
Line 4: waitForWorker failed. Worker 'worker.js' was not available within 5000ms.
Line 5: workerEvaluate failed. No worker matched 'analytics-worker'.
Line 6: workerIntercept failed. Worker '<first>' was not available.
Line 7: workerIntercept failed. workerIntercept body file 'fixtures/profile.json' was not found.
```

Use `listWorkers` to inspect available worker ids and URLs before targeting a specific worker. Worker interception patches `fetch()` inside existing worker targets and uses the same URL match, body-file, and response-header validation as page routes.

## Coverage Failure

```text
Line 3: startCoverage failed. startCoverage option js= must be true or false.
Line 7: stopCoverage failed. JavaScript evaluation failed.
```

Call `startCoverage` before the actions that should be measured and `stopCoverage` after them. If the page blocks script or stylesheet inspection, CMG reports the browser evaluation failure.

## HAR Failure

```text
Line 4: replayHar failed. HAR file 'missing.har' was not found.
Line 5: exportHar failed. exportHar requires path=<file>.
```

Pass `path=<file>` to both HAR actions. `exportHar` writes captured page-level `fetch` and `XMLHttpRequest` traffic; `replayHar` installs routes from that file.

## File Action Failure

```text
Line 3: readFile failed. File 'fixtures\payload.json' was not found.
Line 4: writeFile failed. writeFile requires path=<file>.
Line 5: expectFile failed. Expected file 'demo-output\result.txt' to contain 'Done'.
```

Pass `path=<file>` to every file action. For `readFile` and `fixture`, the first argument is the variable name that receives the file content. `expectFile contains="..."` reports the expected text when the file exists but does not match.

## PDF Failure

```text
Line 4: printPdf failed. printPdf requires path=<file>.
Line 5: printPdf failed. printPdf option scale= must be a positive number.
Line 6: printPdf failed. printPdf option format= must be Letter, Legal, Tabloid, Ledger, or A0-A6.
Line 7: printPdf failed. printPdf option width= must be a positive size using in, cm, mm, px, or a bare inch value.
Line 8: printPdf failed. Firefox did not return PDF data.
```

PDF generation is available in both direct scripts and `cmg run`. Invalid PDF options fail before the browser call with the specific option name. If the browser provider does not return PDF bytes, CMG reports that provider failure directly.

## Visual Assertion Failure

```text
STEP FAIL line=5 action=expectScreenshot reason=Screenshot diff 0.0321 exceeded tolerance 0.01.
STEP FAIL line=6 action=toHaveScreenshot reason=Baseline 'dialog.png' did not exist. Created it from the actual screenshot.
STEP FAIL line=7 action=expectScreenshot reason=expectScreenshot option fullPage= must be true or false.
```

The `VISUAL_ACTUAL` output line points to the actual PNG written for review. If the baseline file is missing, CMG creates it from the actual screenshot and fails with a message saying the baseline was created. Mask selectors are resolved before comparison; missing mask elements fail the step with the selector-specific reason.

## UI State Assertion Failure

```text
STEP FAIL line=7 action=expectVisible reason=Expected element to be visible.
STEP FAIL line=8 action=expectAttribute reason=Expected attribute aria-label to contain Save, got Cancel.
```

`expectVisible`, `expectHidden`, `expectEnabled`, `expectDisabled`, `expectValue`, `expectAttribute`, `expectChecked`, and `expectCount` run in the page and report the expected state plus the actual state that caused the failure.

## Evaluated Assertion Failure

```text
Line 4: expectEval failed. Expected evaluated value to be truthy. Actual: 'false'.
Line 5: assertExpression failed. Expected evaluated value to equal 'Saved' within 5000ms. Actual: 'Saving'.
```

Use `equals=`, `eq=`, or `contains=` when truthiness is not specific enough. Add `timeout=<milliseconds>` when the page expression should be polled until it reaches the expected state.

## Storage State Failure

```text
STEP FAIL line=4 action=storageState reason=Storage state file 'auth.json' was not found.
```

Use `storageState save path="..."` before loading a state file.

## File Upload Failure

```text
STEP FAIL line=6 action=uploadFiles reason=Upload file 'fixtures\avatar.png' was not found.
STEP FAIL line=7 action=setInputFiles reason=Upload file 'fixtures\avatar.png' was not found.
```

Check that every file path passed after the selector exists from the process working directory, or use absolute paths. A missing selector or missing file argument fails before any browser interaction runs. `setInputFiles` and `selectFile` report the same upload failure reasons as `uploadFiles`.

## Undefined Variable

```text
Line 3: click failed. Variable 'target' is not defined.
```

Define variables before using them:

```text
set target "#openProfileDialog"
click "${target}"
```

## Import Failure

```text
Imported script 'C:\Projects\CMG\demo-scripts\missing.cmgscript' was not found.
Import cycle detected for 'C:\Projects\CMG\demo-scripts\shared.cmgscript'.
Invalid import syntax 'import shared.cmgscript'. Use import "path".
```

Imports are expanded before parsing. Quote the path, keep relative paths relative to the importing file, and avoid cycles between shared helper files.

## Control Flow And Macro Failure

```text
Line 6: elseif failed. elseif must follow an if block.
Line 8: call failed. Macro 'login' is not defined.
Line 3: macro failed. macro requires a block body.
Line 5: call failed. Macro 'fillProfile' expects 2 argument(s), got 1.
Line 7: for failed. for option step= cannot be 0.
Line 8: while failed. while exceeded max=100 iteration(s).
Line 8: until failed. until exceeded max=100 iteration(s).
Line 8: doWhile failed. doWhile exceeded max=100 iteration(s).
Line 9: retry failed. retry requires a block body.
Line 9: retry failed. retry max must be greater than 0.
Line 9: retry failed. retry exhausted 3 attempt(s). Last error: Line 10: assertText failed. Expected text 'Ready' was not found. Actual text: 'Waiting'.
Line 9: toPass failed. toPass requires a block body.
Line 9: toPass failed. toPass max must be greater than 0.
Line 9: toPass failed. toPass exhausted 3 attempt(s). Last error: Line 10: expectText failed. Expected text 'Ready' was not found. Actual text: 'Waiting'.
break must be inside a loop.
continue must be inside a loop.
Line 10: catch failed. catch must follow a try block.
Line 12: try failed. catch must appear before finally.
Line 9: if failed. Invalid action condition 'assertText'.
Line 4: case failed. case must follow a switch block.
Line 6: switch failed. switch can contain only case or default blocks.
Line 7: switch failed. switch can have only one default block.
```

Define macros before calling them. Top-level macros in `cmg run` are registered for each test; macros declared inside another macro, branch, or loop are scoped to that block and are not available afterward.

Loop variables and macro parameters are scoped to the loop iteration or macro call. `if`, `elseif`, `while`, `until`, `doWhile`, `doUntil`, and `switch` comparisons support `contains`, `matches`, and `in` in addition to numeric/string comparison operators. Inline actions in conditions are true when they succeed; value-producing actions can be compared by payload. Use `set` when a value must be available after the block completes.

`retry` and `toPass` rerun a failing child block until it succeeds or the attempt limit is exhausted. Failed attempts are written to stdout with `RETRY <line> attempt=<n> failed=<reason>` or `TO_PASS <line> attempt=<n> failed=<reason>`, and an exhausted block reports the final child-action reason.

`try` can recover from expected action failures with `catch`. If the `catch` or `finally` body fails, that new failure becomes the script failure. If there is no `catch`, `finally` still runs and the original failure is reported.

## Invalid GIF Color Controls

```text
Line 2: gif failed. gif option background= must be a named, hex, rgb, rgba, hsl, or hsla color.
Line 3: recording failed. recording option gradientMode= must be one of: smooth, text.
Line 4: gif failed. gif option highContrastPalette= must be true or false.
```

Color-profile changes do not fail a script. CMG emits `GIF_WARN_COLOR_PROFILE path="..." profileChanges=<count>` so an agent can inspect retained frames or run `cmg gif color-diff`.

```text
Line 2: gif failed. gif option viewport= must use <width>x<height> with dimensions from 100 to 10000.
Line 3: recording failed. recording option pixelRatio= must be a number from 1 to 4.
```

Caption booleans reject values other than `true` or `false`. `captionFormat=` accepts `plain` or `markdown`; markdown supports only safe bold and inline-code spans, not arbitrary HTML.

Long-wait duration options must be positive durations. Compression never changes execution time and `GIF_WAIT_COMPRESSION` is a diagnostic, not a failure.

## Invalid Recording Settings

```text
Line 2: recordingDefaults option pointerWiggle= is not a supported recording default.
Line 4: setRecording option pointerDuration= must be zero or greater.
```

`recording`, `withRecording`, `recordingDefaults`, and `setRecording` reject unknown option names. Value ranges are validated when an affected recording or visual action consumes the setting. Use `browser control script --preview-gif-settings` to catch unknown settings, imports, and syntax without connecting to a browser. Warnings such as `GIF_SETTINGS_WARN ... reason=non-visual-action` identify explicit visual options that cannot affect the named action; warnings do not change the exit code.

Runtime `GIF_WARN_MULTIPLE_TARGETS`, `GIF_WARN_TINY_TARGET`, `GIF_WARN_SCROLLED`, and `GIF_WARN_NON_VISUAL` lines are evidence-quality diagnostics, not script failures. Tighten the selector, enlarge or call out the target, or remove the ignored option when deterministic evidence matters.

`pointerStyle` rejects options outside its pointer appearance/visibility set. `recordVariable` fails when the named variable is undefined or `reveal=` is not boolean. These checks occur only with an active recorder; without one the recording-only actions skip without reading state.

Conditional recording blocks propagate their original child failure after retaining the partial artifact. An unchanged/passing discard is not a failure and writes `GIF_SKIPPED` with `reason=unchanged`, `passed`, or `skipped`. `gifSnapshot` rejects a missing name, a body, or a negative/invalid `duration=`.

Invalid runner GIF declarations fail the affected test before browser actions run. Errors name the declaration or mapped recording property, for example `test option gifQuality= must be one of: ...`, `test option fps= must be between 1 and 100`, or `test option scale= must be a number from 0.05 to 1`.

Retention declarations fail before test actions when `gif=` is not `always`, `onFailure`, `onRetry`, or `off`; `gifSampleRate=` is below `1` or not an integer; or `gifCleanPassed=` is not boolean. Artifact deletion failures are reported as runner failures rather than silently claiming cleanup succeeded.

CLI retention setup fails before browser connection when `--gif-sample-rate` is below `1`, a retention mode is unknown, or more than one of `--gif-retention`, `--gif-on-failure`, and `--gif-on-retry` is supplied. Run-config type errors name the exact `gifRetention`, `gifSampleRate`, or `gifCleanPassed` property.

Run-config `gifSettings` rejects unknown keys, incorrect JSON types, and invalid recording values before browser connection or `--list` output. Root settings are validated even when no project is selected; selected project settings overlay root values before validation and CLI precedence is applied per property.

`safeArea` must be from `0` to `500`; `layoutStability` must be from `0` to `5000`. Invalid CLI, config, suite/test, recording-scope, or GIF-block values identify the responsible option. Set either to `0` when exact edge framing or immediate coordinates are intentional.

`pointerPath` and `dragPath` accept `auto`, `direct`, `arc`, `manhattan`, `avoid-target`, or `avoid-center`. Invalid values fail before browser connection for CLI/config settings and at the responsible DSL line for scoped/action settings.

Automatic-caption templates reject unknown placeholders before browser connection for whole-run CLI/config defaults and at the responsible DSL action for scoped overrides. Supported placeholders are `{action}`, `{selector}`, `{target}`, `{line}`, `{arguments}`, `{step}`, and `{assertion}`.

Whole-run redaction rejects empty selectors, non-string config array entries, unknown `autoRedact` or `redactionSafety` values, and strict safety failures with an exact reason. Strict capture failures fail the responsible action rather than writing an unmasked frame.

`showMouseButtons=` accepts only boolean values. Activity overlay blocks require a body and no positional arguments; child action failures propagate normally. Without an active recorder, the blocks do not validate or inject visual evidence, but their child browser actions still run.

`GIF_SETTINGS_WARN ... reason=gif-alias`, `GIF_SETTINGS_WARN ... reason=long-recording-block`, runtime `GIF_ALIAS_WARN`, and runner `GIF_RETENTION_WARN` are authoring guidance, not errors. They are written to stdout, preserve otherwise successful exit code `0`, and never require a browser when produced by `--preview-gif-settings` or `cmg run --list`.

`GIF_DISABLED` and `GIF_SKIPPED ... reason=recording-disabled` are intentional privacy-policy diagnostics, not failures. The wrapped actions still determine success or failure; suppressed recording never injects a virtual pointer or creates a partial artifact.
