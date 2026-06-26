# `.cmgscript` Examples

Runnable demo scripts live in [`../../demo-scripts/`](../../demo-scripts/).

Record a demo as a GIF:

```powershell
dotnet run -- browser control script --file demo-scripts\01-dialog-flow.cmgscript --gif demo-output\dialog-flow.gif
```

## Provider Navigation Aliases

```text
visit "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog"
goto "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog"
```

This example is available as `demo-scripts/25-provider-navigation-aliases.cmgscript`.

## Dialog Flow

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
click "#openProfileDialog"
waitForElement "#profileDialog[open]" timeout=5000
clear "#profileName"
type "#profileName" "CMG Test Profile"
screenshot "#profileDialog" output="profile-dialog.png"
click "#profileDialog button[value='close']"
```

## Capture HTML And Screenshot Data URLs

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog"
html "#openProfileDialog"
screenshot "#openProfileDialog"
```

## Validate Text

```text
navigate "C:\Projects\CMG\index.html"
assertText "h1" "CMG Browser Control Test Page"
contains "Browser Control"
containsText "h1" "CMG Browser Control"
waitForText "h1" "CMG Browser Control Test Page" timeout=5000
notContains "Unhandled error"
notContainsText "h1" "Legacy Title"
```

This example is available as `demo-scripts/26-provider-text-assertions.cmgscript`.

## Page Message Bar

```text
navigate "C:\Projects\CMG\index.html"
showMessageBar "Opening the profile dialog"
screenshotPage output="message-bar.png"
```

## Variables

```text
set button "#openProfileDialog"
navigate "C:\Projects\CMG\index.html"
waitForElement "${button}"
click "${button}"

set heading {
  html "h1"
}
showMessageBar "${heading}"
```

## Selector Evaluation

```text
navigate "C:\Projects\CMG\index.html"
set pageTitle {
  evalOnSelector "h1" "element.textContent"
}
showMessageBar "${pageTitle}"
evaluateAll ".command" "elements => elements.length"
```

This example is available as `demo-scripts/27-selector-evaluation.cmgscript`.

## Element Getters

```text
navigate "C:\Projects\CMG\index.html"
textContent "h1"
set titleText {
  innerText "h1"
}
showMessageBar "${titleText}"
getAttribute "#openProfileDialog" "id"
```

This example is available as `demo-scripts/28-element-getters.cmgscript`.

## Control Flow, Imports, And Macros

```text
import "30-shared-macros.cmgscript"

navigate "C:\Projects\CMG\index.html"
set heading {
  textContent "h1"
}

if (${heading} != "") {
  call announce "${heading}"
} else {
  caption "Heading was empty"
}

foreachSelector action ".command" {
  call chooseCommand "${action}"
}
```

Macros can receive plain values, variables, selectors, or temporary selectors from `foreachSelector`. Control blocks and macros can be nested in any combination. Macro parameters and variables set inside a macro are scoped to the current call, loop variables are scoped to the current iteration, and helper macros declared inside a macro or branch do not leak outward.

This direct-script example is available as `demo-scripts/30-control-flow-macros.cmgscript` and imports `demo-scripts/30-shared-macros.cmgscript`. The structured `cmg run` form is available as `demo-scripts/31-control-flow-runner.cmgscript`.

## Switch Control

```text
set count 7

switch evaluate "'checkout'" {
  case "profile" {
    caption "Profile flow"
  }
  case in "checkout" "billing" {
    caption "Checkout flow"
  }
  default {
    caption "Fallback flow"
  }
}

if (evaluate "'checkout'" in "checkout" "billing" && ${count} > 5) {
  caption "Priority payment flow"
}
```

`switch` supports equality cases plus `==`, `!=`, `>`, `>=`, `<`, `<=`, `contains`, `matches`, and `in`. The word operators `contains`, `matches`, and `in` also work in `if`, `elseif`, and `while` conditions. Value-producing actions such as `evaluate`, page getters, element getters, file reads, and macro calls can be used inline in those conditions.

This direct-script example is available as `demo-scripts/39-switch-control.cmgscript`. The structured `cmg run` form is available as `demo-scripts/40-switch-control-runner.cmgscript`.

## Macro Scoping

```text
set token "global"

macro readGlobal {
  return "${token}"
}

macro parent {
  set token "parent"

  macro child {
    set beforeShadow {
      return "${token}"
    }
    set token "child"
    return "${beforeShadow}-${token}"
  }

  set childResult {
    call child
  }
  set topResult {
    call readGlobal
  }
  return "${childResult}-${token}-${topResult}"
}

set result {
  call parent
}
```

`child` reads `token` from its definition parent before it shadows the value locally. The parent still sees `parent` afterward. `readGlobal` was defined at the top level, so it reads the top-level `global` value rather than the unrelated local `token` inside `parent`.

This direct-script example is available as `demo-scripts/34-macro-scoping.cmgscript`. The structured `cmg run` form is available as `demo-scripts/35-macro-scoping-runner.cmgscript`.

## Loop Control

```text
repeat i 4 {
  if (${i} == 1) {
    continue
  }
  if (${i} == 3) {
    break
  }
  caption "Repeat ${i}"
}

while (${ready} == false) max=3 {
  set ready true
  caption "Ready"
}

until (${ready} == true) max=3 {
  caption "Waiting"
}

doWhile (${ready} == false) max=2 {
  caption "Runs before checking"
}

doUntil (${ready} == true) max=2 {
  set ready true
  caption "Runs until ready"
}
```

`repeat`, `for`, `foreach`, `foreachSelector`, `while`, `until`, `doWhile`, and `doUntil` can contain nested macros, conditionals, and GIF blocks. Condition loops are bounded with `max=` to prevent scripts from hanging indefinitely. `doWhile` and `doUntil` always run their body once before checking the condition.

This example is available as `demo-scripts/32-loop-control.cmgscript`.

## Recoverable Failures

```text
try {
  click "${missingSelector}"
} catch error {
  caption "${error}"
} finally {
  screenshotPage output="demo-output\try-catch-finally.png"
}
```

Use `try` when a script intentionally probes for optional page state. The `catch` block receives the failure reason as a variable when a name is provided, and `finally` runs for cleanup or diagnostics even when the original failure is not caught.

This example is available as `demo-scripts/33-try-catch-finally.cmgscript`.

## Drag And Drop

```text
navigate "C:\Projects\CMG\index.html"
setViewport width=900 height=700
waitForElement "#dropQueue"
scrollIntoView "#dragdrop"
dragAndDrop "[data-command='browser launch']" "#dropQueue"
assertText "#dropQueue" "browser launch"
```

Mobile viewport setup:

```text
viewport 390 844 deviceScaleFactor=2 isMobile=true hasTouch=true
navigate "https://example.com/mobile"
```

## Complex Drag And Drop

```text
navigate "C:\Projects\CMG\index.html"
setViewport width=900 height=700
waitForElement "#dropQueue"
scrollIntoView "#dragdrop"

dragAndDrop "[data-command='browser launch']" {
  delay 200
  hover "#dropQueue"
  delay 200
  drop "#dropQueue"
}

assertText "#dropQueue" "browser launch"
```

Run the complete example:

```powershell
dotnet run -- browser control script --file demo-scripts\07-complex-drag-flow.cmgscript --gif demo-output\complex-drag.gif
```

## Drag Pointer Movement

Use `moveMouse` during GIF recording to move a dragged item to a viewport-relative point. The pointer movement and delay dispatch browser mouse, pointer, drag, and dragover events.

```text
navigate "C:\Projects\CMG\index.html"
setViewport width=900 height=700
waitForElement "#dropQueue"
scrollIntoView "#dragdrop"

dragAndDrop "[data-command='browser launch']" {
  moveMouse "center"
  delay 200
  hover "#dropQueue"
  delay 200
  drop "#dropQueue"
}
```

Run the `moveMouse` demo with GIF recording:

```powershell
dotnet run -- browser control script --file demo-scripts\08-gif-move-mouse.cmgscript --gif demo-output\gif-move-mouse.gif
```

For pages that auto-scroll while a dragged item is held near the lower viewport edge, use `moveMouse "bottom"` with `delay` inside the drag block:

```text
navigate "C:\Projects\CMG\index.html"
setViewport width=900 height=700
waitForElement "#dropQueue"
scrollIntoView "#dragdrop"

dragAndDrop "[data-command='browser launch']" {
  moveMouse "bottom"
  delay 800
  moveMouse "center"
  delay 200
  moveMouse "bottom"
  delay 800
  drop "#dropQueue"
}
```

Run the bottom-edge drag demo with GIF recording:

```powershell
dotnet run -- browser control script --file demo-scripts\09-drag-autoscroll.cmgscript --gif demo-output\drag-autoscroll.gif
```

For apps that scroll a canvas or content area instead of the browser window, target the scroll container edge:

```text
dragAndDrop ".library-item" {
  moveMouse selector=".content-area" edge=bottom inset=24
  delay 1500
  moveMouse selector=".content-area" edge=bottom inset=24
  delay 1500
  drop ".content-drop-target"
}
```

## CSS Hover States

The virtual pointer in GIF mode triggers real browser hover state. This demo moves across elements with CSS `:hover` styling, asserts that the hover card received a browser hover event, and captures the visual state.

```text
navigate "C:\Projects\CMG\index.html"
setViewport width=900 height=700
waitForElement "#hoverDemoCard"
scrollIntoView "#hoverStates"
hover "#hoverDemoCard"
assertText "#hoverStateText" "Hover card active"
evaluate "getComputedStyle(document.querySelector('#hoverDemoCard')).transform !== 'none'"
screenshot "#hoverStates" output="demo-output\css-hover-card.png"
hover "#hoverDemoButton"
hover "#hoverDemoInput"
```

Run it with GIF recording:

```powershell
dotnet run -- browser control script --file demo-scripts\10-css-hover-states.cmgscript --gif demo-output\css-hover-states.gif
```

## Fixtures And Files

```text
readFile payload path="index.html"
writeFile path="demo-output\fixture-copy.txt" text="${payload}"
appendFile path="demo-output\fixture-copy.txt" text="\n<!-- checked by CMG -->"
expectFile path="demo-output\fixture-copy.txt" contains="checked by CMG"
```

The same file actions can run inside direct browser-control scripts and inside `cmg run` tests. File I/O does not move the virtual pointer, but the output and any failure reason are included in reports and traces.

Run the complete direct script:

```powershell
dotnet run -- browser control script --file demo-scripts\11-fixtures-and-files.cmgscript
```

## Print PDF

```text
navigate "C:\Projects\CMG\index.html"
printPdf path="demo-output\page.pdf" printBackground=true
```

PDF generation works in direct browser-control scripts and `cmg run`. It is non-visual, so use a `step` or `caption` when a GIF should explain that the page is being printed.

Run the complete direct script:

```powershell
dotnet run -- browser control script --file demo-scripts\12-print-pdf.cmgscript
```

## Stdin

```powershell
@"
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog"
"@ | cmg browser control script --file -
```

## Runner DSL

Run structured tests with:

```powershell
dotnet run -- browser launch
dotnet run -- run demo-scripts\20-v2-runner-flow.cmgscript --report-json demo-output\runner.json
```

Record every test as a GIF:

```powershell
dotnet run -- run demo-scripts\20-v2-runner-flow.cmgscript --gif demo-output\runner-gifs
```

Use once-per-scope hooks for page setup and teardown:

```text
beforeAll {
  setContent "<main><button id='run'>Run</button><output id='status'>Ready</output></main>"
}

suite "hooked flow" {
  beforeAll {
    caption "Suite setup"
  }

  test "uses setup page" {
    click "#run"
    expectText "#status" "Ready"
  }

  afterAll {
    caption "Suite teardown"
  }
}
```

This example is available as `demo-scripts/38-before-after-all.cmgscript`.

Provider-style structure aliases are also supported:

```text
before {
  setContent "<main><button id='run'>Run</button><output id='status'>Ready</output></main>"
}

describe "provider structure aliases" {
  it "runs with mocha names" {
    click "#run"
    toContainText "#status" "Ready"
  }
}
```

This example is available as `demo-scripts/47-provider-structure-runner.cmgscript`.

Weird but valid formatting is accepted when an AI emits dense scripts:

```text
          describe          "inline formatting"          {          before          {          setContent          "<main>{ready}</main>"          }          it          "handles inline blocks"          {          if          true          {          caption          "inline if"          }          else          {          caption          "inline else"          }          }          }
```

Direct-script inline, semicolon-separated, and spacing-tolerant formatting is available as `demo-scripts/48-weird-formatting.cmgscript`. The structured `cmg run` form is available as `demo-scripts/49-weird-formatting-runner.cmgscript`.

Retry a flaky page transition without retrying the whole test:

```text
test "save eventually completes" {
  retry max=3 delay=100 {
    click "#save"
    assertText "#status" "Saved"
  }
}
```

Failed attempts write `RETRY <line> attempt=<n> failed=<reason>` to stdout, and the successful attempt continues the script. Direct-script retry formatting is available as `demo-scripts/50-retry-block.cmgscript`. The structured `cmg run` form is available as `demo-scripts/51-retry-block-runner.cmgscript`.

Use explicit `fail` for intentional guard clauses. It is catchable inside `try`:

```text
try {
  fail "Expected optional panel"
} catch error {
  caption "${error}"
}
```

Direct-script explicit failure handling is available as `demo-scripts/52-explicit-fail.cmgscript`. The structured `cmg run` form is available as `demo-scripts/53-explicit-fail-runner.cmgscript`.

Record only one block from inside the script:

```text
test "partial recording" {
  navigate "C:\Projects\CMG\index.html"
  waitForElement "#openProfileDialog"
  gif "open-dialog" {
    step "Opening dialog" {
      click "#openProfileDialog"
    }
  }
}
```

Direct browser-control scripts can also record a block:

```text
gif "open-dialog" output="demo-output\open-dialog.gif" {
  click "#openProfileDialog"
  waitForElement "#profileDialog[open]"
}

recordVideo "open-dialog-video" output="demo-output\open-dialog-video.gif" {
  click "#openProfileDialog"
  waitForElement "#profileDialog[open]"
}
```

When command-level `--gif` is used, the whole script or test is recorded and `gif` blocks are included in that recording instead of producing nested GIF files.

## Runner Reports

```powershell
dotnet run -- run demo-scripts\20-v2-runner-flow.cmgscript `
  --report-json demo-output\runner.json `
  --report-html demo-output\runner.html `
  --report-junit demo-output\runner.xml
```

Reports include per-test status, output, GIF paths, and step-level failure reasons.

Write trace files for deeper debugging:

```powershell
dotnet run -- run demo-scripts\20-v2-runner-flow.cmgscript --trace demo-output\traces
```

Each trace is a JSON file with test metadata, steps, output, errors, and GIF references.

## Exposed Page Functions

```text
test "uses exposed helper" {
  exposeFunction cmgAdd "(a, b) => a + b"
  evaluate "window.__sum = window.cmgAdd(2, 3)"
  waitForFunction "window.__sum === 5"
}
```

Exposed functions are page-side helpers available in direct browser-control scripts and `cmg run`. They do not call back into the CMG host process, so they work the same way across supported browser clients.

## Filtering, Sharding, And Retries

```powershell
dotnet run -- run demo-scripts --grep runner
dotnet run -- run demo-scripts --tag smoke
dotnet run -- run demo-scripts --retries 2
dotnet run -- run demo-scripts --max-failures 1
dotnet run -- run demo-scripts\51-retry-block-runner.cmgscript --repeat-each 3
dotnet run -- run demo-scripts\51-retry-block-runner.cmgscript --list
dotnet run -- run demo-scripts --shard 1/2
```

When `--max-failures` reaches its threshold, stdout includes `RUN STOP maxFailures=<count>`. Reports, traces, and GIF output include only tests that actually ran before the stop.

`--repeat-each` schedules each selected test multiple times with names such as `checkout [repeat 2/3]`. Each repeated test gets its own report entry, trace, retry attempts, and command-level GIF when `--gif` is active.

`--list` prints `TEST LIST run <name>` or `TEST LIST skip <name>` for the selected schedule without connecting to a browser or running actions. It respects grep, tag, focus, repeat, and shard options.

Use `tag=` on tests:

```text
test "checkout" tag=smoke,critical {
  click "#checkout"
}
```

Focus or skip tests from the DSL:

```text
test "debug checkout" only=true {
  click "#checkout"
}

test "legacy checkout" skip=true reason="Legacy page is disabled" {
  click "#legacyCheckout"
}
```

If any selected test has `only=true`, CMG runs only focused tests. `skip=true` writes `TEST SKIP <name>` and reports `status=skipped` without running the test actions.

## API Request

```text
test "api health" tag=api {
  apiRequest "GET" "https://example.com/health" status=200 contains="ok"
}
```

The request result appears in CLI output and reports. Failures include the expected and actual status or body mismatch reason.

## Network Mocking

```text
test "mocked profile" {
  navigate "https://example.com"
  intercept "/api/profile" delay=250 status=200 body="{\"name\":\"CMG\"}" contentType="application/json"
  intercept "/api/profile" method=POST status=201 body="created"
  evaluate "fetch('/api/profile').then(r => r.text())"
  evaluate "fetch('/api/profile', { method: 'POST', headers: { Authorization: 'Bearer demo' } }).then(r => r.text())"
  waitForRequestFinished "/api/profile"
  waitForRequest "/api/profile" method=POST header="Authorization: Bearer"
  waitForResponse "/api/profile" method=POST status=201 contains=created mocked=true
  waitForResponse "/api/profile" header="Content-Type: text/plain"
}
```

This patches page `fetch` and `XMLHttpRequest` calls and records matching responses for `waitForResponse`. Use `delay=` to simulate a slow mocked response without moving the virtual pointer. Network waits can filter by method, status, response body text, request or response headers, and whether the match came from a CMG mock.

## HAR Replay

```text
test "records profile traffic" {
  navigate "https://example.com"
  evaluate "fetch('/api/profile').then(r => r.text())"
  waitForResponse "/api/profile"
  exportHar path="demo-output\profile.har"
}

test "replays profile traffic" {
  replayHar path="demo-output\profile.har"
  navigate "https://example.com"
  evaluate "fetch('/api/profile').then(r => r.text())"
  waitForResponse "/api/profile"
}
```

CMG's HAR support is page-level and uses the same `fetch`/`XMLHttpRequest` hook as `route`. It is available in both direct browser-control scripts and `cmg run`.

## Network Environment

```text
test "offline banner" {
  navigate "https://example.com"
  setExtraHTTPHeaders "X-CMG-Agent" "true"
  setHttpCredentials "agent" "secret"
  setProxy "https://proxy.local/?url="
  setOffline true
  evaluate "fetch('/api/profile').catch(error => error.message)"
  setOffline false
  clearProxy
  clearHttpCredentials
}
```

Network environment actions are shared by direct scripts and `cmg run`. They do not move the virtual pointer, but reports and traces include `HEADERS_SET`, `HEADERS_CLEARED`, `HTTP_CREDENTIALS_SET`, `HTTP_CREDENTIALS_CLEARED`, `PROXY_SET`, `PROXY_CLEARED`, and `OFFLINE` lines.

## Browser Environment

```text
test "browser environment knobs" {
  setJavaScriptEnabled false
  bypassCSP true
  serviceWorkers block
  serviceWorkers allow
}
```

These actions are CMG page-side equivalents of provider context settings. They do not relaunch the browser or change browser-level process flags.

## Worker Interception

```text
test "worker profile request" {
  navigate "https://example.com"
  waitForWorker "worker.js" timeout=5000
  workerIntercept "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json" target="worker.js"
  workerEvaluate "fetch('/api/profile').then(r => r.text())" target="worker.js"
}
```

Worker actions are non-visual and do not move the virtual pointer. Use a `step` caption when a GIF should explain that a worker route was installed.

## Coverage Collection

```text
test "checkout coverage" {
  startCoverage js=true css=true
  navigate "https://example.com/checkout"
  click "#submit"
  stopCoverage path="demo-output\coverage.json"
}
```

Coverage actions work in direct browser-control scripts and `cmg run`. They do not move the virtual pointer, but reports and traces include the `COVERAGE` output line.

## Visual Screenshot Assertion

```text
test "dialog matches baseline" {
  navigate "C:\Projects\CMG\index.html"
  click "#openProfileDialog"
  expectScreenshot "#profileDialog" baseline="baselines\profile-dialog.png" output="demo-output\profile-dialog.actual.png" tolerance=0.01
}
```

If the baseline does not exist, CMG writes it from the actual screenshot and fails so the baseline can be reviewed.

## UI State Assertions

```text
test "profile form state" {
  navigate "https://example.com/profile"
  expectVisible "#profileForm" timeout=5000
  waitForVisible "#save" timeout=5000
  expectHidden "#loading"
  waitForHidden "#loading"
  expectEnabled "#save"
  expectDisabled "#archive"
  expectValue "#email" "agent@example.com" timeout=5000
  expectAttribute "#save" "aria-label" "Save profile"
  expectChecked "#marketing" false
  expectCount ".validation-error" 0
}
```

These assertions run in the page and report clear step failures. Wrap them in `step` blocks when a GIF should show a caption for the checked state.

## Evaluated Assertions

```text
setContent "<main><h1>Checkout</h1><output id='status'>Saved</output><script>window.appReady = true;</script></main>"
expectEval "window.appReady === true"
assertEval "document.querySelector('#status').textContent" equals=Saved
expectExpression "document.body.innerText" contains=Checkout
```

Evaluated assertions are useful when provider-style assertions need a page expression rather than a selector. They work in direct browser-control scripts and `cmg run`, and they report the actual value on failure.

This example is available as `demo-scripts/37-evaluated-assertions.cmgscript`.

## Direct Rich Locators

```text
navigate "index.html"
click "text=Open profile"
waitForElement "text=Edit Browser Profile"
type "label=Profile name" "Locator Agent"
assertText "text=Edit Browser Profile" "Edit Browser Profile"
hover "label=Profile name"
click "nth=.command|1"
assertText "hasText=.toast|Saved" "Saved"
```

The same locator resolver is used by direct browser-control scripts and `cmg run`. Filter locators such as `first=`, `last=`, `nth=`, `hasText=`, and `visible=` resolve CSS matches into a temporary marker. Pointer-aware actions still move the virtual pointer to the resolved element marker, so GIFs keep the pointer, browser events, and drag ghost behavior aligned.

Filter locator examples are available as `demo-scripts/41-locator-filters.cmgscript`. The structured `cmg run` form is available as `demo-scripts/42-locator-filters-runner.cmgscript`.

## Scroll And Wheel

```text
setContent "<main style='height:1600px'><section id='pane' style='height:80px; overflow:auto'><div style='height:500px'><button id='target'>Deep target</button></div></section></main>"
scrollTo bottom
expectEval "window.scrollY > 0"
scrollTo top
scrollBy 0 180 selector="#pane"
wheel "#pane" deltaY=120
expectEval "document.querySelector('#pane').scrollTop > 0"
```

`scrollTo` and `scrollBy` work on the window or `selector=<selector>` element. `wheel` dispatches a browser wheel event, scrolls the target, and moves the GIF virtual pointer when a selector, alias, or coordinate target is provided.

This direct-script example is available as `demo-scripts/43-scroll-wheel.cmgscript`. The structured `cmg run` form is available as `demo-scripts/44-scroll-wheel-runner.cmgscript`.

## Provider Aliases

```text
setContent "<!doctype html><title>Alias Demo</title><main><h1>Alias Demo</h1><input id='name'><input id='file' type='file'><div id='source' draggable='true'>Drag me</div><div id='target'>Drop here</div></main>"
toHaveTitle "Alias Demo"
toContainText "Alias Demo"
pressSequentially "#name" "CMG"
setInputFiles "#file" "index.html"
dragTo "#source" "#target"
```

These aliases run through the same CMG actions as `expectTitle`, text assertions, `type`, `uploadFiles`, and `dragAndDrop`, so GIF recordings still use the virtual pointer, pointer events, drag previews, and captions.

This direct-script example is available as `demo-scripts/45-provider-aliases.cmgscript`. The structured `cmg run` form is available as `demo-scripts/46-provider-aliases-runner.cmgscript`.

## Pointer Click Variants

```text
setContent "<button id='target'>Click target</button><output id='result'>none</output><script>const target = document.querySelector('#target'); const result = document.querySelector('#result'); target.addEventListener('dblclick', () => result.textContent = 'double'); target.addEventListener('contextmenu', event => { event.preventDefault(); result.textContent = 'right'; });</script>"
waitForElement "#target"
doubleClick "#target"
assertText "#result" "double"
contextClick "#target"
assertText "#result" "right"
```

This example is available as `demo-scripts/14-pointer-click-variants.cmgscript`.

## Environment Emulation

```text
test "mobile dark mode profile" {
  emulate width=390 height=844 userAgent="CMG Mobile" locale=en-GB colorScheme=dark reducedMotion=reduce
  navigate "https://example.com/profile"
  expectScreenshot baseline="baselines\profile-mobile-dark.png" output="demo-output\profile-mobile-dark.actual.png"
}

test "location-aware profile" {
  grantPermissions "geolocation"
  setGeolocation "51.5,-0.1" accuracy=10
  navigate "https://example.com/profile"
  expectText "#nearest-office" "London"
  clearPermissions
}
```

Use `emulate` before navigation when the page reads environment values during startup. The same action is available in direct browser-control scripts for AI-driven exploration.

## Page Metadata And Content

```text
test "inspects generated page" {
  setContent "<main><h1>CMG</h1></main>"
  title
  url
  content
  toHaveText "h1" "CMG"
}
```

Use `url`, `title`, and `content` when a script needs page state as parseable output. Use `setContent` for generated test pages or AI-driven browser setup.

## File Upload

```text
test "uploads avatar" {
  navigate "https://example.com/profile"
  step "Select avatar file" {
    uploadFiles "#avatar" "fixtures\avatar.png"
  }
  click "#save"
}
```

`uploadFiles` can also run inside `gif` blocks. The step caption gives the recording visible context while the page receives the same `input` and `change` events as a browser-driven file upload.

## Popup And Tab Flow

```text
test "opens help popup" {
  navigate "https://example.com"
  click "#help"
  waitForPopup count=2 timeout=5000
  activateTab index=1
  expectUrl "/help"
  closeTab index=1
  activateTab index=0
}
```

For direct browser-control scripts, `openTab "https://example.com/help"` can create the tab without going through page UI. In recorded test GIFs, user-facing popup flows should prefer `click` plus `waitForPopup` or `waitForTab` so the pointer interaction remains visible.

## Frame Flow

```text
test "saves checkout frame" {
  navigate "https://example.com/checkout"
  frameWaitForElement "#checkoutFrame" "#email" timeout=5000
  frameFill "#checkoutFrame" "#email" "agent@example.com"
  frameClick "#checkoutFrame" "#save"
  frameAssertText "#checkoutFrame" "#status" "Saved"
}
```

Recorded frame pointer actions move the virtual pointer to the element's top-page coordinate inside the iframe. Frame actions require a same-origin iframe.

## Download Flow

```text
test "exports report" {
  navigate "https://example.com/reports"
  download "#exportCsv" directory="demo-output" pattern="*.csv" timeout=10000
}
```

Use `download` when the page interaction should be visible in a GIF. Use `waitForDownload` when another action already triggered the download.

## Console Feedback

```text
test "logs save result" {
  navigate "https://example.com/settings"
  captureConsole
  click "#save"
  waitForConsole "settings saved" level=info timeout=5000
}
```

Console waits are useful when an app reports diagnostics through the browser console. Reports and traces include the `CONSOLE` output line.

## Page Error Feedback

```text
test "captures page failure" {
  navigate "https://example.com/settings"
  capturePageErrors
  click "#breakPage"
  waitForPageError "Cannot read" timeout=5000
}
```

Page-error capture records `error` and `unhandledrejection` events from the page. The output and failure reasons are included in reports and traces.

## Init Script

```text
test "preloads feature flag" {
  addInitScript "window.__featureFlag = true;"
  navigate "https://example.com"
  evaluate "window.__featureFlag"
}
```

Init scripts run before future page documents execute app scripts. The action is available in direct browser-control scripts and `cmg run`.

## Deterministic Time

```text
test "expires session banner" {
  navigate "https://example.com/app"
  clock now=1700000000000
  click "#startSession"
  tick 300000
  expectText "#sessionBanner" "Expired" timeout=1000
  restoreClock
}
```

Clock actions are shared by direct browser-control scripts and `cmg run`. They do not move the virtual pointer, so use a `step` caption when the time jump should be narrated in a GIF.

## Context Cleanup

```text
beforeEach {
  resetContext
  navigate "https://example.com/login"
}

test "fresh login state" {
  fill "#email" "agent@example.com"
  fill "#password" "secret"
  click "#submit"
}
```

Use `clearContext` when the page should stay loaded, and `resetContext` when the next action should start from `about:blank`.

## Isolated Browser Contexts

```text
test "separate session" {
  newContext ctx url="https://example.com/login"
  fill "#email" "agent@example.com"
  closeContext "${ctx}"
}
```

`newContext` activates the new context immediately. Later pointer-aware actions run in that context, so optional GIF recordings keep the same virtual pointer behavior inside the isolated page.

## Accessibility Snapshot

```text
test "dialog accessibility" {
  navigate "https://example.com/profile"
  click "#openProfileDialog"
  accessibilitySnapshot "#profileDialog" output="demo-output\profile-dialog.a11y.json"
  expectAccessible role=button name="Save"
}
```

Accessibility actions do not move the virtual pointer. Wrap them in `step` blocks when the GIF should narrate what is being checked.

## Storage State

```text
test "login saves state" {
  navigate "https://example.com/login"
  fill "#email" "agent@example.com"
  fill "#password" "secret"
  click "#submit"
  storageState save path="demo-output\auth.json"
}

test "uses saved state" {
  navigate "https://example.com"
  storageState load path="demo-output\auth.json"
  reload
}
```

## Storage Commands

```text
test "sets browser storage" {
  localStorage set "token" "abc"
  sessionStorage set "mode" "test"
  cookie set "cmg" "demo"
  localStorage get "token"
  cookie get "cmg"
}
```

## Navigation Controls

```text
test "browser history" {
  navigate "https://example.com/start"
  navigate "https://example.com/checkout"
  goBack
  waitForUrl "/start"
  goForward
  waitForUrl "/checkout"
  waitForNavigation "/checkout" waitUntil=load
  reload
  waitForLoadState "complete"
}
```

## Explicit Waits

```text
test "waits for app readiness" {
  navigate "https://example.com/app"
  waitForSelector "#app"
  waitForSelector "#app" state=visible timeout=5000
  waitForFunction "window.appReady === true" timeout=10000
  waitForTimeout 250
}
```

## Low-Level Mouse

```text
test "canvas drag" {
  mouseMove selector="#canvas" edge=center
  mouseDown selector="#canvas" edge=center
  mouseMove selector="#canvas" edge=bottomRight inset=20
  mouseUp selector="#canvas" edge=bottomRight inset=20
}
```

## Low-Level Keyboard

```text
test "keyboard shortcut" {
  click "#editor"
  keyDown "Control"
  press "A"
  keyUp "Control"
  insertText "Replacement text"
}
```

## Touch And Clipboard

```text
test "touch and clipboard" {
  setClipboard "CMG clipboard"
  readClipboard
  tap "#openProfileDialog"
  waitForElement "#profileDialog[open]"
  clearClipboard
}
```

`tap` and `touchTap` use the same selector and rich locator support as other pointer-aware actions. In GIF recordings the virtual pointer moves to the target and shows the tap pulse, while clipboard setup remains non-visual and appears in stdout, reports, and traces.

## Dialogs

```text
test "handles prompt" {
  captureDialogs
  setDialogBehavior accept promptText="CMG"
  click "#openPrompt"
  waitForDialog "Your name"
}
```

The same waits can be written through the provider-style event adapter:

```text
test "waits for events" {
  openTab "about:blank"
  waitForEvent popup count=2
  captureDialogs
  onDialog accept
  evaluate "alert('Saved')"
  waitForEvent dialog "Saved"
}
```

## Network Request Wait

```text
test "loads profile" {
  route "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json"
  click "#loadProfile"
  waitForRequest "/api/profile"
  waitForRequestFinished "/api/profile"
  waitForResponse "/api/profile"
}

test "reports offline profile failure" {
  setOffline true
  evaluate "fetch('/api/profile').catch(() => 'failed')"
  waitForRequestFailed "/api/profile"
  setOffline false
}

test "handles intercepted outage" {
  intercept "/api/profile" abort=true error="profile service unavailable"
  evaluate "fetch('/api/profile').catch(error => error.message)"
  waitForRequestFailed "/api/profile"
}

test "uses one-shot post intercept" {
  intercept "/api/profile" method=POST times=1 status=201 body="{\"saved\":true}" contentType="application/json"
  evaluate "fetch('/api/profile', { method: 'POST' }).then(response => response.status)"
  waitForRequestFinished "/api/profile"
  waitForResponse "/api/profile"
}
```

## WebSocket Routing

```text
test "captures socket traffic" {
  routeWebSocket "/socket" message="ready"
  evaluate "window.__socket = new WebSocket('wss://example.com/socket')"
  waitForWebSocket "/socket"
  waitForWebSocketMessage "ready"
  clearWebSocketRoutes
}
```

WebSocket routing is page-side and works in direct browser-control scripts and `cmg run`. It does not move the virtual pointer, but reports and traces include `WEBSOCKET_ROUTE`, `WEBSOCKET`, and `WEBSOCKET_MESSAGE` lines.

## API Request

```text
test "creates item through api" {
  apiRequest "POST" "https://example.com/api/items" json="{\"name\":\"demo\"}" query.preview=true header.Authorization="Bearer token" status=201 contains="demo"
}
```

`apiRequest` is runner-side HTTP and does not move the virtual pointer. Its `API` and `API_BODY` output is included in reports and traces.
