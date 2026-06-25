# `.cmgscript` Actions

All actions fail fast. If an action fails, later actions are not executed.

While an action is connected to a page, CMG automatically accepts browser JavaScript dialogs and leave-page prompts. This includes alerts, confirms, prompts, and before-unload confirmation prompts that would otherwise block automation.

## `navigate`

```text
navigate "<url-or-path>"
```

Navigates the primary page target to a URL, data URL, or local file path.

On success, stdout includes a `NAVIGATED <line> <final-url>` line after the `PASS` line. Local file paths must exist; missing path-like targets fail before the browser is asked to navigate.

Example:

```text
navigate "C:\Projects\CMG\index.html"
```

## `reload`, `goBack`, `goForward`, `waitForUrl`, And `waitForLoadState`

```text
reload
goBack timeout=5000
goForward timeout=5000
waitForUrl "/checkout" timeout=10000
waitForLoadState "complete" timeout=5000
```

Runs common page navigation controls from both direct browser-control scripts and `cmg run`.

Options:

- `timeout`: Optional for `goBack`, `goForward`, `waitForUrl`, and `waitForLoadState`. Default is `5000`.

Arguments:

- `waitForUrl`: Required URL substring expected in `location.href`.
- `waitForLoadState`: Optional state. Supports `loading`, `interactive`, `complete`, and `load`. `load` is an alias for `complete`.

Output:

- `RELOADED <line> <url>` after reloading the current URL.
- `BACK <line> <url>` after browser history moves back.
- `FORWARD <line> <url>` after browser history moves forward.
- `URL <line> <url>` when `waitForUrl` matches.
- `LOAD_STATE <line> <state>` when the requested load state is reached.

These actions do not move the virtual pointer. Use `step`, `caption`, or a `gif` block when a GIF should narrate a non-visual navigation wait.

## `waitForElement`

```text
waitForElement "<selector>" timeout=5000
```

Waits until a selector exists in any available page target.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `5000`.

Example:

```text
waitForElement "#openProfileDialog" timeout=5000
```

## `click`

```text
click "<selector>"
```

Clicks the element in the selected browser. `click` does not scroll automatically; the element center must already be inside the current viewport. Use `scrollIntoView` first when the script should move the page.

Example:

```text
click "#openProfileDialog"
```

## `type`

```text
type "<selector>" "text"
```

Focuses the element without scrolling, appends text to its current value, and dispatches `input` and `change` events. The element center must already be inside the current viewport. Use `scrollIntoView` first when the script should move the page.

Example:

```text
type "#profileName" "CMG Test Profile"
```

## `clear`

```text
clear "<selector>"
```

Focuses the element without scrolling, clears its value, and dispatches `input` and `change` events. The element center must already be inside the current viewport. Use `scrollIntoView` first when the script should move the page.

Example:

```text
clear "#profileName"
```

## `press`

```text
press "Enter"
```

Dispatches key down and key up behavior in the selected browser.

Example:

```text
press "Escape"
```

## `hover`

```text
hover "<selector>"
```

Dispatches mouseover and mousemove events at the element center. `hover` does not scroll automatically; the element center must already be inside the current viewport.

Example:

```text
hover "#openProfileDialog"
```

## `moveMouse`

```text
moveMouse "center"
moveMouse "bottom"
moveMouse x=100 y=200
moveMouse selector=".content-area" edge=bottom inset=16
```

Moves the GIF virtual pointer to a viewport-relative position. `moveMouse` is script-only and requires `browser control script --gif <path>`; there is no one-off CLI `browser control moveMouse` command. Without `--gif`, the action fails.

Coordinates are CSS pixels relative to the visible viewport. Alias targets use a small inset so the pointer remains inside the viewport.

For scrollable containers, use selector/edge targeting to move inside the edge of an element instead of the browser viewport. This is useful for apps that edge-scroll a canvas or content area during dragover.

Supported aliases:

- `center`
- `top`
- `bottom`
- `left`
- `right`
- `topLeft`
- `topRight`
- `bottomLeft`
- `bottomRight`

Options:

- `x`: Required when using coordinates.
- `y`: Required when using coordinates.
- `selector`: Optional selector for element-edge targeting. Use with `edge=`.
- `edge`: Required for element-edge targeting. Supports `top`, `bottom`, `left`, `right`, `center`, `topLeft`, `topRight`, `bottomLeft`, and `bottomRight`.
- `inset`: Optional element-edge inset in CSS pixels. Default is `16`.

Examples:

```text
moveMouse "center"
moveMouse x=240 y=320
moveMouse selector=".content-area" edge=bottom inset=24
```

Inside a GIF `dragAndDrop` block, `moveMouse` moves the held pointer and keeps drag movement events active. This is useful for page edge-autoscroll behavior:

```text
dragAndDrop ".card" {
  moveMouse selector=".content-area" edge=bottom inset=24
  delay 1500
  moveMouse selector=".content-area" edge=bottom inset=24
  delay 1500
  drop "#target"
}
```

## `scrollIntoView`

```text
scrollIntoView "<selector>"
```

Scrolls an element into the center of the viewport.

Example:

```text
scrollIntoView "#dragdrop"
```

## `select`

```text
select "<selector>" "value"
```

Sets a select-like element value and dispatches `input` and `change` events. `select` does not scroll automatically; the element center must already be inside the current viewport.

Example:

```text
select "#environment" "prod"
```

## `showMessageBar`

```text
showMessageBar "message"
```

Injects or updates a fixed caption bar near the top center of the page with a custom message. The bar dynamically sizes to the message, supports multi-line text, uses the browser top layer when available so it appears above page dialogs, is visible in screenshots and GIF recordings, and does not intercept pointer input.

Example:

```text
showMessageBar "Opening the profile dialog"
showMessageBar "Opening profile dialog\nWaiting for dialog content"
```

## `delay`

```text
delay 1000
```

Pauses execution for the specified number of milliseconds.

## `html`

```text
html "<selector>"
```

Prints the selected element's `outerHTML` to stdout as an `HTML` result line.

Example:

```text
html "#openProfileDialog"
```

## `screenshot`

```text
screenshot "<selector>" output="element.png"
screenshot "<selector>"
```

Captures a PNG screenshot of an element.

Options:

- `output`: Optional file path. Without it, stdout receives a `data:image/png;base64,...` result.

## `screenshotPage`

```text
screenshotPage output="page.png"
screenshotPage
```

Captures a PNG screenshot of the primary page target.

Options:

- `output`: Optional file path. Without it, stdout receives a `data:image/png;base64,...` result.

## `printPdf`

```text
printPdf path="demo-output\page.pdf" printBackground=true
pdf path="demo-output\page-landscape.pdf" landscape=true scale=0.9
```

Prints the current page to a PDF file. `pdf` is an alias for `printPdf`. This action is available in both direct browser-control scripts and `cmg run`.

Options:

- `path`: Required output PDF path.
- `landscape`: Optional boolean. Default is `false`.
- `printBackground`: Optional boolean. Default is `true`.
- `scale`: Optional positive number. Default is `1`.

Output:

- `PDF <line> <path>` on success.

PDF generation does not move the virtual pointer. It is supported for Chrome and Edge through the browser print API. Firefox runs fail with an explicit unsupported-provider reason.

## `assertText`

```text
assertText "<selector>" "expected text"
assertText "<selector>" "expected text" timeout=5000
```

Reads an element's visible text and fails unless it contains the expected text. When `timeout` is provided, CMG polls the element text until it matches or the timeout expires. The DSL `expectText` action lowers to this command and supports the same option.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `0`, which checks once.

Example:

```text
assertText "#lastDialogAction" "None"
```

## `evaluate`

```text
evaluate "document.title"
```

Evaluates JavaScript in the primary page target and prints the returned value as an `EVALUATE` result line.

## `addInitScript` And `evaluateOnNewDocument`

```text
addInitScript "window.__featureFlag = true;"
addInitScript path="fixtures\init.js"
evaluateOnNewDocument "window.__readyBeforeApp = true;"
```

Registers JavaScript to run before future page documents execute app scripts. `evaluateOnNewDocument` is an alias for Puppeteer-style scripts. Use this before `navigate`, `reload`, `openTab`, or context setup that should see the injected globals.

Options:

- `path`: Optional JavaScript file to read as the init script source. When `path` is omitted, pass one inline script argument.

Output:

- `INIT_SCRIPT <line> <id>` when the script is registered.

Init-script actions do not move the virtual pointer. They are included in reports and traces, and can be wrapped with `step` or captions when GIF narration is useful.

## `setViewport`

```text
setViewport width=1280 height=720
```

Sets viewport metrics for the primary page target.

Required options:

- `width`
- `height`

## `emulate`

```text
emulate width=390 height=844 userAgent="CMG Mobile" locale=en-GB colorScheme=dark reducedMotion=reduce
emulate timezone=Europe/London geolocation="51.5,-0.1" permissions=geolocation
```

Applies browser-environment overrides for both direct browser-control scripts and `cmg run` tests. `width` and `height` use the browser protocol viewport override. Other options install page-side overrides in the current page context.

Options:

- `width`: Viewport width in CSS pixels. Must be used with `height`.
- `height`: Viewport height in CSS pixels. Must be used with `width`.
- `userAgent`: Page-visible `navigator.userAgent` value.
- `locale`: Page-visible `navigator.language` and `navigator.languages` value.
- `timezone`: Reported `Intl.DateTimeFormat().resolvedOptions().timeZone`.
- `colorScheme`: Media query result for `prefers-color-scheme`, for example `dark` or `light`.
- `reducedMotion`: Media query result for `prefers-reduced-motion`, for example `reduce` or `no-preference`.
- `geolocation`: Stubbed coordinates as `<latitude>,<longitude>`.
- `permissions`: Comma-separated permission names that `navigator.permissions.query` should report as `granted`.

Output:

- `EMULATE <line> <option-names>` on success.

## `dragAndDrop`

```text
dragAndDrop "<sourceSelector>" "<targetSelector>"
dragAndDrop "<sourceSelector>" {
  delay 200
  hover "<selector>"
  drop "<targetSelector>"
}
```

Dispatches drag-and-drop DOM events from the source element to the target element.

Simple drag-and-drop does not scroll automatically. The source and target centers must both already be inside the current viewport. Use `scrollIntoView` and, when needed, a large enough viewport before dragging.

Example:

```text
dragAndDrop "[data-command='browser launch']" "#dropQueue"
```

The block form is the complex drag sequence. It has no inline target selector; the target is provided by the required `drop` child action.

Allowed child actions:

- `delay <milliseconds>`: Pause while the drag is active. With `--gif`, frames are captured during the hold.
- `hover "<selector>"`: Move the active drag pointer to another element.
- `moveMouse "<alias>"`, `moveMouse x=<pixels> y=<pixels>`, or `moveMouse selector="<selector>" edge=<edge> inset=<pixels>`: Move the active drag pointer to a viewport-relative position or inside an element edge. Requires `--gif`.
- `scrollIntoView "<selector>"`: Scroll an element into view before continuing.
- `waitForElement "<selector>" timeout=5000`: Wait for an element before continuing.
- `drop "<selector>"`: Finish the drag on the target selector. Required exactly once.

Rules:

- The block form must have exactly one source selector argument.
- The block must contain exactly one `drop`.
- No actions are allowed after `drop`.
- Other child actions fail with `Action '<name>' is not supported inside block dragAndDrop.`
- With `--gif`, recorded drags use one synthetic drag lifecycle and dispatch exactly one `drop` event. The block form dispatches it at the `drop "<selector>"` step.
- With `--gif`, CMG keeps the drag lifecycle active while the body runs so page-owned drag ghosts can stay visible.
- With `--gif`, every automatic pointer movement dispatches browser movement and hover events, including movement before `click`, `type`, `clear`, `hover`, `select`, and `dragAndDrop`.
- With `--gif`, block drag bodies also dispatch DOM `pointerdown`/`mousedown`, held `pointermove`/`mousemove`, and `pointerup`/`mouseup` so page drag state and edge-autoscroll code can react while `moveMouse "bottom"` and `delay` run.
- CMG creates the `DataTransfer` object for synthetic drag events but does not force `effectAllowed`, `dropEffect`, or payload values. The page's own `dragstart` handler remains responsible for setting drag data and allowed operations; CMG preserves those page-set values through the recorded drag.
- If the page does not call `DataTransfer.setDragImage()`, CMG shows a default-preview bridge during the active drag so the live browser and GIF still show a browser-style default drag preview.

Complex example:

```text
dragAndDrop "[data-command='browser launch']" {
  delay 200
  hover "#lastDialogAction"
  delay 200
  hover "#dropQueue"
  drop "#dropQueue"
}
```

## `listTabs`

```text
listTabs
```

Prints available page targets as `TAB` result lines.

## `openTab`

```text
openTab "https://example.com"
```

Opens a new browser tab from the active page with `window.open`. Local file paths are normalized the same way as `navigate`.

Output:

- `TAB_OPENED <line> <target>` on success.

## `waitForTab`

```text
waitForTab count=2 timeout=5000
```

Polls available page targets until at least `count` tabs exist. Use this after an action that opens a popup or after `openTab`.

Required options:

- `count`: Minimum tab count.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `5000`.

Output:

- `TAB_COUNT <line> <actual-count>` on success.

## `download` And `waitForDownload`

```text
download "#export" directory="demo-output" pattern="*.csv" timeout=10000
waitForDownload directory="demo-output" pattern="*.zip" timeout=10000
```

`download` clicks a selector and then waits for a matching file. In GIF recordings, the virtual pointer moves to the selector and records the click that triggers the download. `waitForDownload` only polls the filesystem and does not move the pointer.

Options:

- `directory`: Directory to watch. Default is the current working directory.
- `pattern`: File glob to match. Default is `*`.
- `timeout`: Optional timeout in milliseconds. Default is `5000`.

Output:

- `DOWNLOAD <line> <path>` on success.

## `captureConsole` And `waitForConsole`

```text
captureConsole
waitForConsole "saved" level=info timeout=5000
```

Installs a page-side console hook and waits for a captured console message. Call `captureConsole` before the page action that is expected to log. `waitForConsole` checks captured `log`, `info`, `warn`, and `error` calls.

Options:

- `level`: Optional console level filter: `log`, `info`, `warn`, or `error`.
- `timeout`: Optional timeout in milliseconds. Default is `5000`.

Output:

- `CONSOLE_CAPTURE <line>` when the hook is installed.
- `CONSOLE <line> <level>: <text>` when a matching message is found.

## `capturePageErrors` And `waitForPageError`

```text
capturePageErrors
waitForPageError "Cannot read" timeout=5000
```

Installs page-side listeners for `error` and `unhandledrejection` events and waits for a matching captured page error. Call `capturePageErrors` before the action expected to throw or reject.

Options:

- `timeout`: Optional timeout in milliseconds for `waitForPageError`. Default is `5000`.

Output:

- `PAGE_ERROR_CAPTURE <line>` when the hook is installed.
- `PAGE_ERROR <line> <type>: <text>` when a matching page error is found.

Page-error actions do not move the virtual pointer. They are included in reports and traces, and can be wrapped with `step` or captions when GIF narration is useful.

## Frame Actions

```text
frameClick "#checkoutFrame" "#save"
frameType "#checkoutFrame" "#email" "agent@example.com"
frameFill "#checkoutFrame" "#name" "CMG"
frameHover "#checkoutFrame" "#help"
frameWaitForElement "#checkoutFrame" "#ready" timeout=5000
frameAssertText "#checkoutFrame" "#status" "Saved"
frameEvaluate "#checkoutFrame" "document.title"
```

Runs actions against a same-origin iframe selected from the top page. The first argument is the iframe selector. The second argument is the selector or JavaScript expression inside that frame.

GIF behavior:

- `frameClick`, `frameType`, `frameFill`, and `frameHover` move the virtual pointer to the element's actual top-page coordinate inside the iframe before running.
- Non-visual frame actions do not move the pointer, but their outputs and failures are captured in reports and traces.

Output:

- `FRAME <line> <action>` for successful frame actions.
- `FRAME_EVALUATE <line> <result>` for `frameEvaluate`.

Cross-origin iframes cannot be accessed from page JavaScript. Those fail with a clear same-origin/not-ready reason.

## `clock`, `tick`, And `restoreClock`

```text
clock now=1700000000000
tick 250
restoreClock
```

Installs deterministic page-side time control, advances fake time, and restores the browser clock. The fake clock patches `Date`, `Date.now`, `setTimeout`, `clearTimeout`, `setInterval`, and `clearInterval` in the current page context.

Options:

- `now`: Optional epoch milliseconds for `clock`. Default is the current host time.

Output:

- `CLOCK <line> <epoch-ms>` when the fake clock is installed.
- `TICK <line> <milliseconds> now=<epoch-ms>` after fake time advances.
- `CLOCK_RESTORED <line>` after restoring native time APIs.

Clock actions do not move the virtual pointer. In GIF recordings, wrap them in `step` or `caption` when the time change should be visible to a viewer.

## `clearContext` And `resetContext`

```text
clearContext
resetContext
```

Clears page context state in the current browser page. Both actions clear `localStorage`, `sessionStorage`, same-origin cookies, IndexedDB databases, Cache Storage entries, and registered service workers when the page exposes those APIs.

`resetContext` also navigates the current page to `about:blank` after clearing state. `clearContext` leaves the current page loaded.

Output:

- `CONTEXT_CLEARED <line>` for `clearContext`.
- `CONTEXT_RESET <line>` for `resetContext`.

These actions do not move the virtual pointer. They provide practical context cleanup inside CMG's controlled browser instance; they are not a protocol-native replacement for creating multiple isolated browser contexts.

## `newContext`, `useContext`, `listContexts`, And `closeContext`

```text
newContext ctx url="about:blank"
useContext "${ctx}"
listContexts
closeContext "${ctx}"
```

Creates and switches between isolated browser contexts in Chrome and Edge. `newContext` creates a fresh browser context, opens a page in it, activates that page for later actions, and optionally stores the context id in the variable named by the first argument. `useContext` activates a previously created context by context id or target id. `closeContext` disposes a context created during the same script run.

Options:

- `url`: Optional initial URL for `newContext`. Default is `about:blank`.

Output:

- `CONTEXT_CREATED <line> id=<context-id> target=<target-id> url="<url>"` when a context is created.
- `CONTEXT_ACTIVE <line> <id>` when a context is activated.
- `CONTEXT <index> id=<context-id> target=<target-id> active=<true|false> url="<url>"` for `listContexts`.
- `CONTEXT_CLOSED <line> <id>` when a context is closed.

Context actions do not move the virtual pointer. Later pointer-aware actions use the currently active context, so GIF recordings keep the same virtual pointer and event behavior inside that page. Firefox runs fail with an explicit unsupported-provider reason until CMG has a BiDi-backed isolated context implementation.

## `accessibilitySnapshot` And `expectAccessible`

```text
accessibilitySnapshot
accessibilitySnapshot "#dialog" output="artifacts\a11y.json"
expectAccessible role=button name="Save"
```

Builds a page-derived accessibility snapshot or asserts that an element with a matching role and accessible-ish name exists. The snapshot includes role, name, hidden state, disabled state, and children derived from DOM attributes and common implicit roles.

Options:

- `output`: Optional JSON output path for `accessibilitySnapshot`.
- `role`: Required role for `expectAccessible`.
- `name`: Optional text expected in the accessible name for `expectAccessible`.

Output:

- `ACCESSIBILITY <line> <json-or-path>` for snapshots.
- `ACCESSIBLE <line> role=<role> name="<name>"` for successful assertions.

These actions do not move the virtual pointer. They are included in reports and traces. CMG currently derives this data from the page DOM rather than a protocol-native accessibility tree, which keeps behavior shared across supported browser clients.

## `activateTab`

```text
activateTab index=0
```

Activates a tab by index from `listTabs`.

Required options:

- `index`

## `closeTab`

```text
closeTab index=1
```

Closes a tab by index from `listTabs`.

Required options:

- `index`

## `set`

```text
set name "value"
```

Stores a variable for later `${name}` expansion.

Example:

```text
set target "#openProfileDialog"
click "${target}"
```

## `readFile`, `fixture`, `writeFile`, `appendFile`, And `expectFile`

```text
readFile payload path="fixtures\payload.json"
fixture avatar path="fixtures\avatar.png" encoding=base64
writeFile path="demo-output\result.txt" text="Saved ${payload}"
appendFile path="demo-output\result.txt" text="\nDone"
expectFile path="demo-output\result.txt" contains="Done"
```

Reads, writes, appends, and asserts local files from both direct browser-control scripts and `cmg run`. `readFile` and `fixture` store the file content in the variable named by the first argument, so later actions can use `${name}` expansion. Use `encoding=base64` when the fixture should be stored as base64 text.

Options:

- `path`: Required file path.
- `encoding`: Optional for `readFile` and `fixture`; use `base64` for binary fixtures.
- `text`: Optional text for `writeFile` and `appendFile`. If omitted, the first positional argument is written.
- `contains`: Optional text that `expectFile` requires in the file.

Output:

- `FILE_READ <line> <variable> <path>` when a file or fixture is read.
- `FILE_WRITTEN <line> <path>` when a file is written.
- `FILE_APPENDED <line> <path>` when a file is appended.
- `FILE_OK <line> <path>` when a file assertion passes.

File actions do not move the virtual pointer. In GIF runs, wrap them in `step`, `caption`, or a `gif` block when the recording should narrate setup, fixture loading, or artifact checks.

## Runner-Only Structural Actions

These actions are used with `cmg run`, not `browser control script`.

### `suite`

```text
suite "name" {
  test "case" {
    click "#run"
  }
}
```

Groups tests in reports. Test names are emitted as `<suite> / <test>`.

### `test`

```text
test "name" {
  navigate "https://example.com"
}
```

Defines a runnable test. `cmg run` exits `1` if any test fails.

### `beforeEach` And `afterEach`

```text
beforeEach {
  navigate "https://example.com"
}
```

Root hooks apply to every test. Hooks inside a suite apply to that suite.

### `step`

```text
step "Open dialog" {
  click "#open"
}
```

Adds a visible caption before running the wrapped actions. In GIF mode, the caption appears in the recording.

### `gif`

```text
gif "open-dialog" {
  click "#open"
}
```

Records only the wrapped actions when `cmg run` is used without command-level `--gif`. If `cmg run --gif <directory>` is used, the entire test is recorded and nested `gif` blocks do not create separate GIFs.

## Runner Convenience Actions

These actions lower to existing visual browser operations so GIF recordings still use the virtual pointer where appropriate.

### `caption`

```text
caption "message"
```

Alias for `showMessageBar`.

### `fill`

```text
fill "#name" "CMG"
```

Clears the field and types the value. In GIF mode this keeps the same visible pointer and progressive typing behavior as `clear` plus `type`.

### `check` And `uncheck`

```text
check "#enabled"
uncheck "#enabled"
```

Sets a checkbox-like element and dispatches `input` and `change` events.

### `focus`, `blur`, And `selectText`

```text
focus "#name"
selectText "#name"
blur "#name"
```

Runs the corresponding DOM focus/selection behavior.

### `dblclick` And `rightClick`

```text
dblclick "#item"
rightClick "#item"
```

Moves to the element with the visual hover path, then dispatches the page-facing mouse event.

### `expectUrl` And `expectTitle`

```text
expectUrl "/checkout"
expectTitle "Checkout"
```

Fails unless the current URL or title contains the expected text.

These are shared actions, so they work in both direct browser-control scripts and `cmg run`.

Output:

- `URL <line> <url>` for `expectUrl`.
- `TITLE <line> <title>` for `expectTitle`.

### `expectValue`, `expectAttribute`, `expectChecked`, And `expectCount`

```text
expectValue "#email" "agent@example.com" timeout=5000
expectAttribute "#save" "aria-label" "Save"
expectChecked "#terms" true
expectCount ".result" 3 timeout=5000
```

Runs browser-side assertions for common UI state checks. `expectValue`, `expectAttribute`, and `expectChecked` resolve CMG locators before checking the matched element. `expectCount` counts matching CSS elements and supports zero-count assertions.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `0`, which checks once.

### `waitForUrl`

```text
waitForUrl "/dashboard" timeout=10000
```

Polls the current URL until it contains the expected text, then reports the matched URL. If the timeout expires, the failure reason includes the expected text, timeout, and last URL seen.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `5000`.

### `localStorage`, `sessionStorage`, And `cookie`

```text
localStorage set "token" "abc"
sessionStorage remove "token"
cookie set "mode" "demo"
```

Reads or mutates page storage from the current page context.

### `apiRequest`

```text
apiRequest "GET" "https://example.com/api/status" status=200 contains="ok"
apiRequest "POST" "https://example.com/api/items" body="{\"name\":\"demo\"}" header.Authorization="Bearer token"
```

Runs an HTTP request from the test runner. This does not move the virtual pointer because it is not a browser UI action, but it is included in reports and step failure diagnostics.

Options:

- `status`: Expected numeric response status.
- `contains`: Text expected in the response body.
- `body`: Request body text.
- `header.<name>`: Request header.

### `storageState`

```text
storageState save path="artifacts\auth.json"
storageState load path="artifacts\auth.json"
```

Saves or loads page storage state for the current browser page. The state includes `localStorage`, `sessionStorage`, and the current `document.cookie` string. This is a runner action and reports `STORAGE_STATE` output lines.

### `route`, `intercept`, `mockResponse`, `waitForResponse`, `exportHar`, `replayHar`, And `clearRoutes`

```text
route "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json"
intercept "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json"
waitForResponse "/api/profile" timeout=5000
exportHar path="artifacts\network.har"
replayHar path="artifacts\network.har"
clearRoutes
```

Installs a page-level route for `fetch()` and `XMLHttpRequest`. `intercept` is an alias for `route` for Cypress-style scripts. Matching calls receive the configured mocked response and are recorded in the page response log. `waitForResponse` waits for a logged response whose URL contains the pattern.

`exportHar` writes captured response metadata and bodies to a HAR-like JSON file. `replayHar` reads that file and installs routes for each captured request URL.

Notes:

- Route matching uses substring matching.
- Network actions do not move the virtual pointer. They are captured in reports and can be wrapped with `step` or captions for GIF narration.

Output:

- `ROUTE <line> <pattern>` when a route or intercept is installed.
- `ROUTES_CLEARED <line>` when routes are cleared.
- `RESPONSE <line> <json>` when `waitForResponse` finds a matching response.
- `HAR_EXPORTED <line> <path>` when a HAR file is written.
- `HAR_REPLAY <line> routes=<count> <path>` when HAR routes are installed.

### `setExtraHTTPHeaders`, `clearExtraHTTPHeaders`, And `setOffline`

```text
setExtraHTTPHeaders "X-CMG-Agent" "true" "Accept" "application/json"
clearExtraHTTPHeaders
setOffline true
setOffline false
```

Patches page-side `fetch()` and `XMLHttpRequest` behavior in the current page and future navigations. Extra headers are added to page-originated fetch/XHR requests. Offline mode reports `navigator.onLine=false`, dispatches `offline`/`online`, and makes patched fetch/XHR requests fail while enabled.

Arguments:

- `name value`: Required for `setExtraHTTPHeaders`; repeat pairs to add more request headers.

Output:

- `HEADERS_SET <line> <count>` when headers are installed.
- `HEADERS_CLEARED <line>` when extra headers are cleared.
- `OFFLINE <line> <true|false>` when offline mode changes.

These actions do not move the virtual pointer. They affect page-side requests and are included in reports and traces. Browser-level navigation requests are not rewritten; use them before page actions that call `fetch()` or `XMLHttpRequest`.

### `listWorkers`, `waitForWorker`, `workerEvaluate`, And `workerIntercept`

```text
listWorkers
waitForWorker "worker.js" timeout=5000
workerEvaluate "self.location.href" target="worker.js"
workerIntercept "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json" target="worker.js"
```

Inspects and controls Chrome/Edge worker targets exposed by remote debugging. `workerIntercept` patches a matched worker's `fetch()` function so worker-originated requests can receive deterministic responses. The optional `target` option can be a worker id or URL substring; when omitted, CMG uses the first available worker.

Options:

- `timeout`: Optional for `waitForWorker`. Default is `5000`.
- `target`: Optional worker id or URL substring for `workerEvaluate` and `workerIntercept`.
- `status`: Optional response status for `workerIntercept`. Default is `200`.
- `body`: Optional response body for `workerIntercept`. Default is empty text.
- `contentType`: Optional response content type for `workerIntercept`. Default is `text/plain`.

Output:

- `WORKER <index> id=<id> type=<type> title="<title>" url="<url>"` for `listWorkers`.
- `WORKER_READY <line> id=<id> url="<url>"` for `waitForWorker`.
- `WORKER_EVALUATE <line> <result>` for worker evaluation.
- `WORKER_INTERCEPT <line> routes=<count> <pattern>` when a worker intercept is installed.

Worker actions do not move the virtual pointer. They are included in reports and traces, and can be wrapped with `step` or captions when GIF narration is useful. Firefox runs fail with an explicit unsupported-provider reason until CMG has a BiDi-backed worker target implementation.

### `startCoverage` And `stopCoverage`

```text
startCoverage js=true css=true
navigate "https://example.com"
stopCoverage path="artifacts\coverage.json"
```

Collects JavaScript precise coverage and CSS rule usage from the current Chrome/Edge page target. `startCoverage` enables collection; `stopCoverage` stops collection and returns or writes a JSON object with `js` and `css` arrays.

Options:

- `js`: Optional boolean for `startCoverage`. Default is `true`.
- `css`: Optional boolean for `startCoverage`. Default is `true`.
- `path`: Optional output path for `stopCoverage`. Without it, stdout receives the JSON payload.

Output:

- `COVERAGE_STARTED <line> js=<true|false> css=<true|false>` when collection starts.
- `COVERAGE <line> <path-or-json>` when coverage is stopped.

Coverage actions do not move the virtual pointer. They are included in reports and traces, and can be wrapped with `step` or captions for GIF narration. Firefox runs fail with an explicit unsupported-provider reason until CMG has a BiDi-backed coverage implementation.

### `expectScreenshot`

```text
expectScreenshot "#dialog" baseline="baselines\dialog.png" output="demo-output\dialog.actual.png" tolerance=0.01
expectScreenshot baseline="baselines\page.png" output="demo-output\page.actual.png"
```

Captures an element screenshot when a selector is provided, otherwise captures the page viewport. CMG compares the actual PNG against the baseline and fails if the normalized pixel difference is greater than `tolerance`.

Options:

- `baseline`: Baseline PNG path. If missing, CMG writes it from the actual image and fails with an explanatory reason.
- `output`: Actual PNG path. Default is `actual.png`.
- `tolerance`: Allowed normalized difference from `0` to `1`. Default is `0`.

This action captures visual artifacts and reports them, but it does not move the virtual pointer by itself. Wrap it in a `step` when the GIF should include a caption explaining the comparison.

### `uploadFiles`

```text
uploadFiles "#avatar" "fixtures\avatar.png"
uploadFiles "testid=file-input" "fixtures\one.txt" "fixtures\two.txt"
```

Assigns one or more local files to an `<input type="file">` element and dispatches `input` and `change` events in the page. The first argument is a CMG locator. Remaining arguments are local file paths resolved from the current working directory unless absolute.

Output:

- `UPLOAD <line> <count>` on success.

Failure reasons include a missing selector argument, no file paths, a local file that does not exist, or browser evaluation failure. This action is available at the top level and inside `gif` blocks. It does not move the virtual pointer by itself because browser file choosers cannot be driven from page JavaScript; wrap it in a `step` or `caption` when the GIF should explain the upload transition.

## Unknown Future Actions

Unknown actions fail explicitly instead of being ignored. If a future parity action is not listed in this document, CMG reports it as planned but not implemented so agent callers can distinguish an unsupported DSL command from page behavior.

## Locator Support

Current runner support:

- Plain CSS selectors, for example `"#open"`.
- Explicit CSS selectors, for example `"css=#open"`.
- Test id selectors, for example `"testid=save"`, mapped to `[data-testid='save']`.
- Text locators, for example `"text=Save"`.
- Role locators, for example `"role=button"`.
- Label locators, for example `"label=Email"`.
- Placeholder locators, for example `"placeholder=Search"`.
- Alt text locators, for example `"alt=Logo"`.
- Title locators, for example `"title=Close"`.
- XPath locators, for example `"xpath=//button[.='Save']"`.

For non-CSS locator forms, CMG resolves the element inside the page, marks it with a temporary `data-cmg-locator-id`, and then runs the normal pointer-aware command against that marker. This keeps GIF pointer movement and browser events connected to the resolved element.

## Actionability

Selector-based runner actions wait before running the pointer-aware command. CMG checks that the resolved element:

- exists;
- has a non-empty bounding box;
- is not `display:none` or `visibility:hidden`;
- is not disabled with `disabled` or `aria-disabled="true"`;
- has a stable rectangle across animation frames.

Use `timeout=<milliseconds>` on the action to control the wait. If the element does not become actionable, the step fails with the reason in stderr and reports.
