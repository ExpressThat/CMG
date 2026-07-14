# `.cmgscript` Actions

Actions fail fast unless documented otherwise. If an action fails, later actions are not executed. `softExpect` records a later failure while continuing, and `skip` stops execution as skipped rather than failed.

Browser JavaScript dialogs are not silently removed, accepted, or dismissed through the browser protocol. Use `captureDialogs` and `setDialogBehavior` before the page action that is expected to call `alert`, `confirm`, or `prompt`.

## Default Timeouts

```text
setDefaultTimeout 10000
setDefaultNavigationTimeout 15000
setDefaultAssertionTimeout 5000
setDefaultExpectTimeout 5000
withTimeout 10000 {
  waitForSelector "#slow-panel"
}
withTimeout default=5000 navigation=15000 assertion=2000 {
  navigate "https://example.com" waitUntil=load
  expectText "#status" "Ready"
}
withDefaultTimeout 10000 {
  waitForResponse "/api/slow"
}
withNavigationTimeout 15000 {
  navigate "https://example.com" waitUntil=load
}
withAssertionTimeout 5000 {
  expectText "#status" "Ready"
}
```

Default timeout actions change later timeout-capable actions in the current direct script or `cmg run` test. They are useful when a flow runs against a slow app and repeating `timeout=` on every wait would make the script harder to read.
Scoped timeout blocks change defaults only for their child actions and restore the previous values afterward, even when a child action fails and a surrounding `try` catches the failure.

- `setDefaultTimeout`: Applies to waits, event waits, downloads, network waits, worker waits, tab waits, API requests, and assertions unless a more specific default exists.
- `setDefaultNavigationTimeout`: Applies to navigation actions and navigation waits.
- `setDefaultAssertionTimeout`: Applies to text, evaluated, element-state, accessibility, and related assertion actions.
- `setDefaultExpectTimeout`: Alias for `setDefaultAssertionTimeout`.
- `withTimeout <milliseconds>`: Temporarily sets the general default timeout for the block.
- `withTimeout default=<milliseconds> navigation=<milliseconds> assertion=<milliseconds>`: Temporarily sets one or more timeout families for the block. `timeout=` aliases `default=` and `expect=` aliases `assertion=`.
- `withDefaultTimeout <milliseconds>`: Temporarily sets the general default timeout for the block.
- `withNavigationTimeout <milliseconds>`: Temporarily sets the navigation default timeout for the block.
- `withAssertionTimeout <milliseconds>` / `withExpectTimeout <milliseconds>`: Temporarily sets the assertion default timeout for the block.

Explicit action-level `timeout=<milliseconds>` always wins. The `browser control script` and `cmg run` commands also accept `--timeout`, `--navigation-timeout`, and `--assertion-timeout` to set whole-run defaults before the first action.

Output:

- `DEFAULT_TIMEOUT <line> <milliseconds>`
- `DEFAULT_NAVIGATION_TIMEOUT <line> <milliseconds>`
- `DEFAULT_ASSERTION_TIMEOUT <line> <milliseconds>`

These actions do not move the virtual pointer. In GIF recordings, put them inside a `step` or pair them with a `caption` when the timeout policy should be visible to viewers.

Scoped timeout blocks are structural. They do not emit their own stdout line, and they do not move the virtual pointer by themselves. Pointer-aware child actions inside the block still use normal virtual pointer, pointer event, caption, drag ghost, and GIF frame behavior.

## `navigate`, `goto`, And `visit`

```text
navigate "<url-or-path>"
goto "<url-or-path>"
visit "<url-or-path>"
navigate "<url-or-path>" waitUntil=load timeout=10000
```

Navigates the primary page target to a URL, data URL, or local file path. `goto` is a Playwright/Puppeteer-style alias and `visit` is a Cypress-style alias. All three use the same output and failure behavior.

Relative targets are resolved against command-line `--base-url` in direct scripts and runner tests. Runner files can also use `baseUrl=` or `baseURL=` on suites and tests; suite values cascade and test values override suite/command values. The same base URL resolution is used by `openTab` and `newContext url=`.

Options:

- `waitUntil`: Optional post-navigation state. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`.
- `state`: Alias for `waitUntil`.
- `timeout`: Optional timeout for `waitUntil`. Default is `5000`.

On success, stdout includes a `NAVIGATED <line> <final-url>` line after the `PASS` line. With `waitUntil`, the same line includes `waitUntil=<state> state=<document-state>`. Local file paths must exist; missing path-like targets fail before the browser is asked to navigate.

Example:

```text
navigate "C:\Projects\CMG\index.html"
visit "https://example.com"
goto "https://example.com" waitUntil=domcontentloaded timeout=10000
navigate "profile" # with --base-url https://example.test/app/
```

## `reload`, `goBack`, `goForward`, `waitForUrl`, `waitForLoadState`, `waitForNetworkIdle`, And `waitForNavigation`

```text
reload
reload waitUntil=domcontentloaded timeout=5000
goBack timeout=5000
goForward waitUntil=domcontentloaded timeout=5000
waitForUrl "/checkout" timeout=10000
toHaveURL "/checkout" match=exact
toHaveTitle "checkout" ignoreCase=true
waitForLoadState "complete" timeout=5000
waitForLoadState "networkidle" timeout=5000
waitForNetworkIdle timeout=5000
networkIdle timeout=5000
waitForNavigation "/checkout" waitUntil=domcontentloaded timeout=10000
```

Runs common page navigation controls from both direct browser-control scripts and `cmg run`.

Options:

- `timeout`: Optional for `reload` when `waitUntil` is set, and for `goBack`, `goForward`, `waitForUrl`, `waitForLoadState`, `waitForNetworkIdle`, and `waitForNavigation`. Default is `5000`.
- `match`: Optional for `waitForUrl`, `waitForTitle`, `expectUrl`, `expectTitle`, `toHaveURL`, and `toHaveTitle`. Supports `contains`, `exact`, and `regex`. Default is `contains`.
- `ignoreCase`: Optional for URL/title match actions. Use `true` for case-insensitive matching.
- `waitUntil`: Optional for `reload`, `goBack`, `goForward`, and `waitForNavigation`. Supports `load`, `domcontentloaded`, `networkidle`, and `commit`. `reload`, `goBack`, and `goForward` keep their original URL-change behavior when omitted.
- `state`: Alias for `waitUntil` on `reload`, `goBack`, `goForward`, and `waitForNavigation`.

Arguments:

- `waitForUrl`, `toHaveURL`: Required URL text expected in `location.href`.
- `toHaveTitle`: Required title text expected in `document.title`.
- `waitForLoadState`: Optional state. Supports `loading`, `interactive`, `complete`, `load`, and `networkidle`. `load` is an alias for `complete`; `networkidle` waits for a complete document and a 500ms quiet window in CMG's page-side request log.
- `waitForNetworkIdle`: No positional arguments. This is the first-class provider-style action for `waitForLoadState "networkidle"`. `networkIdle` is an alias.
- `waitForNavigation`: Optional URL substring expected in `location.href`.

Output:

- `RELOADED <line> <url>` after reloading the current URL.
- `BACK <line> <url>` after browser history moves back.
- `FORWARD <line> <url>` after browser history moves forward.
- `URL <line> <url>` when `waitForUrl` or `toHaveURL` matches.
- `TITLE <line> <title>` when `toHaveTitle` matches.
- `LOAD_STATE <line> <state>` when the requested load state is reached.
- `NETWORK_IDLE <line> <state>` when the provider-style network idle action succeeds.
- `NAVIGATION <line> <json>` when the requested navigation URL and state are reached.

These actions do not move the virtual pointer. Use `step`, `caption`, or a `gif` block when a GIF should narrate a non-visual navigation wait.

`waitForLoadState "networkidle"`, `waitForNetworkIdle`, `networkIdle`, and `waitForNavigation waitUntil=networkidle` use CMG's in-page request log as a quiet-window signal, so they work best after CMG has installed page network hooks through route, request waits, or network environment actions.

## `waitForElement`

```text
waitForElement "<selector>" timeout=5000
waitForSelector "<selector>" timeout=5000
waitForSelector "<selector>" state=visible timeout=5000
```

`waitForElement` waits until a selector exists in any available page target. `waitForSelector` is a Playwright/Puppeteer-style wait that can target selector states and reports a parseable output line.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `5000`.
- `state`: Optional for `waitForSelector`. Supports `attached`, `detached`, `visible`, and `hidden`. Default is `attached`.

Output:

- `SELECTOR <line> <selector>` for `waitForSelector`.
- `SELECTOR <line> <selector> state=<state>` when `waitForSelector` uses an explicit state.

Example:

```text
waitForElement "#openProfileDialog" timeout=5000
waitForSelector "#profileDialog" state=visible timeout=5000
waitForSelector "#toast" state=hidden timeout=10000
```

## `waitForFunction` And `waitForTimeout`

```text
waitForFunction "window.appReady === true" timeout=10000
waitForTimeout 500
```

`waitForFunction` polls a browser-side JavaScript expression until it returns a truthy value. `waitForTimeout` is a Playwright-style alias for a fixed delay. These actions are available in both direct browser-control scripts and `cmg run`.

Options:

- `timeout`: Optional for `waitForFunction`. Default is `5000`.

Output:

- `FUNCTION <line> <value>` when the expression becomes truthy.
- `WAIT_TIMEOUT <line> <milliseconds>` after a timeout wait completes.

These actions do not move the virtual pointer. Wrap them in `step`, `caption`, or a `gif` block when a GIF should explain the wait.

## `click`

```text
click "<selector>"
click "<selector>" button=middle clickCount=2 delay=50 modifiers=Control+Shift x=8 y=12
```

Clicks the element in the selected browser. `click` does not scroll automatically; the element center must already be inside the current viewport. Use `scrollIntoView` first when the script should move the page.

When no click options are present, CMG uses the browser-native click path. When click options are present, CMG resolves the selector or rich locator, moves the GIF virtual pointer to the target, and dispatches a page-facing pointer/mouse sequence against that element.

Options:

- `button`: Optional mouse button: `left`, `right`, or `middle`. Default is `left`.
- `clickCount` or `count`: Optional number of clicks. Must be at least `1`.
- `delay`: Optional milliseconds between repeated clicks. Must be zero or greater.
- `modifiers`: Optional comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `x` / `y`: Optional offsets inside the target element. Defaults to the element center. GIF recordings move the virtual pointer to the same offset.
- `holdAfterAction`: Optional post-action GIF hold duration in milliseconds. Use `0` to suppress the hold for this click.
- `clickPulse`: Optional GIF pulse style: `ring`, `ripple`, `dot`, `crosshair`, or `none`. Without an override, right-clicks use `crosshair`, middle-clicks use `dot`, and left clicks use the scoped default.

Example:

```text
click "#openProfileDialog"
click "#openProfileDialog" button=left clickCount=2 modifiers=Shift
```

## `tap` And `touchTap`

```text
tap "<selector>"
touchTap text=Save
tap x=120 y=240
```

Dispatches a touch-style pointer sequence and click against a selector or viewport coordinate. `touchTap` is an alias for `tap`.

Selector targets support the same rich locators as other pointer-aware actions, including `text=`, `role=`, `label=`, `testid=`, provider-style `getByText=`, `getByRole=`, `getByLabel=`, `getByTestId=`, `getByPlaceholder=`, `getByAltText=`, `getByTitle=`, `placeholder=`, `alt=`, `title=`, and `xpath=`.

Options:

- `x`: Required when using coordinates.
- `y`: Required when using coordinates.

Output:

- `TAP <line> <selector>` for selector targets.
- `TAP <line> <x>,<y>` for coordinate targets.

In GIF recordings, `tap` and `touchTap` use CMG's touch pointer ring instead of the arrow pointer, dispatch page-facing movement and hover events, then show a click pulse for the tap.

## `type`

```text
type "<selector>" "text"
pressSequentially "<selector>" "text"
type "<selector>" "text" delay=25
```

Focuses the element without scrolling, appends text to its current value, and dispatches `input` and `change` events. `pressSequentially` is a Playwright-style alias for the same visible typing path. The element center must already be inside the current viewport. Use `scrollIntoView` first when the script should move the page.

Options:

- `delay`: Optional milliseconds between typed characters. Must be zero or greater. Without GIF recording, omitting `delay` uses the fast native type path. With GIF recording, typing is progressive so every character can produce a frame; omitting `delay` uses the default visual typing delay.

Example:

```text
type "#profileName" "CMG Test Profile"
pressSequentially "#profileName" "CMG Test Profile" delay=25
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
press "Control+A"
press "Control+A" delay=25
```

Dispatches key down and key up behavior in the selected browser. When the key contains `+`, CMG treats it as a shortcut chord: modifier keys are pressed first, the final key is pressed, and modifiers are released in reverse order.

Options:

- `delay`: Optional hold duration in milliseconds between keydown and keyup. Must be zero or greater. For chords, modifier keys remain held while the final key uses this delay.

Example:

```text
press "Escape"
press "Control+A" delay=25
```

## `keyboardShortcut`, `shortcut`, `hotkey`, `keyDown`, `keyUp`, And `insertText`

```text
keyboardShortcut "Control+Shift+P"
shortcut "Control+S"
hotkey "Meta+K"
keyDown "Shift"
insertText "ABC"
keyUp "Shift"
```

Runs lower-level keyboard primitives similar to Playwright and Puppeteer. `keyboardShortcut`, `shortcut`, and `hotkey` press a chord such as `Control+S`, `Control+Shift+P`, or `Meta+K`. `Ctrl` normalizes to `Control`, `Cmd`/`Command` normalizes to `Meta`, `Option` normalizes to `Alt`, and `Esc` normalizes to `Escape`. `keyDown` and `keyUp` hold or release a single key. `insertText` inserts text into the currently focused editable element.

Output:

- `KEYBOARD_SHORTCUT <line> <chord>`
- `KEY_DOWN <line> <key>`
- `KEY_UP <line> <key>`
- `TEXT_INSERTED <line> <character-count>`

Keyboard actions do not move the virtual pointer. In GIF recordings, wrap them in `step` or `caption` when the recording should narrate the keyboard state.

## Clipboard Actions

```text
setClipboard "hello"
writeClipboard "hello"
readClipboard
clearClipboard
```

Installs a page-side clipboard shim and controls it from direct browser-control scripts or `cmg run`. This provides deterministic clipboard behavior without relying on operating system clipboard permissions. `writeClipboard` is an alias for `setClipboard`.

Output:

- `CLIPBOARD_SET <line> <character-count>`
- `CLIPBOARD <line> <text>`
- `CLIPBOARD_CLEARED <line>`

Clipboard actions do not move the virtual pointer. In GIF recordings, wrap them in `step` or `caption` when a viewer needs to know that clipboard state changed.

## `hover`

```text
hover "<selector>"
hover "<selector>" modifiers=Control+Shift x=8 y=12
```

Dispatches mouseover and mousemove events at the element center. With `x=`, `y=`, or `modifiers=`, CMG dispatches a page-facing pointer/mouse hover sequence at the configured element-relative point with the requested modifier flags. `hover` does not scroll automatically; the target point must already be inside the current viewport.

Options:

- `modifiers`: Optional comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `x` / `y`: Optional offsets inside the target element. Defaults to the element center. GIF recordings move the virtual pointer to the same offset.

Example:

```text
hover "#openProfileDialog"
hover "#canvas" modifiers=Control+Shift x=12 y=8
```

## `moveMouse`

```text
moveMouse "center"
moveMouse "bottom"
moveMouse x=100 y=200
moveMouse selector=".content-area" edge=bottom inset=16
```

Moves the GIF virtual pointer to a viewport-relative position. `moveMouse` is script-only and has no one-off CLI `browser control moveMouse` command. Without command-level `--gif` and outside an active `gif`, `recordVideo`, or `screencast` block, it is skipped and does not create or move the virtual pointer. In that skipped state, recorder-only arguments, variables, scoped selectors, options, or child bodies are ignored; inside an active recording, unsupported block bodies and invalid movement options fail clearly.

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
- `pointerDuration`: Optional GIF pointer movement duration in milliseconds. Overrides any parent recording block default for this move.
- `duration`: Alias for `pointerDuration`.
- `pointerEasing`: Optional GIF pointer movement easing. Supports `linear`, `ease-in`, `ease-out`, `ease-in-out`, and `spring`.
- `easing`: Alias for `pointerEasing`.
- `pointerSpeed`: Optional GIF pointer speed. Supports `slow`, `normal`, `fast`, `instant`, or a multiplier such as `1.5x`.
- `pointerPath`: Optional GIF pointer route. Supports `auto`, `direct`, `arc`, `manhattan`, `avoid-target`, or `avoid-center`.
- `clickPulse`: Optional GIF pulse style for click/tap/drop actions. Supports `ring`, `ripple`, `dot`, `crosshair`, or `none`.
- `holdAfterAction`: Optional post-action GIF hold duration in milliseconds. Use `0` to suppress the hold for this action.
- `holdAfterMove`: Optional GIF-only hold duration after the pointer finishes this `moveMouse` movement. Use it when the pointer should visibly settle before the next action.

Examples:

```text
moveMouse "center"
moveMouse x=240 y=320
moveMouse selector=".content-area" edge=bottom inset=24 pointerPath=manhattan holdAfterMove=500
```

Output:

- No payload line when a GIF recorder is active and the movement is captured.
- `GIF_MOVE_MOUSE <line> status=skipped reason=no-active-recording` when no GIF recorder is active.

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

## `mouseMove`, `mouseDown`, And `mouseUp`

```text
mouseMove "center"
mouseMove x=100 y=200
mouseMove selector=".canvas" edge=bottom inset=24
mouseDown "center"
mouseUp "center"
```

Runs lower-level mouse primitives similar to Playwright and Puppeteer mouse APIs. Unlike `moveMouse`, these actions are available with or without GIF recording. In GIF mode, `mouseMove` uses CMG's virtual pointer movement and frame capture. `mouseDown` and `mouseUp` move the pointer to the target before sending the button event.

In an active recording, `mouseDown` captures a compressed pressed pointer after the real browser down event. `mouseDownHold=<0..60000>` controls that evidence hold and defaults to `500`. `mouseUp` clears the pressed visual after the real release. Without a recorder, neither action injects virtual-pointer or evidence overlays.

Targets use the same forms as `moveMouse`:

- one alias argument: `center`, `top`, `bottom`, `left`, `right`, `topLeft`, `topRight`, `bottomLeft`, or `bottomRight`;
- `x=<pixels> y=<pixels>` viewport coordinates;
- selector-edge targeting with `selector=<selector> edge=<edge> inset=<pixels>`.

Recording options include `pointerDuration`, `pointerSpeed`, `pointerEasing`, `pointerContrast`, `targetCallout`, `targetCalloutThreshold`, `targetZoom`, `targetZoomThreshold`, `pagePosition`, `tabContext`, `focusPulse`, `teleportMarker`, and `mouseDownHold`. Parent recording defaults are inherited and action options override them locally.

Output:

- `MOUSE_MOVED <line> <x>,<y>`
- `MOUSE_DOWN <line> <x>,<y>`
- `MOUSE_UP <line> <x>,<y>`

## `scrollIntoView`

```text
scrollIntoView "<selector>"
```

Scrolls an element into the center of the viewport.

Example:

```text
scrollIntoView "#dragdrop"
```

## `scrollTo`, `scrollBy`, And `wheel`

```text
scrollTo bottom
scrollTo 0 0 selector="#pane"
scrollBy 0 160 selector="#pane"
wheel "#pane" deltaY=120
wheel center deltaX=0 deltaY=-80
wheel x=120 y=240 deltaY=100
```

Runs page scrolling and mouse-wheel style input in direct browser-control scripts and `cmg run`.

- `scrollTo`: Scrolls the window or `selector=<selector>` element to an absolute position. Accepts aliases `top`, `bottom`, `left`, `right`, positional `x y`, or `x=` / `y=` options.
- `scrollBy`: Scrolls the window or `selector=<selector>` element by a delta. Accepts positional `x y` or `x=` / `y=` options.
- `wheel`: Dispatches a `WheelEvent` and scrolls the target. Use `deltaX=` and `deltaY=`; default is `deltaX=0 deltaY=100`. A selector argument or `selector=` targets an element. Alias or `x=` / `y=` targets move the GIF pointer before dispatching.

Selectors support CMG rich locators. In GIF recordings, `wheel` moves the virtual pointer when a selector, alias, or coordinate target is provided. `scrollTo` and `scrollBy` capture the changed viewport but do not move the pointer by themselves.

## `select` And `selectOption`

```text
select "<selector>" "value"
selectOption "<selector>" "value"
selectOption "<selector>" optionLabel="Visible label"
selectOption "<selector>" optionValue="value"
selectOption "<selector>" index=2
```

Sets a select-like element value and dispatches `input` and `change` events. `selectOption` is a provider-style alias for the same behavior. Use a positional value for the fast native path, or use `optionLabel=`, `optionValue=`, or `index=` for provider-style selection by visible label, value, or zero-based option index. These actions do not scroll automatically; the element center must already be inside the current viewport.

`label=` is reserved for CMG rich locators such as `selectOption label=Plan optionValue=pro`, so script selection by visible option text uses `optionLabel=`. The grouped CLI exposes this as `--label`.

Example:

```text
select "#environment" "prod"
selectOption "#environment" "prod"
selectOption "#plan" optionLabel="Pro"
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
caption "Assertion passed" captionStyle=qa captionPosition=bottom captionSeverity=success
```

Options:

- `captionStyle` / `style`: Optional caption preset: `subtle`, `teaching`, `qa`, `bug-report`, or `compact`.
- `captionPosition` / `position`: Optional placement: `top`, `bottom`, `left`, `right`, or `auto`.
- `captionSeverity` / `severity`: Optional color intent: `info`, `success`, `warning`, or `error`.

`step "name" { ... }` accepts the same caption options for its leading caption. `recording`, `withRecording`, `gif`, `recordVideo`, and `screencast` can set `captionStyle=`, `captionPosition=`, and `captionSeverity=` as scoped defaults; the caption action can override them locally.

Recording timeline options:

- `duration=<milliseconds>` / `captionDuration=<milliseconds>` keeps the caption fully visible for that encoded duration.
- `fadeIn=<milliseconds>` captures two increasing-opacity frames before the hold.
- `fadeOut=<milliseconds>` captures two decreasing-opacity frames and then removes the caption.
- Scoped `captionDuration=`, `fadeIn=`, and `fadeOut=` apply to nested captions and `narrate` blocks.

These timing options affect active GIF recording only. Without a recorder, `caption` keeps its existing page message-bar behavior and does not create screenshots or a virtual pointer.

### `narrate`

```text
narrate "Explain this interaction" {
  click "#save"
  expectText "#status" "Saved"
}
```

`narrate` is a nestable teaching-style caption block. It shows the message, emits `NARRATE <line> "<message>"`, executes children in a `narrate <message>` context, and accepts the caption style/position/severity/timing options above. It can be nested inside control flow, loops, macros, and other narration blocks.

Successful assertions in active GIF recordings automatically replace the current caption with QA evidence containing expected and actual values. Sensitive selectors/values containing `password`, `token`, or `secret` are shown as `[masked]`. Set `assertionCaptions=false` on an assertion or recording scope to disable this.

Failed actions automatically capture a bug-report caption with action, line, and a bounded failure reason before the partial GIF is finalized. Set `failureCaptions=false` to disable it. Successful runs are unaffected.

## `highlight`

```text
highlight "#save"
highlight "getByRole=button|Save" message="Save button" color="#2563eb" duration=1500
```

Draws a temporary CMG-owned overlay around a selector or rich locator. This is useful for visual demos, agent feedback, screenshots, and GIFs where the viewer should know which element the script is talking about.

Options:

- `message`: Optional short message shown above the highlighted element.
- `color`: Optional border and message tag color. Default is `#f59e0b`.
- `duration`: Optional duration in milliseconds. Default is `1200`.

Output:

- `HIGHLIGHT <line> <selector> duration=<milliseconds>`

In GIF recordings, `highlight` moves the virtual pointer to the target before drawing the overlay, so the pointer, browser target, and visual callout stay aligned. The overlay is temporary and removes itself after the configured duration.

## `delay`

```text
delay 1000
```

Pauses execution for the specified number of milliseconds.

## `pauseGif`

```text
pauseGif 1000
```

Adds a recording-only hold frame for the specified number of milliseconds. `pauseGif` does not sleep the browser, does not change page state, and does not create or move the virtual pointer unless GIF recording is active. Without an active recorder it skips before applying recording-only arguments, variables, scoped selectors, options, or child bodies.

Holds at or above `longWaitThreshold` (default `2000`) render three visible progress stages and are compressed to `longWaitDuration` (default `1200`) in the GIF. The label retains the requested duration. Use `compressLongWaits=false` to preserve encoded duration or `waitProgress=false` for a single frame. Real `delay` / `waitForTimeout` actions still wait for their full runtime duration; only visual evidence is compressed.

Output:

- `GIF_PAUSE <line> milliseconds=<value> status=captured` when a GIF recorder is active.
- `GIF_PAUSE <line> status=skipped reason=no-active-recording` when the script or test is running without command-level `--gif` and outside any `gif`, `recordVideo`, or `screencast` block.

## `intro` And `outro`

```text
intro "Checkout walkthrough" duration=900
outro "Order completed" duration=1200
```

Capture full-viewport title cards without the virtual pointer. Each action accepts exactly one text argument and optional `duration=<milliseconds>`, which defaults to `1200` and must be greater than zero. Without an active recorder it skips before variable expansion and emits `GIF_INTRO ... status=skipped` or `GIF_OUTRO ... status=skipped`.

Recording scopes and `gif` / `recordVideo` / `screencast` blocks can instead set `intro=`, `outro=`, `introDuration=`, and `outroDuration=`. A scoped intro is captured before the first recorded child; a scoped outro is captured during finalization, including for a partial failure artifact.

## `hideFromGif`, `cutGif`, `speedUpGif`, And `slowDownGif`

```text
hideFromGif {
  click "#setup"
  waitForText "#status" "Ready"
}

speedUpGif factor=4 { pauseGif 1200 }
slowDownGif factor=2 { click "#important" }
```

All four actions require a block body and are nestable. `hideFromGif` and `cutGif` execute children while suspending frame capture, virtual-pointer DOM, automatic captions, checkpoints, and nested GIF artifacts. Browser state changes remain visible to later recorded actions.

`speedUpGif` divides encoded frame delays by `factor`; `slowDownGif` multiplies them. `factor` must be greater than zero and at most `100`. Nested rates compose and restore after their block. They alter only GIF playback timing, not browser waits, action ordering, or page event timing.

Without an active recorder, children execute normally, no virtual pointer is injected, and output includes `GIF_TIMELINE_BLOCK ... status=inactive`. Active blocks emit `status=applied` with their mode and factor.

## `recordCheckpoint`

```text
recordCheckpoint "after login"
```

Adds a named marker to the active GIF timeline JSON without capturing a frame or changing page state. Use it before or after important visual transitions when CI reports or agents need a stable bookmark in the sidecar metadata.

`recordCheckpoint` is metadata-only. It does not create or move the virtual pointer, and it is skipped when no GIF recorder is active. Without an active recorder it skips before applying recording-only arguments, variables, scoped selectors, options, or child bodies.

Output:

- `GIF_CHECKPOINT <line> name="<name>" status=recorded` when a GIF recorder is active.
- `GIF_CHECKPOINT <line> status=skipped reason=no-active-recording` when the script or test is running without command-level `--gif` and outside any `gif`, `recordVideo`, or `screencast` block.

## `showPointer` And `hidePointer`

```text
showPointer
hidePointer
```

Captures a recording-only pointer visibility frame. `showPointer` captures the current virtual pointer position; `hidePointer` removes the virtual pointer and captures the page without it. Use these when a GIF needs to briefly reveal unobstructed page state or reintroduce the pointer before the next visual action.

Without an active recorder, both actions skip before applying recording-only arguments, variables, scoped selectors, options, or child bodies. They do not create or move the virtual pointer outside command-level `--gif` or an active `gif`, `recordVideo`, or `screencast` block.

Output:

- `GIF_SHOW_POINTER <line> status=captured` or `GIF_HIDE_POINTER <line> status=captured` when a GIF recorder is active.
- `GIF_SHOW_POINTER <line> status=skipped reason=no-active-recording` or `GIF_HIDE_POINTER <line> status=skipped reason=no-active-recording` when no GIF recorder is active.

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
screenshot "<selector>" output="element.jpg" type=jpeg quality=80
screenshot "<selector>" output="stable.png" style=".clock{visibility:hidden}"
screenshot "<selector>" output="masked.png" mask="#clock;#ad" maskColor="#000000"
screenshot "<selector>" output="deterministic.png" animations=disabled caret=hide
screenshot "<selector>"
```

Captures a screenshot of an element. In GIF mode this is a visual action: CMG moves the virtual pointer to the target first so the recording shows what is being captured.

Options:

- `output`: Optional file path. Without it, stdout receives a `data:image/png;base64,...` or `data:image/jpeg;base64,...` result.
- `type`: Optional `png`, `jpeg`, or `jpg`. Default is `png`.
- `quality`: Optional JPEG quality from `0` to `100`. Valid only when `type=jpeg` or `type=jpg`.
- `omitBackground`: Optional boolean. Allows a transparent background when the browser supports it.
- `style`: Optional CSS applied only while the screenshot artifact is captured, then removed.
- `stylePath`: Optional CSS file applied only while the screenshot artifact is captured, then removed. Cannot be combined with `style`.
- `mask`: Optional semicolon-separated selectors or rich locators to cover while the screenshot artifact is captured, then removed.
- `maskColor`: Optional CSS color used for masks. Default is `#ff00ff`.
- `animations`: Optional animation handling for the artifact. Use `disabled` to stop CSS animations/transitions during capture, or `allow` to leave them alone.
- `caret`: Optional caret handling for the artifact. Use `hide` to make text carets transparent during capture, or `initial` to leave them alone.

## `screenshotPage`

```text
screenshotPage output="page.png"
screenshotPage output="page-full.png" fullPage=true
screenshotPage output="page.jpg" type=jpeg quality=85
screenshotPage output="viewport-card.png" clipX=40 clipY=120 clipWidth=640 clipHeight=360
screenshotPage output="stable-page.png" stylePath="fixtures\screenshot.css"
screenshotPage output="masked-page.png" mask="#clock;hasText=.ad|Sponsored" maskColor="#000000"
screenshotPage output="deterministic-page.png" animations=disabled caret=hide
screenshotPage
```

Captures a screenshot of the primary page target. Page screenshots do not move the virtual pointer; selector screenshots do.

Options:

- `output`: Optional file path. Without it, stdout receives a `data:image/png;base64,...` or `data:image/jpeg;base64,...` result.
- `fullPage`: Optional boolean. Default is `false`. When `true`, captures the full scrollable page instead of only the current viewport.
- `type`: Optional `png`, `jpeg`, or `jpg`. Default is `png`.
- `quality`: Optional JPEG quality from `0` to `100`. Valid only when `type=jpeg` or `type=jpg`.
- `omitBackground`: Optional boolean. Allows a transparent background when the browser supports it.
- `style`: Optional CSS applied only while the screenshot artifact is captured, then removed.
- `stylePath`: Optional CSS file applied only while the screenshot artifact is captured, then removed. Cannot be combined with `style`.
- `mask`: Optional semicolon-separated selectors or rich locators to cover while the screenshot artifact is captured, then removed.
- `maskColor`: Optional CSS color used for masks. Default is `#ff00ff`.
- `animations`: Optional animation handling for the artifact. Use `disabled` to stop CSS animations/transitions during capture, or `allow` to leave them alone.
- `caret`: Optional caret handling for the artifact. Use `hide` to make text carets transparent during capture, or `initial` to leave them alone.
- `clipX` / `clipY` / `clipWidth` / `clipHeight`: Optional page or viewport clip rectangle in CSS pixels. `clipWidth` and `clipHeight` must be greater than `0`. With `fullPage=true`, the clip is relative to the page document; otherwise it is relative to the current viewport.

Clipped, styled, masked, animation-stabilized, and caret-stabilized page screenshots do not move the virtual pointer. Element screenshots still move the pointer before capture in GIF mode. Temporary screenshot artifact changes are removed before later actions continue, so GIF frames keep showing the real page state unless the script changes it directly.

## `printPdf`

```text
printPdf path="demo-output\page.pdf" printBackground=true
pdf path="demo-output\page-landscape.pdf" landscape=true scale=0.9
printPdf path="demo-output\a4.pdf" format=A4 marginTop=10mm marginBottom=10mm pageRanges="1-2,4"
```

Prints the current page to a PDF file. `pdf` is an alias for `printPdf`. This action is available in both direct browser-control scripts and `cmg run`.

Options:

- `path`: Required output PDF path.
- `landscape`: Optional boolean. Default is `false`.
- `printBackground`: Optional boolean. Default is `true`.
- `scale`: Optional positive number. Default is `1`.
- `format`: Optional paper format: `Letter`, `Legal`, `Tabloid`, `Ledger`, or `A0` through `A6`.
- `width` / `height`: Optional custom paper size. Values accept bare inches, `in`, `cm`, `mm`, or `px`.
- `marginTop` / `marginRight` / `marginBottom` / `marginLeft`: Optional page margins using the same size syntax.
- `pageRanges`: Optional pages to print, for example `1-3,5`.
- `preferCssPageSize`: Optional boolean. Prefer CSS `@page` size when the browser supports it.

Output:

- `PDF <line> <path>` on success.

PDF generation does not move the virtual pointer. It is supported through Chrome and Edge page printing and Firefox WebDriver BiDi printing.

## `assertText`

```text
assertText "<selector>" "expected text"
assertText "<selector>" "expected text" timeout=5000
assertText "<selector>" "^Ready \\d+$" match=regex ignoreCase=true
contains "expected text"
contains "<selector>" "expected text"
containsText "<selector>" "expected text"
toContainText "<selector>" "expected text"
waitForText "<selector>" "expected text" timeout=5000
notContains "unexpected text"
notContainsText "<selector>" "unexpected text"
toNotContainText "<selector>" "unexpected text"
expectNoText "<selector>" "unexpected text"
toHaveNoText "<selector>" "unexpected text"
toHaveNotText "<selector>" "unexpected text"
```

Reads an element's visible text and fails unless it matches the expected text. Negative aliases fail when matching text is still present. When `timeout` is provided, CMG polls the element text until it matches, becomes absent, or the timeout expires. The DSL `expectText`, `toHaveText`, `toContainText`, `containsText`, `waitForText`, `contains`, `expectNoText`, `expectNotText`, `notContains`, `notContainsText`, `toNotContainText`, `toHaveNoText`, and `toHaveNotText` actions use this assertion path and support the same options.

`contains "text"`, `toContainText "text"`, `notContains "text"`, and `toNotContainText "text"` check the page `body`, which matches the common provider pattern for finding text anywhere on the page. Selector forms and the other aliases check the target selector or rich locator.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `0`, which checks once.
- `match`: `contains`, `exact`, or `regex`. Default is `contains`.
- `ignoreCase`: Optional boolean. Use `true` for case-insensitive text matching.

Example:

```text
assertText "#lastDialogAction" "None"
contains "CMG Browser Control Test Page"
notContains "Unhandled error"
expectText "h1" "^CMG Browser" match=regex
waitForText "h1" "CMG Browser Control Test Page" timeout=5000
```

## `evaluate`

```text
evaluate "document.title"
evaluateOnSelector "#save" "element.textContent"
evalOnSelector "#save" "element => element.textContent"
evaluateAll ".item" "elements => elements.length"
evalAll ".item" "elements.map(element => element.textContent).join(', ')"
```

Evaluates JavaScript in the primary page target and prints the returned value as an `EVALUATE` result line.

`evaluateOnSelector` and `evalOnSelector` resolve a selector or rich locator, expose it as `element`, and return either the value of the expression or the result of a function/arrow function called with that element. `evaluateAll` and `evalAll` expose all matching CSS elements as `elements`.

The output payload is just the returned value. A `set` block can capture it without storing the `PASS` or `EVALUATE` prefix.

## `expectEval`, `assertEval`, `expectExpression`, And `assertExpression`

```text
expectEval "window.appReady === true"
assertEval "document.title" equals="Checkout"
expectExpression "document.body.innerText" contains="Saved"
assertExpression "window.retryCount < 3" timeout=5000
```

Evaluates JavaScript in the primary page target and fails unless the returned value matches the requested assertion. These aliases are shared by direct browser-control scripts and `cmg run`.

Options:

- `equals` or `eq`: Optional exact expected value.
- `contains`: Optional substring expected in the evaluated value.
- `timeout`: Optional polling timeout in milliseconds. Default is `0`, which checks once.

When no matcher option is supplied, the value must be truthy. Empty text, `false`, `0`, `null`, and `undefined` are falsey.

Output:

- `EXPECT_EVAL <line> <value>` when the assertion passes.

Evaluated assertions do not move the virtual pointer. Wrap them in `step`, `caption`, or `gif` blocks when a recording should narrate the check.

## `url`, `title`, `content`, And `setContent`

```text
url
title
content
setContent "<main>CMG</main>"
```

Reads or replaces page-level metadata and HTML content. These are shared actions for direct browser-control scripts and `cmg run`.

Output:

- `URL <line> <url>` for the current page URL.
- `TITLE <line> <title>` for the current document title.
- `CONTENT <line> <html>` for `document.documentElement.outerHTML`.
- `CONTENT_SET <line> length=<character-count>` after replacing the document with `setContent`.

`url`, `title`, and `content` do not move the virtual pointer. `setContent` changes the page without pointer movement; wrap it in `step`, `caption`, or a `gif` block when a recording should narrate the page content change.

## `textContent`, `innerText`, `inputValue`, `getAttribute`, `computedStyle`, And `property`

```text
textContent "#status"
innerText "#status"
inputValue "#email"
getAttribute "#profile" "href"
computedStyle "#status" "display"
property "#status" "dataset.state"
count ".row"
boundingBox "#card"
allTextContents ".item"
allInnerTexts ".item"
```

Reads element values from a selector or rich locator. These are non-visual getter actions for direct browser-control scripts and `cmg run`; they do not move the virtual pointer. Missing elements fail with the selector-specific reason from the browser.

`computedStyle` reads a CSS property from `getComputedStyle(element)`. `property` reads a dot-separated JavaScript property path from the element, such as `dataset.state`, and returns strings directly or JSON for structured values. `count` and `locatorCount` return the number of matching elements. `boundingBox` returns a JSON object with `x`, `y`, `width`, and `height`. `allTextContents` and `allInnerTexts` return JSON arrays.

The output payload is just the retrieved value, so a `set` block stores only that value:

```text
set href {
  getAttribute "#profile" "href"
}
```

Output:

- `TEXT <line> <text>` for `textContent` and `innerText`.
- `VALUE <line> <value>` for `inputValue`.
- `ATTRIBUTE <line> <value>` for `getAttribute`.
- `STYLE <line> <value>` for `computedStyle`.
- `PROPERTY <line> <value>` for `property`.
- `COUNT <line> <number>` for `count` and `locatorCount`.
- `BOUNDING_BOX <line> <json>` for `boundingBox`.
- `TEXTS <line> <json-array>` for `allTextContents` and `allInnerTexts`.

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

## `exposeFunction` And `exposeBinding`

```text
exposeFunction cmgAdd "(a, b) => a + b"
exposeBinding cmgBinding "(source, value) => `${source.name}:${value}`"
```

Installs a named page-side function in the current page and future navigations. `exposeFunction` calls the supplied JavaScript function with the page arguments. `exposeBinding` calls the supplied function with a source object as the first argument, followed by page arguments. The source object exposes `page`, `frame`, and `name` fields.

Arguments:

- First argument: JavaScript identifier to add to `window`.
- Second argument: JavaScript function expression to execute when the page calls the exposed name.

Output:

- `EXPOSED_FUNCTION <line> <name>` when the function is installed.

CMG exposes deterministic page-side functions instead of host-process callbacks. This keeps the feature available in both direct scripts and `cmg run`, including Firefox, reports, traces, and GIF narration. These actions do not move the virtual pointer.

## `addScriptTag` And `addStyleTag`

```text
addScriptTag "window.__runtimeTag = true;"
addScriptTag url="https://example.com/app.js"
addScriptTag path="fixtures\runtime.js"
addStyleTag "body { outline: 2px solid red; }"
addStyleTag url="https://example.com/app.css"
```

Injects a runtime `<script>`, `<style>`, or stylesheet `<link>` into the current page. These actions are available in direct browser-control scripts and `cmg run`.

Inputs:

- `url`: Optional script or stylesheet URL.
- `path`: Optional local file path whose content is injected.
- `content`: Optional inline content.
- inline positional content: Optional single content argument when no option is used.

Output:

- `SCRIPT_TAG <line> <content|url>` when a script tag is added.
- `STYLE_TAG <line> <content|url>` when a style tag or stylesheet link is added.

Tag injection actions do not move the virtual pointer. Wrap them in `step`, `caption`, or a `gif` block when the recording should narrate page setup.

## `setViewport`, `viewport`, And `setViewportSize`

```text
setViewport width=1280 height=720
setViewport width=390 height=844 deviceScaleFactor=2 isMobile=true hasTouch=true
viewport 390 844
setViewportSize 390 844
```

Sets viewport metrics for the primary page target. `viewport` is a Cypress-style alias and `setViewportSize` is a Playwright-style alias. The aliases accept either positional width/height arguments or the same options as `setViewport`.

Required options:

- `width`
- `height`

Optional options:

- `deviceScaleFactor`: Device scale factor. Default is `1`.
- `isMobile`: Optional boolean. Default is `false`.
- `hasTouch`: Optional boolean. Default is `false`.

Output:

- A normal `PASS <line> <action>` line. Viewport actions do not emit a separate data line.

## `emulate`

```text
emulate device="Pixel 7"
emulate "iPhone 13" locale=en-GB timezone=Europe/London
emulate width=390 height=844 deviceScaleFactor=2 isMobile=true hasTouch=true userAgent="CMG Mobile" locale=en-GB colorScheme=dark reducedMotion=reduce
emulate timezone=Europe/London geolocation="51.5,-0.1" permissions=geolocation
emulateMedia media=print colorScheme=dark reducedMotion=reduce forcedColors=active contrast=more
```

Applies browser-environment overrides for both direct browser-control scripts and `cmg run` tests. `width` and `height` use the browser protocol viewport override. Other options install page-side overrides in the current page context.

Named devices expand to viewport, scale, mobile, touch, and user-agent values. Explicit options override the preset values, so `emulate device="iPad Pro" width=1024 height=900` keeps the iPad user agent while changing the viewport size.

Options:

- `device`: Optional named device preset. Supported names are `iPhone 13`, `iPhone SE`, `Pixel 5`, `Pixel 7`, `Galaxy S9+`, `iPad`, `iPad Pro`, and `Desktop Chrome`.
- `width`: Viewport width in CSS pixels. Must be used with `height`.
- `height`: Viewport height in CSS pixels. Must be used with `width`.
- `deviceScaleFactor`: Optional viewport device scale factor. Default is `1`.
- `isMobile`: Optional boolean for mobile viewport metrics. Default is `false`.
- `hasTouch`: Optional boolean for touch viewport hints. Default is `false`.
- `userAgent`: Page-visible `navigator.userAgent` value.
- `locale`: Page-visible `navigator.language` and `navigator.languages` value.
- `timezone`: Reported `Intl.DateTimeFormat().resolvedOptions().timeZone`.
- `colorScheme`: Media query result for `prefers-color-scheme`, for example `dark` or `light`.
- `reducedMotion`: Media query result for `prefers-reduced-motion`, for example `reduce` or `no-preference`.
- `geolocation`: Stubbed coordinates as `<latitude>,<longitude>`.
- `permissions`: Comma-separated permission names that `navigator.permissions.query` should report as `granted`.

Output:

- `EMULATE <line> <option-names>` on success.

`emulateMedia` is the provider-style media-only action. It installs the same page-side `matchMedia` override immediately and for future navigations. It accepts `media=screen|print`, `colorScheme=light|dark|no-preference`, `reducedMotion=reduce|no-preference`, `forcedColors=active|none`, and `contrast=more|less|custom|no-preference`.

Output:

- `MEDIA <line> <key=value...>` on success.

Media emulation is non-visual by itself. GIF recording logs the step and captures later visible changes, but it does not move the virtual pointer.

## `setGeolocation`, `grantPermissions`, And `clearPermissions`

```text
setGeolocation "51.5,-0.1" accuracy=10
setGeolocation latitude=51.5 longitude=-0.1
grantPermissions "geolocation" "notifications"
grantPermissions permissions="geolocation,notifications"
clearPermissions
setJavaScriptEnabled false
javaScriptEnabled true
bypassCSP true
serviceWorkers block
serviceWorkers allow
setServiceWorkers block
```

Controls page-visible geolocation and permission query state without changing the rest of the emulated environment. These actions are available in both direct browser-control scripts and `cmg run`.

Arguments:

- `setGeolocation`: Optional `<latitude>,<longitude>` positional argument.
- `grantPermissions`: One or more permission names. Alternatively, pass a comma-separated `permissions=` option.
- `setJavaScriptEnabled` or `javaScriptEnabled`: `true` or `false` for CMG's dynamic-script blocker.
- `bypassCSP`: `true` or `false` for removing page CSP meta tags.
- `serviceWorkers` or `setServiceWorkers`: `allow` or `block` for page `navigator.serviceWorker.register()`.

Options:

- `latitude`: Latitude for `setGeolocation` when no positional coordinate argument is used.
- `longitude`: Longitude for `setGeolocation` when no positional coordinate argument is used.
- `accuracy`: Optional coordinate accuracy in meters. Default is `1`.
- `permissions`: Optional comma-separated permission list for `grantPermissions`.

Output:

- `GEOLOCATION <line> <latitude>,<longitude> accuracy=<accuracy>` when geolocation is set.
- `PERMISSIONS <line> <comma-separated-permissions>` when permissions are granted.
- `PERMISSIONS_CLEARED <line>` when all page-side permission grants are cleared back to `prompt`.
- `JAVASCRIPT_ENABLED <line> <true|false>` when dynamic script blocking changes.
- `CSP_BYPASS <line> <true|false>` when page-side CSP bypass changes.
- `SERVICE_WORKERS <line> <allow|block>` when service-worker registration mode changes.

These actions do not move the virtual pointer. In GIF recordings, wrap them in `step`, `caption`, or `gif` blocks when the permission or location change should be narrated.

## `dragAndDrop`

```text
dragAndDrop "<sourceSelector>" "<targetSelector>"
dragTo "<sourceSelector>" "<targetSelector>"
dragTo "<sourceSelector>" "<targetSelector>" sourceX=8 sourceY=8 targetX=24 targetY=16
dragAndDrop "<sourceSelector>" {
  delay 200
  hover "<selector>"
  drop "<targetSelector>"
}
```

Dispatches drag-and-drop DOM events from the source element to the target element. `dragTo` is a provider-style alias for the simple two-selector drag path.

Simple drag options:

- `sourceX` / `sourceY`: Optional element-relative drag start offsets. Defaults to the source center.
- `targetX` / `targetY`: Optional element-relative drop offsets. Defaults to the target center.
- `pointerDuration`: Optional GIF drag travel duration in milliseconds. For `dragAndDrop`, this controls movement while dragging and does not change the initial move to the source.
- `sourcePointerDuration`: Optional GIF duration for the initial move to the source.
- `targetPointerDuration`: Optional GIF duration for the drag movement to the target.
- `pointerSpeed`: Optional GIF pointer speed. Supports `slow`, `normal`, `fast`, `instant`, or a multiplier such as `1.5x`.
- `pointerEasing`: Optional GIF pointer easing for pointer movement.
- `dragEasing`: Optional GIF easing for drag travel.
- `pressedPointer`: Optional GIF drag visual state. Defaults to `true` while recorded drag is active; set `false` to keep the normal pointer shape.
- `dragTrail`: Optional GIF drag trail line. Defaults to `false`.
- `dragBreadcrumbs`: Optional GIF breadcrumb dots for held-pointer drag movement. Defaults to `false`.
- `preDragHold`: Optional hold in milliseconds before starting the page drag.
- `dragHold`: Optional hold in milliseconds while the drag is active before dropping.
- `postDropHold`: Optional hold in milliseconds after the drop pulse.
- `clickPulse`: Optional GIF drop pulse style. Supports `ring`, `ripple`, `dot`, `crosshair`, or `none`.

Simple drag-and-drop does not scroll automatically. The source and target points must both already be inside the current viewport. Use `scrollIntoView` and, when needed, a large enough viewport before dragging. In GIF recordings, the virtual pointer, page drag lifecycle, and default drag ghost use the same source and target points.

Example:

```text
dragAndDrop "[data-command='browser launch']" "#dropQueue"
dragTo "[data-command='browser launch']" "#dropQueue" sourceX=8 sourceY=8 targetX=24 targetY=16
```

The block form is the complex drag sequence. It has no inline target selector; the target is provided by the required `drop` child action.

Block `dragAndDrop` accepts the same GIF choreography options as scoped defaults for its child actions. Child actions inside the block can specify their own values to override the parent for that child only.

Allowed child actions:

- `delay <milliseconds>`: Pause while the drag is active. With `--gif`, frames are captured during the hold. Without an active recorder, this skips with `GIF_DRAG_DELAY ... status=skipped`.
- `hover "<selector>"`: Move the active drag pointer to another element. Without an active recorder, this skips with `GIF_DRAG_HOVER ... status=skipped`.
- `moveMouse "<alias>"`, `moveMouse x=<pixels> y=<pixels>`, or `moveMouse selector="<selector>" edge=<edge> inset=<pixels>`: Move the active drag pointer to a viewport-relative position or inside an element edge. Skips with `GIF_MOVE_MOUSE ... status=skipped` when no GIF recorder is active.
- `pauseGif <milliseconds>`: Add a recording-only drag hold. Skips with `GIF_PAUSE ... status=skipped` when no GIF recorder is active.
- `recordCheckpoint "<name>"`: Add a timeline marker without adding a frame. Skips with `GIF_CHECKPOINT ... status=skipped` when no GIF recorder is active.
- `showPointer` / `hidePointer`: Capture a pointer-visible or pointer-hidden drag frame. Skips with `GIF_SHOW_POINTER ... status=skipped` or `GIF_HIDE_POINTER ... status=skipped` when no GIF recorder is active.
- `scrollIntoView "<selector>"`: Scroll an element into view before continuing.
- `waitForElement "<selector>" timeout=5000`: Wait for an element before continuing.
- `drop "<selector>"`: Finish the drag on the target selector. Required exactly once.

Child recording options:

- `hover` and `moveMouse` can set `pointerDuration=`, `pointerSpeed=`, and `pointerEasing=`.
- `moveMouse` can also use `duration=` and `easing=` aliases.
- `hover` and `moveMouse` can set `pointerPath=auto|direct|arc|manhattan|avoid-target|avoid-center`.
- `hover`, `moveMouse`, `delay`, and `drop` can set `pressedPointer=`, `dragTrail=`, and `dragBreadcrumbs=`.
- `moveMouse` can set `holdAfterMove=` to keep the held pointer visibly settled after that movement.
- `drop` can set `dropPointerDuration=`, `dragPath=`, `postDropHold=`, and `clickPulse=`.

Rules:

- The block form must have exactly one source selector argument.
- The block must contain exactly one `drop`.
- No actions are allowed after `drop`.
- Other child actions fail with `Action '<name>' is not supported inside block dragAndDrop.`
- With `--gif`, recorded drags use one synthetic drag lifecycle and dispatch exactly one `drop` event. The block form dispatches it at the `drop "<selector>"` step.
- With `--gif`, CMG keeps the drag lifecycle active while the body runs so page-owned drag ghosts can stay visible.
- With `--gif`, every automatic pointer movement dispatches browser movement and hover events, including movement before `click`, `type`, `clear`, `hover`, `select`, and `dragAndDrop`.
- With `--gif`, block drag bodies also dispatch DOM `pointerdown`/`mousedown`, held `pointermove`/`mousemove`, and `pointerup`/`mouseup` so page drag state and edge-autoscroll code can react while `moveMouse "bottom"` and `delay` run.
- `pointerPath=` controls ordinary pointer movement into the drag body. `dragPath=` controls held-pointer movement during the drag and can be set on the parent `dragAndDrop` block or an individual `drop` child.
- During recorded drags, the virtual pointer visually compresses by default to show the held state. Use `pressedPointer=false` to disable it. Use `dragTrail=true` or `dragBreadcrumbs=true` on the drag block or individual drag children when long drags need path evidence.
- Without `--gif` or an active `gif` block, block-drag choreography-only children skip: `delay` reports `GIF_DRAG_DELAY ... status=skipped`, `hover` reports `GIF_DRAG_HOVER ... status=skipped`, `moveMouse` reports `GIF_MOVE_MOUSE ... status=skipped`, `pauseGif` reports `GIF_PAUSE ... status=skipped`, `recordCheckpoint` reports `GIF_CHECKPOINT ... status=skipped`, `showPointer` reports `GIF_SHOW_POINTER ... status=skipped`, and `hidePointer` reports `GIF_HIDE_POINTER ... status=skipped`. Setup children such as `scrollIntoView` and `waitForElement` still run before CMG performs the fallback native `dragAndDrop`.
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

dragAndDrop ".todo-card" pointerDuration=1200 dragTrail=true dragHold=250 {
  hover ".lane" pointerDuration=700
  moveMouse selector=".board" edge=bottom duration=300 easing=linear dragBreadcrumbs=true
  drop ".done-column" dropPointerDuration=450 postDropHold=800 clickPulse=ripple
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

## `waitForTab` And `waitForPopup`

```text
waitForTab count=2 timeout=5000
waitForPopup count=2 timeout=5000
```

Polls available page targets until at least `count` tabs exist. `waitForPopup` is a provider-style alias for the same behavior. Use either action after a pointer action that opens a popup or after `openTab`.

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

## `captureConsole`, `listConsole`, `waitForConsole`, `expectNoConsole`, And `toHaveNoConsole`

```text
captureConsole
listConsole level=error
waitForConsole "^saved$" match=regex ignoreCase=true level=info timeout=5000
expectNoConsole level=error timeout=250
toHaveNoConsole "deprecated" match=exact level=warn
```

Lists captured console messages, waits for a captured console message, or asserts that no matching console message was captured. CMG arms diagnostics automatically when it launches or attaches to a controlled browser/app. Captured messages live in `window.__cmgConsole` and continue accumulating between CMG commands from that point forward. Events before launch/attach/arming are not recoverable.

`captureConsole` is deprecated as a new-workflow requirement. It remains as an idempotent compatibility action that ensures capture is installed and does not clear existing entries. The old "arm before the interaction" workflow is easy for agents to miss and loses errors that occur between commands; prefer automatic launch/attach capture plus `listConsole` and assertions.

`listConsole` and `waitForConsole` check captured `log`, `info`, `warn`, and `error` calls. `expectNoConsole` and `toHaveNoConsole` default to the `error` level and can watch a short future window with `timeout=`.

For visual testing and debugging, interact first, then list and assert:

```text
click "#save"
screenshotPage output="artifacts/after-click.png"
listPageErrors
listConsole level=error
expectNoPageError timeout=250
expectNoConsole level=error timeout=250
```

If a matching console error is captured, `expectNoConsole` fails and reports the message in stderr. To dump captured entries for debugging, use `listConsole` with optional `level=`, text, `match=`, and `ignoreCase=` filters. To wait for one future or existing matching message, use `waitForConsole "." level=error match=regex timeout=250`, which emits a `CONSOLE` stdout line.

Options:

- `level`: Optional console level filter: `log`, `info`, `warn`, or `error`.
- `timeout`: Optional timeout in milliseconds. Default is `5000` for `waitForConsole` and `0` for no-console assertions.
- `match`: Optional text match mode: `contains`, `exact`, or `regex`. The `matches` alias is accepted for scripts.
- `ignoreCase`: Optional boolean for text matching. Default is `false`.

Output:

- `CONSOLE_CAPTURE <line>` when the hook is installed.
- `CONSOLE_LIST <line> count=<n>` when captured console entries are listed.
- `CONSOLE_ENTRY <line> index=<i> level=<level> text=<message>` for each listed console entry.
- `CONSOLE <line> <level>: <text>` when a matching message is found.
- `CONSOLE_OK <line> level=<level>` when no matching console message is found.

## `capturePageErrors`, `listPageErrors`, `waitForPageError`, `expectNoPageError`, And `toHaveNoPageError`

```text
capturePageErrors
listPageErrors
waitForPageError "Cannot read" match=contains ignoreCase=true timeout=5000
expectNoPageError timeout=250
toHaveNoPageError "ResizeObserver loop" match=exact timeout=250
```

Lists captured page errors, waits for a matching captured page error, or asserts that no matching page error was captured. CMG arms diagnostics automatically when it launches or attaches to a controlled browser/app. Captured `error` and `unhandledrejection` events live in `window.__cmgPageErrors` and continue accumulating between CMG commands from that point forward. Events before launch/attach/arming are not recoverable.

`capturePageErrors` is deprecated as a new-workflow requirement. It remains as an idempotent compatibility action that ensures capture is installed and does not clear existing entries. Prefer automatic launch/attach capture plus `listPageErrors` and assertions.

If a matching page error is captured, `expectNoPageError` fails and reports the message in stderr. To dump captured entries for debugging, use `listPageErrors` with optional text, `match=`, and `ignoreCase=` filters. To wait for one future or existing matching page error, use `waitForPageError "." match=regex timeout=250`, which emits a `PAGE_ERROR` stdout line.

Options:

- `timeout`: Optional timeout in milliseconds for `waitForPageError`. Default is `5000`.
- `timeout`: Optional observation window in milliseconds for no-page-error assertions. Default is `0`.
- `match`: Optional text match mode: `contains`, `exact`, or `regex`. The `matches` alias is accepted for scripts.
- `ignoreCase`: Optional boolean for text matching. Default is `false`.

Output:

- `PAGE_ERROR_CAPTURE <line>` when the hook is installed.
- `PAGE_ERROR_LIST <line> count=<n>` when captured page errors are listed.
- `PAGE_ERROR_ENTRY <line> index=<i> type=<type> source=<source> line=<pageLine> column=<pageColumn> text=<message>` for each listed page error.
- `PAGE_ERROR <line> <type>: <text>` when a matching page error is found.
- `PAGE_ERROR_OK <line>` when no matching page error is found.

Page-error actions do not move the virtual pointer. They are included in reports and traces, and can be wrapped with `step`, `caption`, or `gif` blocks when GIF narration is useful.

## `captureDialogs`, `setDialogBehavior`, `onDialog`, `handleDialog`, `dialogBehavior`, And `waitForDialog`

```text
captureDialogs
setDialogBehavior accept promptText="CMG"
setDialogBehavior dismiss
onDialog accept
handleDialog dismiss
dialogBehavior accept promptText="CMG"
waitForDialog "^Saved$" match=regex timeout=5000
```

Installs page-side dialog automation for `alert`, `confirm`, and `prompt` calls in the current page and future navigations. Captured dialogs are logged with their type, message, accepted state, and prompt value when available. Install this before the action that opens a dialog.

Arguments:

- `setDialogBehavior`, `onDialog`, `handleDialog`, and `dialogBehavior`: `accept` or `dismiss`.
- `waitForDialog`: Text expected in the dialog message.

Options:

- `promptText`: Optional text returned from accepted prompts.
- `timeout`: Optional for `waitForDialog`. Default is `5000`.
- `match`: Optional dialog message match mode: `contains`, `exact`, or `regex`. The `matches` alias is accepted for scripts.
- `ignoreCase`: Optional boolean for dialog message matching. Default is `false`.

Output:

- `DIALOG_CAPTURE <line>` when dialog capture is installed.
- `DIALOG_BEHAVIOR <line> <accept|dismiss>` when behavior changes through any behavior alias.
- `DIALOG <line> <json>` when a matching dialog is found.

Dialog actions do not move the virtual pointer. Wrap them in `step`, `caption`, or a `gif` block when the recording should narrate dialog handling.

## `waitForEvent`

```text
waitForEvent popup count=2 timeout=5000
waitForEvent dialog "Saved" timeout=5000
waitForEvent console "ready" level=info
waitForEvent request "/api/profile"
waitForEvent requestFinished "/api/profile"
waitForEvent requestFailed "/api/profile"
waitForEvent response pattern="/api/profile"
waitForEvent worker "worker.js"
waitForEvent serviceWorker "sw.js"
waitForEvent websocket "/socket"
waitForEvent websocketMessage "ready"
waitForEvent download directory="demo-output" pattern="*.csv"
```

Provider-style event wait that maps to CMG's explicit wait actions. Use it when an AI agent is translating Cypress, Puppeteer, or Playwright-style event waits into CMG scripts while preserving CMG's parseable output and failure reasons.

Arguments:

- First argument: event name. Supported values are `popup`, `page`, `tab`, `download`, `dialog`, `console`, `pageError`, `request`, `requestFinished`, `requestFailed`, `response`, `worker`, `serviceWorker`, `websocket`, and `websocketMessage`.
- Second argument: matcher text for events that require a message or URL matcher. `popup`, `page`, `tab`, and `download` do not need a matcher.

Options:

- `pattern`, `text`, `message`, or `url`: Matcher aliases when a second argument is not supplied.
- `timeout`: Optional timeout in milliseconds. Defaults to the target wait action default.
- Event-specific options are passed through, such as `count` for popup/tab waits, `level` for console waits, and `directory`/`pattern` for download waits.

Output:

- Uses the output shape of the mapped action, such as `TAB_COUNT`, `DOWNLOAD`, `DIALOG`, `CONSOLE`, `PAGE_ERROR`, `REQUEST`, `REQUEST_FINISHED`, `REQUEST_FAILED`, `RESPONSE`, or `WORKER_READY`.
- WebSocket event waits use `WEBSOCKET` or `WEBSOCKET_MESSAGE`.

Failures:

- Unknown events fail with the supported event list.
- Matcher-based events fail if no matcher argument or matcher option is supplied.
- Timeout and match failures use the same messages as the mapped wait action.

`waitForEvent` does not move the virtual pointer. It is included in reports and traces, and can be wrapped with `step`, `caption`, or `gif` blocks when the recording should narrate the wait.

## Frame Actions

```text
frameClick "#checkoutFrame" "#save"
frameClick "#checkoutFrame" "getByRole=button|Save"
frameType "#checkoutFrame" "#email" "agent@example.com"
frameFill "#checkoutFrame" "#name" "CMG"
frameHover "#checkoutFrame" "#help"
frameWaitForElement "#checkoutFrame" "#ready" timeout=5000
frameWaitForSelector "#checkoutFrame" "#ready" timeout=5000
frameAssertText "#checkoutFrame" "#status" "^Saved$" match=regex ignoreCase=true
frameExpectText "#checkoutFrame" "#status" "Saved"
frameToHaveText "#checkoutFrame" "#status" "Saved"
frameToContainText "#checkoutFrame" "#status" "Saved"
frameContains "#checkoutFrame" "#status" "Saved"
frameEvaluate "#checkoutFrame" "document.title"
frameTextContent "#checkoutFrame" "#status"
frameInnerText "#checkoutFrame" "#status"
frameInputValue "#checkoutFrame" "#email"
frameGetAttribute "#checkoutFrame" "#save" "disabled"
frameComputedStyle "#checkoutFrame" "#status" "display"
frameProperty "#checkoutFrame" "#status" "dataset.state"
frameCount "#checkoutFrame" ".row"
frameLocatorCount "#checkoutFrame" "getByRole=button|Save"
frameBoundingBox "#checkoutFrame" "#save"
frameAllTextContents "#checkoutFrame" ".row"
frameAllInnerTexts "#checkoutFrame" ".row"

frame "#checkoutFrame" {
  fill "#email" "agent@example.com"
  click "#save"
  contains "Saved"
  computedStyle "#save" "display"
  property "#save" "dataset.state"
}
```

Runs actions against a same-origin iframe selected from the top page. The first argument is the iframe selector. The second argument is the selector or JavaScript expression inside that frame.

The iframe selector itself is a top-page CSS selector. Element targets inside the frame accept CSS and CMG rich/provider locators such as `getByRole=button|Save`, `text=Saved`, `getByTestId=status`, `xpath=//button`, and `hasText=.row|Ready`.

Explicit frame actions also accept locator option form when the parser would otherwise treat a rich locator as an option, for example `frameClick "#checkoutFrame" getByRole=button|Save`.

`frame "<iframe>" { ... }` and `frameLocator "<iframe>" { ... }` are script-only structural blocks that scope supported child actions to the iframe. Supported child actions include `click`, `hover`, `type`, `pressSequentially`, `fill`, `waitForElement`, `waitForSelector`, `assertVisible`, text assertions, `evaluate`, and element getters such as `textContent`, `computedStyle`, and `property`. Child actions are rewritten before GIF recording, so pointer-aware frame actions still move the virtual pointer to the element's top-page coordinate inside the iframe.

`within` can be nested inside a frame block. In that case, child selectors are first composed under the `within` container, then rewritten to frame actions.

GIF behavior:

- `frameClick`, `frameType`, `frameFill`, and `frameHover` move the virtual pointer to the element's actual top-page coordinate inside the iframe before running.
- Non-visual frame actions do not move the pointer, but their outputs and failures are captured in reports and traces.
- `frameWaitForSelector` is an alias for `frameWaitForElement`.
- `frameAssertText`, `frameExpectText`, `frameToHaveText`, `frameToContainText`, and `frameContains` support `match=contains|exact|regex` plus `ignoreCase=true` for frame-local text assertions.
- Frame getter aliases include `frameTextContent`, `frameInnerText`, `frameInputValue`, `frameGetAttribute`, `frameComputedStyle`, `frameProperty`, `frameCount`, `frameLocatorCount`, `frameBoundingBox`, `frameAllTextContents`, and `frameAllInnerTexts`.

Output:

- `FRAME <line> <action>` for successful frame actions.
- `FRAME_EVALUATE <line> <result>` for `frameEvaluate`.
- `FRAME_TEXT`, `FRAME_VALUE`, `FRAME_ATTRIBUTE`, `FRAME_STYLE`, `FRAME_PROPERTY`, `FRAME_COUNT`, `FRAME_BOUNDING_BOX`, and `FRAME_TEXTS` for frame getter payloads.

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

Context actions do not move the virtual pointer. Later pointer-aware actions use the currently active context, so GIF recordings keep the same virtual pointer and event behavior inside that page.

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
set title {
  evaluate "document.title"
}
set currentUrl {
  url
}
```

Stores a variable for later `${name}` expansion.

`set` is a scripting-only action. It is not exposed as a standalone `browser control` CLI command because CLI action invocations do not share DSL scope.

Initial variables can also come from command-line `--var name=value` / `--env name=value` or runner declaration options such as `var.user=Ada`. These values are available before the first action runs. Declaration variables are inserted before macros and hooks, so helper macros can read them. A later `set` in the same script scope can replace the value.

The two-argument form stores a literal value. The block form runs the wrapped actions and stores only the payload from the last output-producing action. For example, `set title { evaluate "document.title" }` stores only the document title string, not the `PASS`, `EVALUATE`, or `SET` log text.

Block form rules:

- The first argument is the variable name.
- The block can contain any action that is valid in the current script type.
- The stored value comes from the final value-producing output line in the block. CMG removes status labels and metadata, so `evaluate`, element getters, macro `return`, storage getters, and similar actions store the actual value rather than `PASS`, action names, operation names, or keys.
- If the block does not produce any output, the `set` action fails.

For storage getters, a missing `localStorage`, `sessionStorage`, or cookie value is stored as an empty string:

```text
set token {
  localStorage get "token"
}

if ("${token}" == "") {
  caption "token was missing"
}
```

Output:

- Literal `set` writes only the normal `PASS <line> set ...` line.
- Block `set` includes the wrapped action output and `SET <line> <name> <value>`.

Example:

```text
set target "#openProfileDialog"
click "${target}"

set title {
  evaluate "document.title"
}
showMessageBar "Current page: ${title}"
```

## `fail`

```text
fail "Missing required setup"
```

Fails the current direct script or `cmg run` test with a custom reason. Use it for guard clauses inside macros, branches, loops, retries, and `try` blocks when an AI-authored script knows the current state should abort.

Inside a `try` block, `fail` is catchable like any other action failure:

```text
try {
  fail "Expected optional panel"
} catch error {
  caption "${error}"
}
```

Output:

- No action-specific stdout is emitted because the action fails immediately.
- The failure reason is `Line <line>: fail failed. <message>`.

`fail` is non-visual and does not move the virtual pointer. In GIF mode, frames captured before the failure are still written as a partial GIF. Wrap a preceding `caption` or `step` around the guard when the recording should explain why the run stopped.

## `skip`

```text
skip "Feature flag disabled"
```

Stops the current direct script or `cmg run` test as skipped. Use it for runtime guard clauses when a discovered browser, account, feature-flag, environment, or fixture state makes the rest of the flow not applicable.

Output:

- `SKIP <line> <reason>`

Behavior:

- In `browser control script`, `skip` stops later actions and exits successfully.
- In `cmg run`, `skip` marks the current test as `skipped`, writes `TEST SKIP <name>`, and emits skipped entries in JSON, HTML, and JUnit reports.
- A skipped test does not count toward `--max-failures`.

`skip` is non-visual and does not move the virtual pointer. In GIF mode, frames captured before the skip are still written.

## `expect`, `assert`, `softExpect`, And `softAssert`

```text
expect (${count} > 5)
assert (${mode} in "checkout" "billing") message="Unexpected mode"
expect evaluate "document.title" contains "CMG"
softExpect (${optionalCount} > 0) message="Optional widgets were missing"
expect.soft textContent "#status" == "ready"
```

Asserts a generic CMG condition without adding an `if` block. It uses the same condition engine as `if`, `elseif`, `while`, `until`, `doWhile`, and `doUntil`.

`expect` and `assert` fail immediately. `softExpect`, `softAssert`, `expect.soft`, and `assert.soft` record the failure, continue running later actions, and fail the direct script or `cmg run` test after all remaining actions finish. This is useful when a visual evidence run should collect several independent diagnostics before stopping.

Supported condition inputs:

- Static values, quoted strings, and variables such as `${count}`.
- Boolean operators `&&`, `||`, and `!`.
- Comparison operators `==`, `!=`, `>`, `>=`, `<`, and `<=`.
- Word operators `contains`, `matches`, and `in`.
- Value-producing actions such as `evaluate`, `title`, `url`, element getters, file reads, and macro calls.
- Assertion or wait actions such as `expectVisible`, `assertText`, and `waitForText`; the condition passes when the action succeeds.

Options:

- `message`: Custom failure reason.
- `reason`: Alias for `message`.

Output:

- `EXPECT <line> true` when the condition passes.
- `SOFT_EXPECT <line> true` when a soft condition passes.
- `SOFT_EXPECT <line> false <reason>` when a soft condition fails and execution continues.

Failure output:

- The action fails with the custom message when `message=` or `reason=` is provided.
- Otherwise the failure reason is `Expected condition to pass: <condition>`.
- If one or more soft assertions fail, the final run failure is `Soft assertion failure(s): <reason> | <reason>`.

These assertions are non-visual by themselves. If a condition runs a pointer-aware action, that child action still uses CMG's normal virtual pointer and GIF recorder hooks. With command-level `--gif`, soft assertion failures allow later visual actions to continue into the same recording before the final run is marked failed.

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

### `suite`, `describe`, And `context`

```text
suite "name" {
  test "case" {
    click "#run"
  }
}

describe "checkout" {
  it "submits payment" {
    click "#pay"
  }
}
```

Groups tests in reports. `describe` and `context` are Cypress/Mocha-style aliases for `suite`. Test names are emitted as `<suite> / <test>`.

### `test`, `it`, And `specify`

```text
test "name" {
  navigate "https://example.com"
}

it "name" {
  navigate "https://example.com"
}

test "focused" only=true {
  click "#run"
}

test "skipped" skip=true reason="Covered by API test" {
  click "#legacy"
}

test.only "focused with provider syntax" {
  click "#run"
}

test.fixme "known issue"
test.todo "queued coverage"
```

Defines a runnable test. `it` and `specify` are Cypress/Mocha-style aliases for `test`. `test.only`, `it.only`, `specify.only`, `test.skip`, `test.fixme`, and `test.todo` are provider-style declaration aliases for the matching `only=true` or `skip=true` options. `cmg run` exits `1` if any non-skipped test fails.

Options:

- `tag`: Optional comma-separated tags used by `cmg run --tag`.
- `only`: Optional focus flag. When any selected test has `only=true`, only focused tests run.
- `skip`: Optional skip flag. `skip=true` records `TEST SKIP <name>` and a skipped report entry without running the test actions.
- `reason`: Optional skip reason included in reports.

Skipped tests are successful for process exit purposes unless another selected test fails.

The same suffixes work on `suite`, `describe`, and `context`. Suite-level focus and skip options cascade to child tests; skipped suites keep all child tests skipped even when a child sets `skip=false`.

### `beforeAll`, `before`, `afterAll`, `after`, `beforeEach`, And `afterEach`

```text
beforeAll {
  navigate "https://example.com"
}

before {
  navigate "https://example.com"
}

beforeEach {
  resetContext
}

afterAll {
  caption "Finished"
}

after {
  caption "Finished"
}
```

Root hooks apply to every test file. Hooks inside a suite apply to that suite. `before` is an alias for `beforeAll`; `after` is an alias for `afterAll`. `beforeEach` and `afterEach` run around every selected test. Once hooks run before the first non-skipped selected test in their root or suite scope and after the last non-skipped selected test in that scope.

Once hooks are inserted into the first or last selected test script after macro registration. Use them for browser/page setup and teardown. Script variables created in a `beforeAll` hook are available only inside the test script where that hook runs; use top-level macros, imports, browser state, fixtures, or files for reusable state across tests.

### `step`

```text
step "Open dialog" {
  click "#open"
}
```

Adds a visible caption before running the wrapped actions. This works in direct browser-control scripts and `cmg run`, including nested blocks, macros, imports, `try`/`catch`, loops, `gif` blocks, and scoped selectors. In GIF mode, the caption appears in the recording before the wrapped visual actions run. If a child action fails, the failure keeps the child's line number and action name so agent callers can see the precise cause.

### Recording Settings

```text
recording pointerDuration=300 clickPulse=dot holdAfterAction=500 {
  click "#save"
}

withRecording pointerSpeed=slow pointerTheme=ring pointerColor="#dc2626" {
  gif "focused evidence" {
    click "#pay" pointerTheme=hand pointerColor="#16a34a"
  }
}

recordingDefaults quality=highest captionStyle=qa {
  setRecording pointerSpeed=fast
  previewRecordingSettings
  click "#save"
}
```

`recording`, `withRecording`, and `recordingDefaults` set scoped GIF recording defaults without starting a recording by themselves. They are equivalent block forms. The virtual pointer is still injected only when command-level `--gif` is active or a nested `gif`, `recordVideo`, or `screencast` block creates a recording. Without an active recorder, recording-only actions inside the scope still skip with their normal `GIF_* ... reason=no-active-recording` lines.

`setRecording option=value ...` changes defaults for subsequent actions in the current lexical scope and accepts no body. A nested block receives the current values, may change its own copy, and restores the parent values when it exits. It writes `RECORDING_SETTINGS <line> options=<effective-settings>` and does not start capture.

`previewRecordingSettings` accepts no arguments, options, or body. It writes the current effective defaults as `GIF_SETTINGS <line> options=<effective-settings>` during execution and does not need an active recorder.

The scope inherits through nested macros, loops, `try` / `catch` / `finally`, `within`, frame blocks, and nested recording blocks. Child action options override the scoped defaults locally. Unknown default names fail with the exact action, line, and option.

### Recording Annotations

```text
pointerStyle pointerTheme=hand pointerColor="#dc2626" pointerSize=40
annotateTarget "#save" "Primary action" color="#2563eb" duration=800
highlightTarget getByRole=button|Save message="Confirm changes"
recordVariable "status" label="Current state" duration=700
recordVariable "apiToken"
```

These actions require an active command-level GIF or `gif` / `recordVideo` / `screencast` block. Otherwise they write a `status=skipped reason=no-active-recording` line and do not resolve selectors, read variables, add overlays, capture screenshots, or inject a virtual pointer.

- `pointerStyle` accepts `pointerTheme=`, `pointerColor=`, `pointerSize=`, `pointerShadow=`, and `showPointer=`. It changes subsequent actions in the current lexical recording scope, writes `GIF_POINTER_STYLE <line> status=updated options=...`, and restores with the containing scope.
- `annotateTarget` and `highlightTarget` are aliases. They accept a selector/rich locator and optional positional text. `message=` is the option form; `color=` defaults to amber and `duration=` defaults to `1200`. The recorder moves its virtual pointer to the target, captures the live outline/label, and writes `GIF_TARGET_ANNOTATION <line> selector="..." duration=<ms> status=captured`.
- `recordVariable` accepts one variable name plus optional `label=`, caption style/position/severity/timing options, and `reveal=<true|false>`. It writes `GIF_VARIABLE <line> name="..." value="..." status=captured`. Names containing password, token, secret, authorization, cookie, or API key are shown as `[masked]` unless `reveal=true` is explicit. Undefined variables and invalid booleans fail with the exact reason.

### Conditional Recording

```text
gifIfChanged "saved state" output="artifacts/saved.gif" {
  click "#save"
  gifSnapshot "saved result" duration=700
}

gif.onFailure "diagnostic" output="artifacts/failure.gif" {
  assertText "#status" "Complete"
}
```

`gifIfChanged` and `gif.ifChanged` are aliases. They buffer normal virtual-pointer frames but write the artifact only when the final page screenshot differs from the baseline taken before the block. CMG pointer/caption UI is removed from both signatures. A matching page writes `GIF_SKIPPED <line> path="..." reason=unchanged`; any block failure retains partial evidence.

`gifOnFailure` and `gif.onFailure` are aliases. A passing block discards its buffered artifact and writes `GIF_SKIPPED <line> path="..." reason=passed`. A failing block writes its partial GIF before propagating the original failure; a skipped block uses `reason=skipped`.

`gifSnapshot` and `gif.snapshot` are aliases that add a named checkpoint plus a still hold inside an active recorder. They require one name and accept `duration=<milliseconds>`, defaulting to the recorder's post-action hold. They write `GIF_SNAPSHOT <line> name="..." status=captured`, or skip without screenshot/pointer work when no recorder is active.

Command-level `--gif` / runner `-gif` records the whole run and suppresses all nested conditional artifacts with `GIF_BLOCK_SUPPRESSED ... reason=command-level-recording`.

Options:

- `quality`: Default quality for nested recording blocks that create their own artifact.
- `pointerDuration`: Default pointer movement duration in milliseconds.
- `pointerSpeed`: Default pointer speed: `slow`, `normal`, `fast`, `instant`, or a multiplier such as `1.5x`.
- `pointerEasing`: Default easing: `linear`, `ease-in`, `ease-out`, `ease-in-out`, or `spring`.
- `pointerPath`: Default pointer route: `auto`, `direct`, `arc`, `manhattan`, `avoid-target`, or `avoid-center`. `auto` is the geometry-aware default.
- `dragPath`: Default route while the pointer is held during drag movement.
- `pointerTheme`: Default pointer theme: `arrow`, `hand`, `dot`, `ring`, `branded`, or `touch`.
- `pointerColor`: Default pointer CSS color. Use one CSS color value, not a declaration.
- `pointerSize`: Default pointer size in CSS pixels from `8` to `96`, or `auto`.
- `pointerShadow`: Default pointer shadow: `none`, `light`, `medium`, or `strong`.
- `showPointer`: Default pointer visibility for captured frames: `true`, `false`, or `auto`. Defaults to `auto`, which currently shows the pointer for pointer-aware evidence frames.
- `captionStyle`: Default caption style: `subtle`, `teaching`, `qa`, `bug-report`, or `compact`.
- `captionPosition`: Default caption position: `top`, `bottom`, `left`, `right`, or `auto`.
- `captionSeverity`: Default caption severity: `info`, `success`, `warning`, or `error`.
- `autoCaptions`: Automatic narration for supported visual child actions. Defaults to `false`.
- `eventCaptions`: Enables all privacy-safe event outcome captions.
- `networkCaptions`, `dialogCaptions`, `consoleCaptions`, `downloadCaptions`, `uploadCaptions`, `serviceWorkerCaptions`, `webSocketCaptions`, `workerCaptions`: Override individual event categories for inherited frames.
- `captionTemplate`: Automatic-caption template with `{action}`, `{selector}`, `{target}`, `{line}`, `{arguments}`, `{step}`, and `{assertion}`. `{step}` is the lexical execution context (or action name); `{assertion}` is the assertion action name or empty text.
- `intro` / `outro`: Optional opening and final full-viewport title-card text.
- `introDuration` / `outroDuration`: Optional title-card durations in milliseconds. Defaults to `1200`.
- `resultOutro`: Optional `true`/`false` generated passed, failed, or skipped final card. Explicit outro text/actions take precedence.
- `coalesceDuplicates`: Optional `true`/`false` exact consecutive-frame coalescing. Defaults to `true`.
- `sampleEvery`: Optional integer from `1` to `100` for intermediate pointer/drag movement sampling. Semantic/final frames are always retained.
- `pressedPointer`: Default held-pointer visual compression during recorded drags. Defaults to `true`.
- `dragTrail`: Default held-pointer trail line during recorded drags. Defaults to `false`.
- `dragBreadcrumbs`: Default held-pointer breadcrumb dots during recorded drags. Defaults to `false`.
- `clickPulse` / `pulse`: Default click/tap/drop pulse style: `ring`, `ripple`, `dot`, `crosshair`, or `none`.
- `holdAfterAction`: Default post-action hold in milliseconds.
- `preClickHold`: Default hold before click/tap dispatch after pointer movement.
- `postClickHold`: Default hold after click/tap pulse frames.
- `holdAfterMove`: Default hold after recorded `moveMouse` actions.
- `holdAfterNavigation`: Default hold after navigation actions and waits.
- `holdAfterAssertion`: Default hold after assertion actions.
- `holdOnFailure`: Default final failure-state hold for nested recording blocks.
- `fps`: Default GIF frame rate from `1` to `100`.
- `frameDelay`: Default GIF frame delay in milliseconds from `10` to `10000`. Overrides `fps`.
- `timeline`: Default timeline JSON sidecar behavior for nested recording blocks.
- `redact`: Semicolon-separated selectors or rich locators to mask on every inherited action frame.
- `redactStyle`: Default redaction style: `solid`, `blur`, or `replacement`.
- `redactColor`: CSS color used by solid masks and as the replacement background.
- `redactReplacement`: Text shown by replacement masks. Defaults to `[redacted]`.
- `redactPadding`: Extra CSS pixels around each explicit mask, from `0` to `100`.
- `autoRedact`: Automatic masking mode: `passwords` (default), `tokens`, `emails`, `payment`, `privacy`, or `none`. `privacy` combines every preset; `sensitive` aliases `tokens`.
- `redactionSafety`: `standard` (default) or `strict`. Strict mode refuses a frame when any visible password input is not covered.
- `accessibilityEvidence`: Enables keystroke, focus, accessible-name, high-contrast, and contrast-warning evidence for inherited frames.
- `showKeystrokes`: Shows safe key/chord labels. Text-entry values are never included.
- `showMouseButtons`: Labels low-level left-button down/up frames while preserving the real pressed pointer state.
- `focusEvidence`: Amplifies the actual focused element during frame capture.
- `accessibleNames`: Shows the targeted or focused control's derived role and name.
- `highContrast`: Uses high-contrast evidence colors.
- `contrastWarnings`: Shows a visual warning when the targeted control is below the applicable `4.5:1` or `3:1` WCAG contrast threshold.
- `reducedMotion`: Removes default pointer travel and inherited caption fades while retaining static action evidence.
- `highContrastPointer`: Uses the large yellow ring pointer preset with a strong dark edge.
- `debug`: Enables the complete capture-only diagnostics HUD and per-frame debug sidecar.
- `debugAction`, `debugContext`, `debugTarget`, `debugCoordinates`, `debugScroll`: Individual diagnostics toggles inherited by child actions.

### Activity Overlay Blocks

```text
showKeystrokes {
  keyboardShortcut "Control+K"
  press "Enter"
}

showMouseButtons {
  mouseDown x=80 y=120
  mouseUp x=80 y=120
}

showNetworkActivity {
  waitForResponse "/api/save"
}

showConsoleActivity {
  waitForConsole "sync complete"
}
```

Convenience aliases for `recording showKeystrokes=true`, `recording showMouseButtons=true`, `recording networkCaptions=true`, and `recording consoleCaptions=true`. Each accepts no arguments, requires a child block, and accepts the same scoped recording options as `recording`. The blocks do not start recording. Without command-level `--gif` or a nested recording block, child actions still execute but CMG injects no overlay or virtual pointer. Text-entry actions display `Text input`, never the entered value; event overlays summarize outcomes without console payloads or request URLs.

### `gif`, `recordVideo`, And `screencast`

```text
gif "open-dialog" quality=highest {
  click "#open"
}

recordVideo "checkout" quality=high timeline=true {
  click "#pay"
}

screencast "compact" quality=medium {
  click "#open"
}
```

Encoder options:

- `quality=<archival|highest|high|medium|low>` selects a preset. `archival` prioritizes frame-local color fidelity over file size.
- `dither=<none|floyd-steinberg|bayer|atkinson|sierra>` overrides the preset dithering algorithm.
- `palette=<global|local|adaptive>` controls the GIF color table. `adaptive` currently uses frame-local tables.
- `colors=<2..256>` overrides the maximum color count.
- `keepFrames=<true|false|directory>` retains each exact pre-encoding PNG as `frame-NNNN.png`; `true` uses a sibling `<gif-name>.frames` directory.
- `background=<color|transparent|none>` flattens captured alpha onto a color, or clears an inherited background.
- `gradientMode=<smooth|text>` supplies gradient- or text-oriented encoder defaults; explicit encoder controls override it.
- `highContrastPalette=<true|false>` intentionally increases contrast and saturation for accessibility evidence.
- Caption scopes support `captionStacking`, `persistentStepTitle`, `sourceLineCaptions`, `debugNarration`, `captionFormat=plain|markdown`, and localized generated-label options. All are recording-only defaults.
- `crop=<selector-or-rich-locator>` clips every frame to the live target bounds.
- `cropPadding=<0..2000>` adds CSS-pixel context around `crop=` and requires it.
- `safeArea=<0..500>` keeps pointer targets and tight crops clear of clipping; default `24`, disabled with `0`.
- `layoutStability=<0..5000>` waits for settled target coordinates after scrolling; default `150`, disabled with `0`.
- `targetZoom=<auto|always|none>` and `targetZoomThreshold=<8..100>` control the tiny-target inset; defaults are `auto` and `24`.
- `pagePosition=<auto|always|none>` controls the long-page viewport rail; default `auto` activates above 1.5 viewports.
- `tabContext=<auto|always|none>` controls a capture-only active-title/tab-count badge; default `auto` activates with multiple tabs.
- `smartCrop=<true|false|widthxheight>` follows the pointer and active target using stable frame dimensions; `true` means `640x480` and cannot be combined with `crop=`.
- `splitTabs=<auto|always|none>` composes labeled screenshots of every open tab. `always` reserves two tiles for stable popup evidence; `auto` expands only after another tab exists. It cannot be combined with `crop=` or `smartCrop=`. Redactions apply independently to every tab, while pointer events and overlays remain limited to the active tab.
- `scale=<0.05..1>` downscales the cropped or viewport frame before GIF quantization.
- `maxWidth=<1..10000>` and `maxHeight=<1..10000>` add output dimension caps while preserving aspect ratio.
- `viewport=<width>x<height>` temporarily sets recording viewport dimensions and restores the prior viewport afterward.
- `pixelRatio=<1..4>` controls high-DPI recording capture.
- `sizeBudget=<bytes|KB|MB|GB>` targets a maximum encoded artifact size, such as `750KB` or `2MB`.
- `budgetQualityFallback=<true|false>` permits deterministic quality reduction when the requested encoding exceeds `sizeBudget`; default `true`.
- `budgetDownscaleFallback=<true|false>` permits bounded dimension reduction after quality fallback; default `true`.

These options may be inherited from `recording` / `withRecording` when a nested GIF block creates the artifact. Invalid names or ranges fail with the exact option and accepted values. Retained PNGs contain the page and CMG recording UI after requested crop, scale, background, and contrast transforms, so treat them with the same privacy controls as the GIF.

Budget fallback never changes browser actions, virtual-pointer events, drag ghosts, captions, or source-frame timing. CMG tries the requested encoding first, then lower quality and smaller dimensions only when enabled. It retains the smallest valid candidate when the requested target is impossible and reports the decision through `GIF_CAPTURE_STATS` and timeline `captureDiagnostics`.

Crop bounds are re-resolved before every frame, so a moving or resizing panel remains framed. CMG clips the browser capture after placing the virtual pointer and overlays, then scales the resulting bitmap; pointer coordinates therefore stay aligned. Title cards reuse the most recent crop bounds while their temporary card hides page content.

Records only the wrapped actions when direct `browser control script` or `cmg run` is used without command-level `--gif`. If command-level `--gif` is used, the entire script or test is recorded and nested `gif` blocks do not create separate GIFs.
With command-level `--no-gif` or enabled `CMG_DISABLE_GIF`, every recording-block alias executes its children with no recorder and emits `GIF_SKIPPED <line> status=skipped reason=recording-disabled source=<cli|environment>`. No screenshot, artifact, recording overlay, or virtual pointer is created.
`recordVideo` and `screencast` are provider-style aliases for the same CMG GIF recorder. Output is still an animated GIF so the virtual pointer, pointer events, drag ghost behavior, and captions remain consistent. At runtime either alias emits `GIF_ALIAS_WARN <line> action=<alias> format=gif suggestion=use-gif`; the warning is advisory and does not alter the artifact or exit code. Use `gif` when GIF output is intended.

Options:

- `output`: Optional GIF path for direct browser-control scripts. Without `output`, CMG writes `<name>.gif` in the current directory.
- `accessibilityEvidence`, `showKeystrokes`, `focusEvidence`, `accessibleNames`, `highContrast`, `contrastWarnings`: Optional accessibility evidence controls. Each accepts `true` or `false`; the umbrella preset enables all five individual behaviors.
- `showMouseButtons`: Optional low-level mouse down/up labels. Accepts `true` or `false` and remains independent from the accessibility umbrella preset.
- `reducedMotion`, `highContrastPointer`: Optional accessible choreography presets. Child pointer, fade, and pulse options override preset properties locally.
- `debug`, `debugAction`, `debugContext`, `debugTarget`, `debugCoordinates`, `debugScroll`: Optional frame diagnostics. Invalid booleans fail with the exact option name; `debug=false` disables inherited diagnostics for that child.
- `quality`: Optional GIF quality: `highest`, `high`, `medium`, or `low`. Defaults to `highest`. This affects palette generation and dithering only; virtual pointer movement, pointer events, drag ghosts, captions, timing, and captured frames stay the same.
- `pointerPath`: Optional default pointer route: `auto`, `direct`, `arc`, `manhattan`, `avoid-target`, or `avoid-center`.
- `dragPath`: Optional default route while the pointer is held during drag movement.
- `pointerTheme`: Optional default pointer theme: `arrow`, `hand`, `dot`, `ring`, `branded`, or `touch`.
- `pointerColor`: Optional default pointer CSS color.
- `pointerSize`: Optional default pointer size in CSS pixels from `8` to `96`, or `auto`.
- `pointerShadow`: Optional pointer shadow: `none`, `light`, `medium`, or `strong`.
- `showPointer`: Optional pointer visibility default for child frames: `true`, `false`, or `auto`.
- `captionStyle`: Optional caption style default for child `caption`, `showMessageBar`, and `step` captions.
- `captionSize`: Optional caption text size: `normal`, `large`, or `x-large`.
- `captionPosition`: Optional caption position default for child captions.
- `captionSeverity`: Optional caption severity default for child captions.
- `autoCaptions`: Optional `true`/`false` automatic narration for supported visual child actions.
- `eventCaptions`, `networkCaptions`, `dialogCaptions`, `consoleCaptions`, `downloadCaptions`, `uploadCaptions`, `serviceWorkerCaptions`, `webSocketCaptions`, `workerCaptions`: Optional recording-only event narration. The umbrella enables every category; child category options win.
- `intro`, `outro`, `introDuration`, `outroDuration`, `resultOutro`: Optional title-card defaults. Durations must be greater than zero.
- `coalesceDuplicates`, `sampleEvery`: Optional capture-efficiency defaults inherited by child actions. Child actions can override either value.
- `captionTemplate`: Optional automatic-caption template inherited by child actions.
- `intro` / `outro`: Optional opening and final title-card text.
- `introDuration` / `outroDuration`: Optional title-card durations in milliseconds. Defaults to `1200`.
- `pressedPointer`: Optional held-pointer visual compression default for recorded drags.
- `dragTrail`: Optional held-pointer trail line default for recorded drags.
- `dragBreadcrumbs`: Optional held-pointer breadcrumb dots default for recorded drags.
- `holdAfterAction`: Optional default post-action hold in milliseconds for child actions. Defaults to `350`; child actions can override it locally with their own `holdAfterAction=`.
- `preClickHold`: Optional default hold before click/tap dispatch after pointer movement. Defaults to `0`.
- `postClickHold`: Optional default hold after click/tap pulse frames. Defaults to `350`.
- `holdAfterMove`: Optional default hold after recorded `moveMouse` actions. Child `moveMouse` actions can override it locally.
- `holdAfterNavigation`: Optional default hold after navigation actions and waits. Defaults to `350`.
- `holdAfterAssertion`: Optional default hold after assertion actions. Defaults to `350`.
- `holdOnFailure`: Optional final failure-state hold in milliseconds. Defaults to `1200`; use `0` to suppress the extra failure hold.
- `fps`: Optional frame rate from `1` to `100`. Defaults to `10`.
- `frameDelay`: Optional frame delay in milliseconds from `10` to `10000`. Overrides `fps`.
- `timeline`: Optional timeline JSON sidecar. Use `true` to write next to the GIF, `false` / `off` / `none` to disable, a directory to write `<gif-name>.timeline.json` inside it, or a `.json` path to write that exact file.
- `redact`: Semicolon-separated selectors or rich locators to mask throughout the block.
- `redactStyle`: `solid`, `blur`, or `replacement` for block-level `redact` rules.
- `redactColor`: CSS mask color. Defaults to `#111827`.
- `redactReplacement`: Replacement-mask text. Defaults to `[redacted]`.
- `redactPadding`: Extra mask padding from `0` to `100` CSS pixels.
- `autoRedact`: `passwords` (default), `tokens`, `emails`, `payment`, `privacy`, or `none`. `privacy` combines every preset; `sensitive` aliases `tokens`.
- `redactionSafety`: `standard` (default) or `strict`. Strict mode aborts capture if a visible password field remains uncovered.

Output:

- `GIF <path>` when a script-level block writes its own recording.
- `GIF_TIMELINE <path>` when the recording also writes timeline metadata.
- `GIF_BLOCK_SUPPRESSED <line>` when command-level `--gif` is active and the block is included in the full recording instead.

If the block fails while recording, CMG captures one extra final-state frame before writing the partial GIF. Non-GIF runs do not create a virtual pointer, timeline file, or failure frames.

Automatic captions are recording-only. Default `fill` and `type` narration never includes the entered value. Set `captionPosition=auto` to place automatic captions opposite the target, and use action-level `autoCaptions=false` or `captionTemplate=` when one step needs different narration.

### `maskGif`, `redactGif`, And `redactText`

```text
maskGif "#account" style=replacement replacement="Account hidden" padding=4
redactGif getByLabel="Email" style=blur
redactText "#api-token" style=solid color="#000000"
```

Adds a persistent GIF-only redaction. All three names are aliases. The action requires one selector or rich locator and accepts `style=solid|blur|replacement`, `color=`, `replacement=`, and `padding=0..100`. A later action using the same locator replaces the earlier rule. CMG resolves the locator again for every captured frame, so the mask follows a moved or remounted element.

The overlay exists only while CMG takes a recording screenshot and is removed immediately afterward. It does not modify the element, its value, application state, pointer events, or normal browser interaction. The virtual pointer, click pulse, and CMG captions remain above masks. Use `solid` or `replacement` for security-sensitive evidence; `blur` is visual obscuration, not irreversible data removal.

Output while recording:

```text
GIF_REDACT 003 target="#account" status=active
```

Without an active recording the action skips before resolving variables or locators:

```text
GIF_REDACT 003 status=skipped reason=no-active-recording
```

### `unmaskGif` And `unredactGif`

```text
unmaskGif "#account"
unredactGif
```

Removes a persistent redaction by its exact locator. With no argument, it clears all persistent rules added by redaction actions. It does not remove inherited `redact=` rules while their recording scope remains active. Both names are aliases.

Output is `GIF_UNREDACT <line> target="<locator>" status=active`, or `target=all` when every persistent rule is cleared. Without recording it reports `status=skipped reason=no-active-recording` and does not touch the page.

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

Clears the field and types the value. Text inputs and textareas use the browser's native `value` setter followed by bubbling, composed `InputEvent` and `change` events, so React controlled inputs and other value-tracking frameworks receive the update. In GIF mode this keeps the same visible pointer and progressive typing behavior as `clear` plus `type`. Rich locators are live: if a controlled render replaces the element after a character, CMG resolves the locator again before the next mutation. `fill` accepts the same typing options as `type`, including `delay=`. This action works in direct browser-control scripts and `cmg run`.

### `check` And `uncheck`

```text
check "#enabled"
uncheck "#enabled"
```

Sets a checkbox-like element and dispatches `input` and `change` events. These actions work in direct browser-control scripts and `cmg run`.

### `focus`, `blur`, And `selectText`

```text
focus "#name"
selectText "#name"
blur "#name"
```

Runs the corresponding DOM focus/selection behavior. These actions work in direct browser-control scripts and `cmg run`.

### `dblclick`, `doubleClick`, `rightClick`, And `contextClick`

```text
dblclick "#item"
doubleClick "#item"
rightClick "#item"
contextClick "#item"
dblclick "#canvas" modifiers=Control+Shift x=8 y=12
```

Moves to the element with the visual hover path, then dispatches the page-facing mouse event. `doubleClick` is an alias for `dblclick`; `contextClick` is an alias for `rightClick`. With `x=`, `y=`, or `modifiers=`, CMG dispatches the event at the configured element-relative point with the requested modifier flags. In GIF recordings, the virtual pointer moves to the same element-relative point before the event is dispatched.

GIF recordings capture two pulse frames for `dblclick` / `doubleClick`, and `rightClick` / `contextClick` default to the crosshair pulse unless `clickPulse=` overrides the style.

Options:

- `modifiers`: Optional comma- or plus-separated modifiers: `Alt`, `Control`, `Meta`, and `Shift`.
- `x`: Optional X offset inside the element.
- `y`: Optional Y offset inside the element.
- `clickPulse`: Optional GIF pulse style override: `ring`, `ripple`, `dot`, `crosshair`, or `none`.

Output:

- `MOUSE_EVENT <line> dblclick <selector>` for `dblclick` and `doubleClick`.
- `MOUSE_EVENT <line> contextmenu <selector>` for `rightClick` and `contextClick`.

### `dispatchEvent`

```text
dispatchEvent "#item" "ready"
dispatchEvent "#item" "cmg:ready" detail="{\"ok\":true}" bubbles=true cancelable=true
```

Dispatches a page-side `Event` on the selected element. When `detail=` is provided, CMG dispatches a `CustomEvent` and parses `detail` as JSON. This action is available in direct browser-control scripts and `cmg run`.

Options:

- `detail`: Optional JSON payload for a `CustomEvent`.
- `bubbles`: Optional boolean. Default is `true`.
- `cancelable`: Optional boolean. Default is `true`.

Output:

- `EVENT <line> <event-name> <selector>` when the event is dispatched.

`dispatchEvent` does not move the virtual pointer. Use pointer actions such as `click`, `dblclick`, `doubleClick`, `rightClick`, or `contextClick` when the event should be visible in a GIF.

### `expectUrl`, `expectTitle`, And `waitForTitle`

```text
expectUrl "/checkout"
expectTitle "Checkout" match=exact
waitForTitle "checkout" ignoreCase=true timeout=5000
```

Fails unless the current URL or title matches the expected text. `waitForTitle` polls until the title matches or the timeout expires.

These are shared actions, so they work in both direct browser-control scripts and `cmg run`.

Options:

- `match`: `contains`, `exact`, or `regex`. Default is `contains`.
- `ignoreCase`: Use `true` for case-insensitive matching.

Output:

- `URL <line> <url>` for `expectUrl`.
- `TITLE <line> <title>` for `expectTitle` and `waitForTitle`.

### Element Assertion Aliases

```text
expectVisible "#save" timeout=5000
expectNotVisible "#spinner" timeout=5000
waitForVisible "#save" timeout=5000
expectHidden "#spinner" timeout=5000
expectNotHidden "#save" timeout=5000
waitForHidden "#spinner" timeout=5000
expectEnabled "#save"
expectNotEnabled "#archive"
expectDisabled "#archive"
expectNotDisabled "#save"
expectAttached "#save"
expectNotAttached "#toast"
expectDetached "#toast"
expectNotDetached "#save"
expectEditable "#email"
expectNotEditable "#readonly"
expectEmpty "#empty"
expectNotEmpty "#status"
expectFocused "#email"
expectNotFocused "#save"
expectInViewport "#save"
expectNotInViewport "#offscreen"
expectValue "#email" "agent@example.com" timeout=5000
expectValues "#plans" "basic" "pro" timeout=5000
expectAttribute "#save" "aria-label" "Save"
expectClass "#save" "ready"
expectId "#save" "save"
expectCSS "#save" "display" "block"
expectProperty "#save" "dataset.ready" "true"
expectAccessibleName "#save" "Save"
expectRole "#save" "button"
expectChecked "#terms" true
expectNotChecked "#marketing"
expectUnchecked "#marketing"
expectCount ".result" 3 timeout=5000
toHaveText "#status" "Saved"
toBeVisible "#save"
toBeNotVisible "#spinner"
toBeHidden "#spinner"
toBeNotHidden "#save"
toBeEnabled "#save"
toBeNotEnabled "#archive"
toBeDisabled "#archive"
toBeNotDisabled "#save"
toBeAttached "#save"
toBeNotAttached "#toast"
toBeDetached "#toast"
toBeNotDetached "#save"
toBeEditable "#email"
toBeNotEditable "#readonly"
toBeEmpty "#empty"
toBeNotEmpty "#status"
toBeFocused "#email"
toBeNotFocused "#save"
toBeInViewport "#save"
toBeNotInViewport "#offscreen"
toHaveValue "#email" "agent@example.com"
toHaveValues "#plans" "basic" "pro"
toHaveAttribute "#save" "aria-label" "Save"
toHaveClass "#save" "ready"
toHaveId "#save" "save"
toHaveCSS "#save" "display" "block"
toHaveJSProperty "#save" "dataset.ready" "true"
toHaveAccessibleName "#save" "Save"
toHaveRole "#save" "button"
toBeChecked "#terms" true
toBeNotChecked "#marketing"
toBeUnchecked "#marketing"
toHaveCount ".result" 3
```

Runs browser-side assertions for common UI state checks. The `toHave*` and `toBe*` forms are Playwright-style aliases over the matching CMG assertions. Negative state aliases map to inverse state checks, including `expectNotVisible`/`toBeNotVisible`, `expectNotAttached`/`toBeNotAttached`, `expectNotEditable`/`toBeNotEditable`, `expectNotEmpty`/`toBeNotEmpty`, `expectNotFocused`/`toBeNotFocused`, and `expectNotInViewport`/`toBeNotInViewport`. `expectNotChecked`, `unchecked`, `expectUnchecked`, and `toBeUnchecked` assert a false checked state. `waitForVisible` and `waitForHidden` are provider-style wait aliases for the visible and hidden assertions. Element assertions resolve CMG locators before checking the matched element. Direct browser-control scripts also accept locator-form options such as `expectVisible text=Save` when the parser would otherwise treat `text=Save` as an option. `expectHidden`, `toBeHidden`, `waitForHidden`, `expectNotVisible`, and `toBeNotVisible` pass when no connected matching element exists. `expectDetached`, `toBeDetached`, `expectNotAttached`, and `toBeNotAttached` pass when no connected matching element exists. `expectValues` and `toHaveValues` compare selected option values in order. `expectAccessibleName` and `toHaveAccessibleName` check aria-label, alt, title, or text-derived accessible names. `expectRole` and `toHaveRole` check explicit roles and common implicit roles. `expectClass` accepts a class token or class-name fragment. `expectCSS` checks computed style values. `expectProperty` and `toHaveJSProperty` accept dotted DOM property paths such as `dataset.ready`. `expectCount` and `toHaveCount` count matching CSS elements and support zero-count assertions.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `0`, which checks once.

Output:

- `EXPECT <line> <visible|hidden|enabled|disabled|attached|detached|editable|noteditable|empty|notempty|focused|notfocused|inviewport|notinviewport|value|values|attribute|class|id|css|property|accessiblename|role|checked|unchecked|count> <selector>` for direct element assertion actions and aliases.

### `waitForUrl`

```text
waitForUrl "/dashboard" timeout=10000
```

Polls the current URL until it matches the expected text, then reports the matched URL. If the timeout expires, the failure reason includes the expected text, match mode, timeout, and last URL seen.

Options:

- `timeout`: Optional timeout in milliseconds. Default is `5000`.
- `match`: `contains`, `exact`, or `regex`. Default is `contains`.
- `ignoreCase`: Use `true` for case-insensitive matching.

### `localStorage`, `sessionStorage`, And `cookie`

```text
localStorage set "token" "abc"
localStorage get "token"
localStorage clear
sessionStorage remove "token"
cookie set "mode" "demo"
cookie set "scoped" "yes" path="/app" sameSite="Lax" secure="true"
cookie get "mode"
cookie clear
```

Reads or mutates page storage from the current page context. These are shared actions, so they work in both direct browser-control scripts and `cmg run`. `get` writes the actual value as its payload; missing web storage or cookie values produce an empty payload, which `set` blocks store as an empty string.

Arguments:

- `localStorage` and `sessionStorage`: `get <key>`, `set <key> <value>`, `remove <key>`, or `clear`.
- `cookie`: `get [key]`, `set <key> <value>`, `remove <key>`, or `clear`.

Cookie options:

- `domain`: Cookie domain for `set`, `remove`, or `clear`.
- `path`: Cookie path for `set`, `remove`, or `clear`. Defaults to `/`.
- `expires`: Cookie expiry date string for `set`.
- `maxAge`: Cookie `Max-Age` in seconds for `set`.
- `sameSite`: Cookie `SameSite` value for `set`; accepts `Strict`, `Lax`, or `None`.
- `secure`: `true` or `false`; when `true`, adds the `Secure` attribute for `set`.

`HttpOnly` is not available from the `cookie` action because it runs in the page context and browsers do not allow JavaScript to set `HttpOnly` cookies.

Output:

- `LOCAL_STORAGE <line> <operation> ...` for local storage.
- `SESSION_STORAGE <line> <operation> ...` for session storage.
- `COOKIE <line> <operation> ...` for cookies.

### `apiRequest`

```text
apiRequest "GET" "https://example.com/api/status" status=200 contains="ok"
apiRequest "POST" "https://example.com/api/items" body="{\"name\":\"demo\"}" header.Authorization="Bearer token"
apiRequest "POST" "https://example.com/api/items" json="{\"name\":\"demo\"}" query.preview=true timeout=10000
apiRequest "POST" "https://example.com/login" form.user="agent" form.mode="test" auth="user:pass" ok=true
apiRequest "GET" "https://example.com/api/items" status="200-299" expectHeader.Content-Type="json" output="demo-output\items.json"
```

Runs an HTTP request from direct browser-control scripts or `cmg run`. This does not move the virtual pointer because it is not a browser UI action, but it is included in stdout, reports, traces, and step failure diagnostics.

Options:

- `status`: Expected response status. Accepts one status, a comma-separated list, or ranges such as `200-299`.
- `ok`: Optional boolean. When `true`, requires a 2xx response. When `false`, requires a non-2xx response.
- `contains`: Text expected in the response body.
- `notContains`: Text that must not appear in the response body.
- `body`: Request body text.
- `json`: Request body text with `Content-Type: application/json` unless `contentType=` is also set.
- `form.<name>`: Form value. When any form field is present, CMG sends `application/x-www-form-urlencoded`.
- `contentType`: Request content type for `body` or `json`.
- `query.<name>`: Query string value to append to the request URL.
- `header.<name>`: Request header.
- `expectHeader.<name>`: Expected response header substring.
- `auth`: Basic auth credentials as `username:password`.
- `output`: Optional response body output path. When set, stdout writes `API_BODY_FILE` instead of `API_BODY`.
- `timeout`: Request timeout in milliseconds. Default is `30000`.

Output:

- `API <line> <status> <url>` after receiving a response.
- `API_BODY <line> <body>` with the response body.
- `API_BODY_FILE <line> <path>` when `output=` writes the response body to disk.

### `storageState`

```text
storageState save path="artifacts\auth.json"
storageState load path="artifacts\auth.json"
```

Saves or loads page storage state for the current browser page. The state includes `localStorage`, `sessionStorage`, and the current `document.cookie` string. This is a runner action and reports `STORAGE_STATE` output lines.

### `route`, `intercept`, `mockResponse`, `waitForRequest`, `waitForRequestFinished`, `waitForRequestFailed`, `waitForResponse`, `exportHar`, `replayHar`, And `clearRoutes`

```text
route "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json"
route "/api/profile" bodyFile="fixtures/profile.json" contentType="application/json"
intercept "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json"
intercept "/api/profile" method=POST times=1 delay=250 status=201 body="{\"saved\":true}"
mockResponse "/api/profile" header="X-Trace: demo" headers="Cache-Control: no-store; X-Mode: mock"
intercept "/api/profile/\\d+" match=regex ignoreCase=true status=200 body="profile"
intercept "/api/down" abort=true error="offline"
waitForResponse "/api/profile" timeout=5000
waitForResponse "/api/profile" method=POST status=201 contains=created mocked=true timeout=5000
waitForResponse "/api/profile" header="Content-Type: json"
waitForResponse "/api/profile/\\d+" match=regex ignoreCase=true
waitForRequest "/api/profile" headerName=Authorization headerValue=Bearer
waitForRequest "/api/profile" timeout=5000
waitForRequestFinished "/api/profile" timeout=5000
waitForRequestFailed "/api/profile" timeout=5000
exportHar path="artifacts\network.har"
replayHar path="artifacts\network.har"
clearRoutes
```

Installs a page-level route for `fetch()` and `XMLHttpRequest`. `intercept` is an alias for `route` for Cypress-style scripts. Matching calls receive the configured mocked response and are recorded in the page response log. Routes and network waits match URLs with `match=contains|exact|regex`; default is `contains`. Use `ignoreCase=true` for case-insensitive URL matching. Use `method=` to restrict a route to one HTTP method, `times=` to remove it after a fixed number of matches, and `delay=` to simulate response latency. Routes can set mocked response bodies inline with `body=` or from disk with `bodyFile=`/`file=`, and response headers with `header=`, `headers=`, `headerName=`, and `headerValue=`. Use `abort=true` or `action=abort` to reject matching requests instead; aborted matches are recorded in the request failure log. Requests are recorded before dispatch. Failed page-side `fetch()` and `XMLHttpRequest` calls are recorded in a separate failure log. `waitForRequest` waits for dispatch, `waitForRequestFinished` and `waitForResponse` wait for completed responses, and `waitForRequestFailed` waits for failures. Waits can also filter by `method=`, `status=`, response or error text with `contains=`, mocked-state with `mocked=true|false`, and headers with `header=`, `headerName=`, and `headerValue=`; timeout failures include the requested filters.

Options:

- `status`: Optional mocked response status. Default is `200`.
- `body`: Optional mocked response body. Default is empty text.
- `bodyFile`: Optional mocked response body file path.
- `file`: Alias for `bodyFile`.
- `contentType`: Optional mocked response content type. Default is `text/plain`.
- `method`: Optional HTTP method filter, for example `GET` or `POST`.
- `times`: Optional positive integer. The route is removed after that many matches.
- `delay`: Optional non-negative integer in milliseconds. The mocked fulfill or abort waits for this duration.
- `abort`: Optional boolean-like route abort switch. Use `true` to fail matching requests.
- `action`: Optional route action. Use `abort` to fail matching requests.
- `error`: Optional failure message for aborted routes. Default is `Request aborted by CMG route`.
- `header`: Optional route response header formatted as `Name: value`; for waits, a header filter formatted as `Name` or `Name: value`.
- `headers`: Optional route response headers separated by semicolons, for example `A: 1; B: 2`.
- `timeout`: Optional for network waits. Default is `5000`.
- `match`: Optional URL match mode for routes and network waits. Supports `contains`, `exact`, and `regex`. Default is `contains`.
- `ignoreCase`: Optional boolean for URL matching on routes and network waits.
- `contains`: Optional text filter for response bodies or failure messages on network waits.
- `mocked`: Optional boolean filter for network waits. Use `true` for mocked route traffic or `false` for real page traffic.
- `headerName`: Optional route response header name; for waits, a header name filter. Header names are matched case-insensitively.
- `headerValue`: Optional route response header value; for waits, a header value substring filter. On waits, requires `header` or `headerName`.

`exportHar` writes captured response metadata and bodies to a HAR-like JSON file. `replayHar` reads that file and installs routes for each captured request URL.

Notes:

- Routes and network waits support substring, exact, and regex URL matching.
- Route aborts are observable with `waitForRequestFailed`, not `waitForResponse`.
- Header filters on `waitForRequest` and `waitForRequestFailed` match request headers. Header filters on `waitForResponse` and `waitForRequestFinished` match response headers.
- Network actions do not move the virtual pointer. They are captured in reports and can be wrapped with `step` or captions for GIF narration.

Output:

- `ROUTE <line> <pattern>` when a route or intercept is installed.
- `ROUTES_CLEARED <line>` when routes are cleared.
- `REQUEST <line> <json>` when `waitForRequest` finds a matching request.
- `REQUEST_FINISHED <line> <json>` when `waitForRequestFinished` finds a completed matching request.
- `REQUEST_FAILED <line> <json>` when `waitForRequestFailed` finds a matching failed request.
- `RESPONSE <line> <json>` when `waitForResponse` finds a matching response.
- `HAR_EXPORTED <line> <path>` when a HAR file is written.
- `HAR_REPLAY <line> routes=<count> <path>` when HAR routes are installed.

### `routeWebSocket`, `clearWebSocketRoutes`, `waitForWebSocket`, And `waitForWebSocketMessage`

```text
routeWebSocket "/socket" message="ready"
routeWebSocket "/socket" close=true code=1000 reason="done"
routeWebSocket "/socket/\\d+" match=regex ignoreCase=true message="ready"
waitForWebSocket "/socket" timeout=5000
waitForWebSocketMessage "^ready:" match=regex ignoreCase=true timeout=5000
clearWebSocketRoutes
```

Wraps the page `WebSocket` constructor in the current page and future navigations. Matching sockets are recorded, sent messages are captured on the socket record, and matching routes can dispatch a synthetic message on open or close the socket. This is CMG's page-side equivalent of provider WebSocket routing, so it works in direct scripts and `cmg run` while preserving report, trace, and GIF behavior.

Arguments:

- `pattern`: Required matcher. Routes and socket waits match the WebSocket URL. Message waits match message data or the WebSocket URL.

Options:

- `message`: Optional synthetic message dispatched to matching sockets after `open`.
- `close`: Optional boolean. Use `true` to close matching sockets after `open`.
- `code`: Optional close code. Default is `1000`.
- `reason`: Optional close reason. Default is `Closed by CMG routeWebSocket`.
- `timeout`: Optional for waits. Default is `5000`.
- `match`: Optional match mode: `contains`, `exact`, or `regex`. Default is `contains`.
- `ignoreCase`: Optional boolean for URL and message matching. Default is `false`.

Output:

- `WEBSOCKET_ROUTE <line> <pattern>` when a route is installed.
- `WEBSOCKET <line> <json>` when a matching socket is found.
- `WEBSOCKET_MESSAGE <line> <json>` when a matching message is found.
- `WEBSOCKET_ROUTES_CLEARED <line>` when routes are cleared.

These actions do not move the virtual pointer. Wrap them in `step`, `caption`, or `gif` blocks when a GIF should narrate WebSocket setup or waits.

### `setExtraHTTPHeaders`, `clearExtraHTTPHeaders`, `setHeaders`, `clearHeaders`, `setHttpCredentials`, `clearHttpCredentials`, And `setOffline`

```text
setExtraHTTPHeaders "X-CMG-Agent" "true" "Accept" "application/json"
clearExtraHTTPHeaders
setHeaders "X-CMG-Agent" "true"
clearHeaders
setHttpCredentials "agent" "secret"
clearHttpCredentials
setProxy "https://proxy.local/?url="
clearProxy
setOffline true
setOffline false
```

Patches page-side `fetch()` and `XMLHttpRequest` behavior in the current page and future navigations. `setHeaders` and `clearHeaders` are concise aliases for `setExtraHTTPHeaders` and `clearExtraHTTPHeaders`. Extra headers are added to page-originated fetch/XHR requests. HTTP credentials add a Basic `Authorization` header to page-originated fetch/XHR requests. Offline mode reports `navigator.onLine=false`, dispatches `offline`/`online`, and makes patched fetch/XHR requests fail while enabled.
`setProxy` rewrites page fetch/XHR request URLs to `<prefix><encoded-url>`.

Arguments:

- `name value`: Required for `setExtraHTTPHeaders`; repeat pairs to add more request headers.
- `username password`: Required for `setHttpCredentials`, `httpCredentials`, or `authenticate`.
- `proxy prefix`: Required for `setProxy` or `proxy`.

Output:

- `HEADERS_SET <line> <count>` when headers are installed.
- `HEADERS_CLEARED <line>` when extra headers are cleared.
- `HTTP_CREDENTIALS_SET <line> <username>` when Basic auth credentials are installed.
- `HTTP_CREDENTIALS_CLEARED <line>` when Basic auth credentials are removed.
- `PROXY_SET <line> <prefix>` when page request rewriting is installed.
- `PROXY_CLEARED <line>` when page request rewriting is cleared.
- `OFFLINE <line> <true|false>` when offline mode changes.

These actions do not move the virtual pointer. They affect page-side requests and are included in reports and traces. Browser-level navigation requests, browser-native HTTP auth prompts, and browser launch proxy state are not rewritten; use them before page actions that call `fetch()` or `XMLHttpRequest`.

### `listWorkers`, `waitForWorker`, `workerEvaluate`, And `workerIntercept`

```text
listWorkers
evaluate "window.worker = new Worker('/worker.js', { name: 'worker.js' }); true"
waitForWorker "worker.js" timeout=5000
waitForWorker "worker-\\d+\\.js" match=regex ignoreCase=true timeout=5000
workerEvaluate "self.ready === true" target="worker.js"
workerIntercept "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json" target="worker.js"
workerIntercept "/api/profile/\\d+" match=regex ignoreCase=true bodyFile="fixtures/profile.json" header="X-Trace: demo" target="worker.js"
```

Inspects and controls worker targets exposed by the browser automation provider. `listWorkers`, `workerEvaluate`, and `workerIntercept` initialize CMG's page-side worker bridge. Same-origin classic workers created after that initialization keep CMG-owned id, URL, and worker name/title metadata so they can be targeted reliably in headless Chrome.

`workerIntercept` patches a matched worker's `fetch()` function so worker-originated requests can receive deterministic responses. Worker intercepts support the same URL match modes, file-backed bodies, and response header options as page `route`. The optional `target` option can be a worker id, URL substring, or worker name/title substring; when omitted, CMG uses the first available worker.

Workers that already existed before bridge initialization are still listed from browser target metadata when Chrome exposes them, but URL/title metadata and direct evaluation depend on what the browser target reports. Worker interception does not rewrite browser-level navigation requests, service worker traffic, module workers, or cross-origin workers.

Options:

- `timeout`: Optional for `waitForWorker`. Default is `5000`.
- `match`: Optional URL match mode for `waitForWorker` and `workerIntercept`. Supports `contains`, `exact`, and `regex`. Default is `contains`.
- `ignoreCase`: Optional boolean for worker URL matching. `waitForWorker` defaults to case-insensitive matching.
- `target`: Optional worker id, URL substring, or worker name/title substring for `workerEvaluate` and `workerIntercept`.
- `status`: Optional response status for `workerIntercept`. Default is `200`.
- `body`: Optional response body for `workerIntercept`. Default is empty text.
- `bodyFile`: Optional response body file for `workerIntercept`.
- `file`: Alias for `bodyFile`.
- `contentType`: Optional response content type for `workerIntercept`. Default is `text/plain`.
- `header`: Optional worker response header formatted as `Name: value`.
- `headers`: Optional semicolon-separated worker response headers.
- `headerName`: Optional worker response header name.
- `headerValue`: Optional worker response header value for `headerName`.

Output:

- `WORKER <index> id=<id> type=<type> title="<title>" url="<url>"` for `listWorkers`.
- `WORKER_READY <line> id=<id> url="<url>"` for `waitForWorker`.
- `WORKER_EVALUATE <line> <result>` for worker evaluation.
- `WORKER_INTERCEPT <line> routes=<count> <pattern>` when a worker intercept is installed.

Worker actions do not move the virtual pointer. They are included in reports and traces. Active recordings with `eventCaptions=true` or `workerCaptions=true` add privacy-safe outcome evidence without exposing worker expressions, results, URLs, or intercepted bodies.

### `startCoverage` And `stopCoverage`

```text
startCoverage js=true css=true
navigate "https://example.com"
stopCoverage path="artifacts\coverage.json"
```

Collects JavaScript and CSS coverage from the current page target. Chrome and Edge use precise browser protocol coverage. Firefox uses WebDriver BiDi plus page inspection to return a best-effort coverage envelope for discovered scripts and stylesheets. `startCoverage` enables collection; `stopCoverage` stops collection and returns or writes a JSON object with `js` and `css` arrays.

Options:

- `js`: Optional boolean for `startCoverage`. Default is `true`.
- `css`: Optional boolean for `startCoverage`. Default is `true`.
- `path`: Optional output path for `stopCoverage`. Without it, stdout receives the JSON payload.

Output:

- `COVERAGE_STARTED <line> js=<true|false> css=<true|false>` when collection starts.
- `COVERAGE <line> <path-or-json>` when coverage is stopped.

Coverage actions do not move the virtual pointer. They are included in reports and traces, and can be wrapped with `step` or captions for GIF narration.

### `startTracing` And `stopTracing`

```text
startTracing path="artifacts\script.trace.json"
navigate "https://example.com"
stopTracing
```

Collects a CMG script trace with step line numbers, action names, stdout lines, and failure reasons. Direct `browser control script` can also trace the whole run with `--trace <path>`. Inside a script, `startTracing` begins a partial trace and `stopTracing` writes it. If `startTracing` has a `path` or `output` option and the script fails before `stopTracing`, CMG writes the partial trace with `success=false` and the failure message. During command-level `browser control script --trace`, nested trace actions are suppressed so the command-level trace remains the single source of truth.

Aliases:

- `tracingStart`: Alias for `startTracing`.
- `tracingStop`: Alias for `stopTracing`.

Options:

- `path`: Optional trace JSON path. May be supplied to `startTracing` or `stopTracing`.
- `output`: Alias for `path`.

Output:

- `TRACE_STARTED <line> <path>` when tracing starts.
- `TRACE <line> <path>` when `stopTracing` writes the trace.
- `TRACE <path>` when command-level tracing or active failure tracing writes at script end.
- `TRACE_BLOCK_SUPPRESSED <line>` when command-level tracing suppresses nested trace actions.

Tracing does not move the virtual pointer. It records the same output and failure reasons that appear in stdout, reports, and GIF-aware runs.

### `expectScreenshot`

```text
expectScreenshot "#dialog" baseline="baselines\dialog.png" output="demo-output\dialog.actual.png" tolerance=0.01
expectScreenshot baseline="baselines\page.png" output="demo-output\page.actual.png" fullPage=true mask="#clock;#ad"
toHaveScreenshot "#dialog" baseline="baselines\dialog.png" output="demo-output\dialog.actual.png"
```

Captures an element screenshot when a selector is provided, otherwise captures the page viewport. `toHaveScreenshot` is a Playwright-style alias. CMG compares the actual PNG against the baseline and fails if the normalized pixel difference is greater than `tolerance`.

Options:

- `baseline`: Baseline PNG path. If missing, CMG writes it from the actual image and fails with an explanatory reason.
- `output`: Actual PNG path. Default is `actual.png`.
- `tolerance`: Allowed normalized difference from `0` to `1`. Default is `0`.
- `fullPage`: Optional boolean. For page assertions, captures the full scrollable page instead of only the viewport. Ignored when a selector is provided.
- `mask`: Optional semicolon-separated selectors or rich locators to cover before comparison, for example `"#clock;hasText=.ad|Sponsored"`.
- `maskColor`: Optional mask color as hex. Default is `#ff00ff`.

When a selector is provided in GIF mode, CMG moves the virtual pointer to that element before the comparison frame. Page-level visual comparisons do not move the pointer. Wrap this action in a `step` when the GIF should include a caption explaining the comparison.

### `uploadFiles`, `setInputFiles`, And `selectFile`

```text
uploadFiles "#avatar" "fixtures\avatar.png"
setInputFiles "#avatar" "fixtures\avatar.png"
selectFile "#avatar" "fixtures\avatar.png"
uploadFiles "testid=file-input" "fixtures\one.txt" "fixtures\two.txt"
```

Assigns one or more local files to an `<input type="file">` element and dispatches `input` and `change` events in the page. `setInputFiles` and `selectFile` are provider-style aliases. The first argument is a CMG locator. Remaining arguments are local file paths resolved from the current working directory unless absolute.

Output:

- `UPLOAD <line> <count>` on success.

Failure reasons include a missing selector argument, no file paths, a local file that does not exist, or browser evaluation failure. This action is available at the top level and inside `gif` blocks. In GIF mode, CMG moves the virtual pointer to the file input and captures the page-side upload transition; browser file chooser windows are not opened.

## Imports, Control Flow, Loops, And Macros

These language actions are available in both direct `browser control script` files and `cmg run`.

### `import`

```text
import "shared.cmgscript"
```

Expands another script file before parsing. Relative paths resolve from the directory of the importing script. Imported files can contain macros, setup actions, tests for `cmg run`, and more imports. Missing files and import cycles fail before any action runs.

Imports use the same formatting normalizer as the rest of the DSL. Semicolon-separated lines such as `import "shared.cmgscript"; call helper` are valid outside quoted strings.

### `if`, `elseif`, And `else`

```text
if (${count} > 5 && !(${mode} == "")) {
  click "#save"
} elseif (${mode} == "preview") {
  hover "#save"
} else {
  caption "No action"
}
```

Conditions support static values, `${variables}`, strings, numbers, empty strings, `==`, `!=`, `>`, `>=`, `<`, `<=`, `contains`, `matches`, `in`, `&&`, `||`, and unary `!`. The same comparison operators are available in `elseif`, `while`, and `switch` case matchers.

Conditions can also run actions. Actions that emit a payload, such as `evaluate`, `title`, `url`, `content`, `textContent`, `innerText`, `inputValue`, `getAttribute`, selector evaluation, `readFile`, `fixture`, `call`, and `return`, can be compared with the same operators:

```text
if (evaluate "window.checkoutReady" == "true") {
  click "#continue"
}
```

Actions that do not emit a payload are boolean probes: they are true when they succeed and false when they fail. Pointer-aware actions used in conditions still use CMG's virtual pointer and pointer event path when GIF recording is active.

Only the first matching branch runs. Branch bodies can contain any action, including nested control flow, loops, macros, `gif` blocks, pointer actions, and non-visual actions. Macros declared inside a branch are scoped to that branch body.

### `switch`, `case`, And `default`

```text
switch title {
  case "profile" {
    caption "Profile flow"
  }
  case contains "Checkout" {
    caption "Payment flow"
  }
  case matches "admin-.+" {
    caption "Admin flow"
  }
  default {
    caption "Fallback flow"
  }
}
```

`switch` evaluates one value and runs the first matching `case`. The switch subject can be static text, a variable, or a value-producing action such as `title` or `evaluate`. A bare `case "value"` is an equality match. A case can also start with `==`, `!=`, `>`, `>=`, `<`, `<=`, `contains`, `matches`, or `in`. `matches` uses a regular expression and `in` accepts one or more values.

`default` is optional and runs when no case matches. A switch block can contain only `case` and `default` blocks, and can have only one default. Case/default bodies are scoped like `if` branches: variables set inside macros remain local to their macro calls, and helper macros declared inside a branch do not leak outward.

### `macro`, `call`, And `return`

```text
macro fillProfile name email {
  fill "#name" "${name}"
  fill "#email" "${email}"
}

call fillProfile "Agent" "agent@example.com"
```

`macro` registers a reusable block. `call` executes it. Parameters are untyped values, so callers can pass variables, selectors, temporary selectors from `foreachSelector`, URLs, file paths, or any other string value. Macro calls can be nested, and macros can declare helper macros inside their body.

Macro variable lookup starts with parameters and local `set` values, then walks upward through the parent tree scopes where that macro was defined until it finds a matching variable. It does not read unrelated local variables from a caller outside that definition tree. Variables set inside the macro body are scoped to the call. A `set` inside a macro does not overwrite a variable with the same name in a parent scope. Macros declared inside another macro, branch, or loop are restored when that block finishes, so helper macros do not leak into later steps. Top-level macros in `cmg run` are registered before each planned test.

`set` can capture macro output:

```text
macro readTitle {
  evaluate "document.title"
}

set title {
  call readTitle
}
```

The variable receives only the final payload value from the macro body, such as the document title string. Use `return` when a macro should explicitly return a variable or static value:

```text
macro labelFor item {
  return "label-${item}"
}

set label {
  call labelFor "save"
}
```

`return` requires at least one argument and emits `RETURN <line> <value>` when run directly.

### `for`, `repeat`, `while`, `until`, `doWhile`, `doUntil`, `foreach`, `foreachJson`, `foreachList`, `foreachSelector`, `break`, And `continue`

```text
for 3 {
  caption "index ${index}"
}

repeat i 3 {
  caption "repeat ${i}"
}

while (${ready} != true) max=10 {
  waitForTimeout 250
  set ready {
    evaluate "window.ready === true"
  }
}

until (${ready} == true) max=10 {
  waitForTimeout 250
}

doWhile (${shouldPoll} == true) max=10 {
  caption "poll once before checking"
}

doUntil (${ready} == true) max=10 {
  set ready {
    evaluate "window.ready === true"
  }
}

for i 1 4 step=1 {
  click "#item-${i}"
}

foreach name Alice Bob {
  fill "#name" "${name}"
}

set names {
  allTextContents ".person"
}

foreachJson name "${names}" {
  caption "Person ${index}: ${name}"
}

foreachList mode "smoke, visual, report" delimiter="," {
  caption "Mode ${index}: ${mode}"
}

foreachSelector row ".result" {
  if (${index} == 10) {
    break
  }
  click "${row}"
}
```

`for <count>` iterates from `0` up to but not including `<count>` and exposes `${index}`. `for <variable> <start> <end>` uses an exclusive end value and exposes the named variable. `step=<integer>` changes the increment and cannot be `0`.

`repeat <count>` repeats a fixed number of times and exposes `${index}`. `repeat <variable> <count>` exposes the named variable instead. `while <condition>` repeats while the same condition syntax used by `if` remains true. `until <condition>` repeats while that condition remains false. `doWhile <condition>` and `doUntil <condition>` run the body once before evaluating the condition, then repeat while the condition is true or false respectively. Condition loops have a safety guard: `max=<count>` defaults to `100` and the action fails if that many iterations are exceeded.

`foreach` iterates over explicit values. `foreachJson` parses a JSON array, which is useful with `set names { allTextContents ".person" }`; strings become their text value, numbers and booleans become text, objects and arrays stay compact JSON, and `null` becomes an empty string. `foreachList` splits a delimited string; `delimiter=` defaults to `,`, `trim=false` preserves whitespace, and `empty=true` keeps empty items. `foreachSelector` finds all CSS matches, binds the variable to a temporary selector for each element, and exposes `${index}`. `break` exits the nearest loop. `continue` skips the rest of the current iteration. Using either action outside a loop fails clearly.

Pointer-aware child actions still use CMG's normal virtual pointer movement, browser events, hover behavior, drag ghosts, and captions when GIF recording is active.

### `within`

```text
within ".dialog" {
  fill "input[name=email]" "agent@example.com"
  contains "Saved"
  click "button.save"
}

within ".app-shell" {
  within ".toolbar" {
    click ".refresh"
  }
}
```

`within "<containerSelector>" { ... }` scopes selector-based child actions to a container, similar to provider scoped locators. Plain CSS selectors and explicit `css=` selectors are composed under the current container. Nested `within` blocks compose their containers.

Inside a `within` block, one-argument body-text assertions such as `contains "Saved"` and `toContainText "Saved"` check the scoped container instead of the whole page body. `foreachSelector` also scopes its match set, so `foreachSelector row ".item"` iterates `.item` elements inside the container. Selector-based getters, including `computedStyle` and `property`, read from the scoped element.

Rich locators such as `text=Save`, `role=button`, `hasText=.row|Ready`, and `xpath=...` still use CMG's existing locator resolver. For a one-line scoped target that works in scripts and one-off CLI commands, use `inside=.container|target`; for larger flows, nest plain CSS actions under `within`.

`within` is a script-only structural block. It is available in direct `browser control script` files and in `cmg run`; it is not exposed as a one-shot CLI command because the scoped child actions are the units that perform browser work.

The structural `within` block does not move the virtual pointer by itself. Pointer-aware child actions are rewritten before recording, so GIF pointer movement, pointer events, drag ghosts, screenshots, and captions target the scoped selector.

### `retry` And `toPass`

```text
retry max=3 delay=100 {
  click "#save"
  assertText "#status" "Saved"
}

retry 2 {
  waitForText "#status" "Ready"
}

toPass max=3 delay=100 {
  expectText "#status" "Saved"
}
```

`retry` reruns its child block until the block succeeds or the attempt limit is exhausted. `toPass` is the provider-style assertion-block alias with the same behavior. Use either a positional attempt count, such as `retry 2` or `toPass 2`, or `max=<count>`. If neither is provided, CMG uses `max=3`. `max` must be greater than `0`. `delay=<milliseconds>` is optional and waits between failed attempts.

Failed attempts write parseable diagnostic output before the next attempt:

```text
RETRY 012 attempt=1 failed=Line 13: assertText failed. Expected text 'Saved' was not found. Actual text: 'Saving'.
RETRY 012 success attempt=2
TO_PASS 020 attempt=1 failed=Line 21: expectText failed. Expected text 'Saved' was not found. Actual text: 'Saving'.
TO_PASS 020 success attempt=2
```

If all attempts fail, the block fails with the last child-action reason:

```text
Line 12: retry failed. retry exhausted 3 attempt(s). Last error: Line 13: assertText failed. Expected text 'Saved' was not found. Actual text: 'Error'.
Line 20: toPass failed. toPass exhausted 3 attempt(s). Last error: Line 21: expectText failed. Expected text 'Saved' was not found. Actual text: 'Error'.
```

The structural `retry` and `toPass` actions do not move the virtual pointer by themselves. Pointer-aware actions inside them still use normal GIF recording behavior, including virtual pointer movement, browser pointer events, captions, and drag ghosts on every attempt.

### `try`, `catch`, And `finally`

```text
try {
  click "#maybe-there"
} catch error {
  caption "${error}"
} finally {
  screenshotPage output="demo-output\after-try.png"
}
```

`try` runs its body with normal fail-fast behavior. If an action fails and a following `catch` block exists, CMG binds the failure message to the optional catch variable and runs the catch body. If no catch exists, the original failure is reported after any `finally` block runs.

`finally` always runs after the `try` body and after a matching `catch`. A failure inside `catch` or `finally` fails the script. `catch` and `finally` must immediately follow a `try` block; standalone `catch` or `finally` fails clearly.

These blocks can contain any action, including pointer-aware actions, GIF blocks, macros, loops, and assertions. The structural blocks do not move the virtual pointer by themselves, but their child actions record normally.

## Unknown Actions

Unknown actions fail explicitly instead of being ignored. If an action is not listed in this document, CMG reports it as unsupported so agent callers can distinguish a DSL command problem from page behavior.

## Locator Support

Direct browser-control scripts and `cmg run` both support:

- Plain CSS selectors, for example `"#open"`.
- Explicit CSS selectors, for example `"css=#open"`.
- Test id selectors, for example `"testid=save"`, mapped to `[data-testid='save']`.
- Text locators, for example `"text=Save"`.
- Exact text locators, for example `"textExact=Save"`.
- Regex text locators, for example `"textRegex=^Save"`.
- Role locators, for example `"role=button"`.
- Role-plus-name locators, for example `"role=button|Save"`.
- Role-plus-regex-name locators, for example `"roleRegex=button|^Save"`.
- Label locators, for example `"label=Email"`.
- Exact label locators, for example `"labelExact=Email"`.
- Regex label locators, for example `"labelRegex=^Email"`.
- Placeholder locators, for example `"placeholder=Search"`.
- Exact placeholder locators, for example `"placeholderExact=Search"`.
- Regex placeholder locators, for example `"placeholderRegex=Search$"`.
- Alt text locators, for example `"alt=Logo"`.
- Exact alt text locators, for example `"altExact=Logo"`.
- Regex alt text locators, for example `"altRegex=Logo$"`.
- Title locators, for example `"title=Close"`.
- Exact title locators, for example `"titleExact=Close"`.
- Regex title locators, for example `"titleRegex=Close$"`.
- XPath locators, for example `"xpath=//button[.='Save']"`.
- Provider-style locator aliases:
  - `"getByText=Save"`, `"getByTextExact=Save"`, `"getByExactText=Save"`, and `"getByTextRegex=^Save"` map to text locators.
  - `"getByRole=button|Save"` and `"getByRoleRegex=button|^Save"` map to role/name locators.
  - `"getByLabel=Email"`, `"getByLabelText=Email"`, and their `Exact` / `Regex` variants map to label locators.
  - `"getByTestId=save"`, `"getByTestID=save"`, and `"getByTestid=save"` map to `[data-testid='save']`.
  - `"getByPlaceholder=Search"`, `"getByPlaceholderText=Search"`, and their `Exact` / `Regex` variants map to placeholder locators.
  - `"getByAltText=Logo"` plus `Exact` / `Regex` variants map to alt text locators.
  - `"getByTitle=Close"` plus `Exact` / `Regex` variants map to title locators.
- Filter locators:
  - `"first=.item"` resolves the first matching CSS element.
  - `"last=.item"` resolves the last matching CSS element.
  - `"nth=.item|2"` resolves the zero-based matching CSS element at index `2`.
  - `"has=.item|.badge"` resolves the first matching CSS element that contains a descendant matching `.badge`.
  - `"hasNot=.item|.badge"` resolves the first matching CSS element that does not contain a descendant matching `.badge`.
  - `"hasText=.item|Save"` resolves the first matching CSS element whose visible/text content includes `Save`.
  - `"hasNotText=.item|Draft"` resolves the first matching CSS element whose visible/text content does not include `Draft`.
  - `"visible=.item"` resolves the first matching CSS element that has a non-empty box and is not hidden by `display:none` or `visibility:hidden`.
- Composition locators:
  - `"or=.primary|.secondary"` resolves the first `.primary` element, or the first `.secondary` element if no `.primary` exists.
  - `"and=.item|.selected"` resolves the first `.item` element that also matches `.selected`.
  - `"strict=.item"` resolves `.item` only when exactly one element matches; zero or multiple matches fail before pointer movement.
  - `"inside=.card|button.save"` resolves the first `button.save` inside the first `.card`. This is the one-line locator form of a simple `within` scope and is useful for one-off CLI commands.
  - `"closest=.badge|.card"` resolves the nearest `.card` ancestor of the first `.badge`, matching common traversal flows.
  - `"parent=.badge"` resolves the immediate parent of `.badge`; `"parent=.badge|.row"` requires that parent to match `.row`.
  - `"next=.current|.target"` and `"previous=.current|.target"` resolve the next or previous matching sibling. Without the second selector they return the immediate sibling.
- Open shadow DOM locators:
  - `"shadow=#host|button.save"` resolves `button.save` inside `#host`'s open shadow root.
  - `"shadowText=#host|Shadow Save"` resolves the first descendant inside `#host`'s open shadow root whose text contains `Shadow Save`.

For non-CSS locator forms, CMG resolves the element inside the page, marks it with a temporary `data-cmg-locator-id`, and then runs the normal pointer-aware command against that marker. This keeps GIF pointer movement, browser events, drag ghosts, and screenshots connected to the resolved element.

Direct browser-control scripts can pass locator forms as normal arguments or as option-style tokens when the locator contains `=`, for example:

```text
click text=Save
click getByText=Save
click "getByRole=button|Save"
type getByLabel=Email "agent@example.com"
click textExact=Save
click textRegex=^Save
click "role=button|Save"
click "roleRegex=button|^Save"
type label=Email "agent@example.com"
type labelExact=Email "agent@example.com"
click "text=Save changes"
click "nth=.result|2"
click "has=.card|button.primary"
click "or=.primary|.secondary"
click "and=.item|.selected"
click "strict=.only-choice"
click "inside=.card|button.save"
click "closest=.badge|.card"
click "parent=.badge|.card"
click "next=.current|.target"
click "previous=.current|.target"
click "shadow=#host|button.save"
assertText "hasText=.toast|Saved" "Saved"
mouseMove selector="text=Drop here" edge=center
```

Quote the whole locator token when the locator value contains spaces or filter separators, unless your shell/script parser is passing it as one option-style token. Provider-style `getBy*` aliases, filter locators, and open shadow-root locators are resolved to the same temporary element marker as text/role/xpath locators, so pointer-aware actions and GIF recordings target the resolved element.

## Actionability

Selector-based runner actions wait before running the pointer-aware command. CMG checks that the resolved element:

- exists;
- has a non-empty bounding box;
- is not `display:none` or `visibility:hidden`;
- is not disabled with `disabled` or `aria-disabled="true"`;
- has a stable rectangle across actionability polls.

Use `timeout=<milliseconds>` on the action to control the wait. If the element does not become actionable, the step fails with the reason in stderr and reports.
