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

## `assertText`

```text
assertText "<selector>" "expected text"
```

Reads an element's visible text and fails unless it contains the expected text.

Example:

```text
assertText "#lastDialogAction" "None"
```

## `evaluate`

```text
evaluate "document.title"
```

Evaluates JavaScript in the primary page target and prints the returned value as an `EVALUATE` result line.

## `setViewport`

```text
setViewport width=1280 height=720
```

Sets viewport metrics for the primary page target.

Required options:

- `width`
- `height`

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

### `waitForURL`

```text
waitForURL "/dashboard"
```

Checks the current URL and fails with a clear reason when it does not contain the expected text. Full retrying URL waits are planned.

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

## Planned Parity Actions

Commands such as `intercept`, `route`, `apiRequest`, `uploadFiles`, `download`, `popup`, `frame`, device emulation, and full network mocking are reserved for parity work. Until implemented, they fail explicitly with a message saying the action is planned but not implemented in the current slice.

## Locator Support

Current runner support:

- Plain CSS selectors, for example `"#open"`.
- Explicit CSS selectors, for example `"css=#open"`.
- Test id selectors, for example `"testid=save"`, mapped to `[data-testid='save']`.

Planned locators fail during validation with a clear reason until the full locator engine lands:

- `text=`
- `role=`
- `label=`
- `placeholder=`
- `alt=`
- `title=`
- `xpath=`
