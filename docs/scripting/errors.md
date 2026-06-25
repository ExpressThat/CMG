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
- Using inline comments. Only full-line comments are supported.
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
```

## V1 Script Passed To `cmg run`

```text
Line 1: cmg run requires the new DSL with test/suite blocks. V1 flat scripts are not supported; see docs/scripting/migration.md.
```

Use `browser control script --file <path>` for direct browser-control scripts, or migrate the file to the new test DSL for `cmg run`.

## Runner Step Failure

`cmg run` prints step diagnostics on stderr:

```text
STEP FAIL line=8 action=click reason=Line 8: click failed. No element matched selector '#missing'.
```

JSON and HTML reports include the same reason, the test name, output lines, and any GIF path connected to the failure.

Use `cmg run --trace <directory>` to write per-test trace JSON with every recorded step and failure reason.

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

## `moveMouse` Without GIF

```text
Line 3: moveMouse failed. moveMouse requires script GIF recording. Run the script with --gif <path>.
```

`moveMouse` exists only for `browser control script --gif` runs. It is not available as a one-off CLI command and does not run in non-GIF scripts.

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

## Invalid Emulation

```text
Line 3: emulate failed. emulate requires both width and height when overriding viewport.
Line 4: emulate failed. geolocation must be '<latitude>,<longitude>'.
Line 5: setGeolocation failed. setGeolocation requires '<latitude>,<longitude>' or latitude=<value> longitude=<value>.
Line 6: grantPermissions failed. grantPermissions requires at least one permission name.
Line 7: setViewport failed. setViewport option hasTouch= must be true or false.
Line 8: viewport failed. viewport expects width=<pixels> height=<pixels> or '<width>' '<height>'.
```

Provide both viewport dimensions together, pass boolean viewport options as `true` or `false`, pass geolocation as a comma-separated latitude/longitude pair or latitude/longitude options, and grant at least one permission name.

## Download Timeout

```text
Line 6: download failed. No download matching '*.csv' appeared in 'C:\Projects\CMG\demo-output' within 10000ms.
```

Check the browser download directory, file pattern, and timeout. `download` ignores in-progress Chrome `.crdownload` files.

## Screenshot Failure

```text
Line 4: screenshotPage failed. screenshotPage option fullPage= must be true or false.
Line 5: screenshotPage failed. Firefox did not return screenshot image data.
```

Use `fullPage=true` for a full scrollable page capture. Use `fullPage=false` or leave it unset for the current viewport.

## Console Timeout

```text
Line 7: waitForConsole failed. Console message 'settings saved' was not seen within 5000ms.
```

Call `captureConsole` before the action that should log the message, and check the optional `level=` filter.

## Page Error Timeout

```text
Line 8: waitForPageError failed. Page error 'Cannot read' was not seen within 5000ms.
```

Call `capturePageErrors` before the action that should throw or reject, and match text from the page error or rejected value.

## Init Script Failure

```text
Line 2: addInitScript failed. addInitScript requires inline script text or path=<file>.
Line 3: addInitScript failed. Init script file 'C:\Projects\CMG\fixtures\init.js' was not found.
```

Pass one inline script argument or `path=<file>`. Register init scripts before navigating to the page that should receive them.

## Frame Failure

```text
Line 8: frameClick failed. Iframe #checkoutFrame is not same-origin or is not ready.
Line 9: frameAssertText failed. Expected frame text to contain Saved, got Saving.
```

Frame actions require a same-origin iframe because they run through page JavaScript. Use tab or browser-context actions for cross-origin flows that open in their own page target.

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
Line 3: useContext failed. Browser context 'ctx-2' was not found in this script run.
Line 4: closeContext failed. Browser context 'ctx-2' was not found in this script run.
```

`useContext` and `closeContext` can only reference contexts created earlier in the same script run. Use the context id stored by `newContext ctx` through `${ctx}` when possible.

## Accessibility Failure

```text
Line 6: expectAccessible failed. expectAccessible requires role=<role>.
Line 7: expectAccessible failed. No accessible element matched role=button name=Save.
```

Use `accessibilitySnapshot output="..."` to inspect the derived role/name tree when an assertion fails.

## Failed Assertion

```text
Line 5: assertText failed. Expected text 'Ready' was not found. Actual text: 'Loading'.
```

The assertion checks whether the element text contains the expected text.

## API Request Failure

```text
STEP FAIL line=3 action=apiRequest reason=Expected status 200, got 500.
STEP FAIL line=4 action=apiRequest reason=apiRequest option timeout= must be a positive number of milliseconds.
```

`apiRequest` failures include the reason in stderr and reports. The step output includes `API` and `API_BODY` lines when a response was received.

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
Line 2: waitForUrl failed. URL did not match /checkout within 5000ms. Last URL: https://example.com/cart
Line 3: waitForLoadState failed. waitForLoadState expects loading, interactive, complete, or load.
Line 4: waitForNavigation failed. waitForNavigation waitUntil= expects load, domcontentloaded, networkidle, or commit.
Line 5: waitForNavigation failed. Navigation did not reach load within 5000ms. Last URL: https://example.com/cart; state: loading
```

Navigation wait failures include the expected state or URL, timeout, and the last observed browser state when available.

## Storage Failure

```text
Line 4: localStorage failed. localStorage set expects a key and value.
Line 5: cookie failed. cookie expects get, set, remove, or clear.
```

Storage actions validate the operation and required key/value arguments before mutating browser state.

## Wait Failure

```text
Line 6: waitForFunction failed. waitForFunction did not become truthy within 5000ms.
Line 7: waitForTimeout failed. delay must be a positive integer.
```

Wait failures include whether the selector, expression, or timeout argument caused the failure.

## Mouse Failure

```text
Line 4: mouseMove failed. mouseMove requires either one alias argument or x=<pixels> y=<pixels> options.
Line 5: mouseDown failed. mouseDown target (900, 900) is outside the current viewport 800x600.
```

Mouse actions validate target shape and viewport bounds before sending pointer events.

## Keyboard Failure

```text
Line 3: keyDown failed. Expected 1 positional argument(s), got 0.
Line 4: insertText failed. Expected 1 positional argument(s), got 0.
```

Keyboard actions require one key or text argument and operate on the current browser focus.

## Dialog Failure

```text
Line 4: setDialogBehavior failed. setDialogBehavior expects accept or dismiss.
Line 5: waitForDialog failed. Timed out waiting for dialog Saved
```

Dialog waits match against captured `alert`, `confirm`, and `prompt` messages. Install `captureDialogs` before triggering the dialog.

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
```

Network waits use substring matching against page `fetch()` and `XMLHttpRequest` URLs recorded by CMG's page-side network patch. `waitForRequestFinished` waits for completed responses. `waitForRequestFailed` waits for rejected `fetch()` calls or XHR `error`, `abort`, or `timeout` events.

## WebSocket Failure

```text
Line 4: routeWebSocket failed. routeWebSocket option close= must be true or false.
Line 5: waitForWebSocketMessage failed. Timed out waiting for websocket message ready
```

WebSocket waits use substring matching against recorded socket URLs and message data. Install `routeWebSocket` before creating sockets that should receive synthetic route behavior.

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
```

Use `listWorkers` to inspect available worker ids and URLs before targeting a specific worker. Worker interception patches `fetch()` inside existing worker targets.

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
Line 6: printPdf failed. Firefox did not return PDF data.
```

PDF generation is available in both direct scripts and `cmg run`. If the browser provider does not return PDF bytes, CMG reports that provider failure directly.

## Visual Assertion Failure

```text
STEP FAIL line=5 action=expectScreenshot reason=Screenshot diff 0.0321 exceeded tolerance 0.01.
```

The `VISUAL_ACTUAL` output line points to the actual PNG written for review. If the baseline file is missing, CMG creates it from the actual screenshot and fails with a message saying the baseline was created.

## UI State Assertion Failure

```text
STEP FAIL line=7 action=expectVisible reason=Expected element to be visible.
STEP FAIL line=8 action=expectAttribute reason=Expected attribute aria-label to contain Save, got Cancel.
```

`expectVisible`, `expectHidden`, `expectEnabled`, `expectDisabled`, `expectValue`, `expectAttribute`, `expectChecked`, and `expectCount` run in the page and report the expected state plus the actual state that caused the failure.

## Storage State Failure

```text
STEP FAIL line=4 action=storageState reason=Storage state file 'auth.json' was not found.
```

Use `storageState save path="..."` before loading a state file.

## File Upload Failure

```text
STEP FAIL line=6 action=uploadFiles reason=Upload file 'fixtures\avatar.png' was not found.
```

Check that every file path passed after the selector exists from the process working directory, or use absolute paths. A missing selector or missing file argument fails before any browser interaction runs.

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
break must be inside a loop.
continue must be inside a loop.
Line 9: if failed. Invalid action condition 'assertText'.
```

Define macros before calling them. Top-level macros in `cmg run` are registered for each test; macros declared inside another macro, branch, or loop are scoped to that block and are not available afterward.

Loop variables and macro parameters are scoped to the loop iteration or macro call. Use `set` when a value must be available after the block completes.
