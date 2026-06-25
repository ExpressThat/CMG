# `.cmgscript` Examples

Runnable demo scripts live in [`../../demo-scripts/`](../../demo-scripts/).

Record a demo as a GIF:

```powershell
dotnet run -- browser control script --file demo-scripts\01-dialog-flow.cmgscript --gif demo-output\dialog-flow.gif
```

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
```

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
```

## Drag And Drop

```text
navigate "C:\Projects\CMG\index.html"
setViewport width=900 height=700
waitForElement "#dropQueue"
scrollIntoView "#dragdrop"
dragAndDrop "[data-command='browser launch']" "#dropQueue"
assertText "#dropQueue" "browser launch"
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

When `cmg run --gif` is used, the whole test is recorded and `gif` blocks are included in that recording instead of producing nested GIF files.

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

## Filtering, Sharding, And Retries

```powershell
dotnet run -- run demo-scripts --grep runner
dotnet run -- run demo-scripts --tag smoke
dotnet run -- run demo-scripts --retries 2
dotnet run -- run demo-scripts --shard 1/2
```

Use `tag=` on tests:

```text
test "checkout" tag=smoke,critical {
  click "#checkout"
}
```

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
  route "/api/profile" status=200 body="{\"name\":\"CMG\"}" contentType="application/json"
  evaluate "fetch('/api/profile').then(r => r.text())"
  waitForResponse "/api/profile"
}
```

This patches page `fetch` and `XMLHttpRequest` calls and records matching responses for `waitForResponse`.

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
  expectValue "#email" "agent@example.com" timeout=5000
  expectAttribute "#save" "aria-label" "Save profile"
  expectChecked "#marketing" false
  expectCount ".validation-error" 0
}
```

These assertions run in the page and report clear step failures. Wrap them in `step` blocks when a GIF should show a caption for the checked state.

## Environment Emulation

```text
test "mobile dark mode profile" {
  emulate width=390 height=844 userAgent="CMG Mobile" locale=en-GB colorScheme=dark reducedMotion=reduce
  navigate "https://example.com/profile"
  expectScreenshot baseline="baselines\profile-mobile-dark.png" output="demo-output\profile-mobile-dark.actual.png"
}
```

Use `emulate` before navigation when the page reads environment values during startup. The same action is available in direct browser-control scripts for AI-driven exploration.

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
  waitForTab count=2 timeout=5000
  activateTab index=1
  expectUrl "/help"
  closeTab index=1
  activateTab index=0
}
```

For direct browser-control scripts, `openTab "https://example.com/help"` can create the tab without going through page UI. In recorded test GIFs, user-facing popup flows should prefer `click` plus `waitForTab` so the pointer interaction remains visible.

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
