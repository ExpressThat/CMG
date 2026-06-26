# `browser control script`

Runs a `.cmgscript` browser automation script against the selected CMG-controlled browser instance. This command is the direct AI/browser-control scripting surface. It is intentionally separate from `cmg run`, which executes the new test DSL and does not run V1 flat scripts. Chrome is the default. Use `--chrome` to select Chrome explicitly, `--edge` for Microsoft Edge, or `--firefox` for Firefox.

```powershell
cmg browser control script --file <path>
cmg browser control script --file -
cmg browser control script --file <path> --gif <path>
cmg --chrome browser control script --file <path>
cmg --edge browser control script --file <path>
cmg --firefox browser control script --file <path>
```

## Options

- `--file <path>`: Path to a `.cmgscript` file.
- `--file -`: Read script text from stdin.
- `--gif <path>`: Optional output path for an animated GIF recording of the script run.

## Behavior

- Requires a browser started with [`browser launch`](../launch.md). For Edge, use `cmg --edge browser launch`. For Firefox, use `cmg --firefox browser launch`.
- Use [`validateScript`](validateScript.md) to check imports and syntax before connecting to a browser.
- Executes actions in file order.
- Stops on the first failed action.
- Writes step logs and action outputs to stdout.
- Writes validation, parse, browser, and action errors to stderr.
- Supports line-level `import "path"` statements. Relative imports resolve from the script file's directory.
- Supports control flow and reuse actions: `if`, `elseif`, `else`, `switch`, `case`, `default`, `for`, `repeat`, `while`, `until`, `doWhile`, `doUntil`, `retry`, `foreach`, `foreachSelector`, `break`, `continue`, `try`, `catch`, `finally`, `macro`, `call`, and `return`.
- Supports the same parity actions as the structured runner DSL, including `goto`, `visit`, `viewport`, `setViewportSize`, `wait`, `caption`, `fail`, `assertVisible`, `expectText`, `toHaveText`, `toContainText`, `contains`, `containsText`, `waitForText`, `notContains`, `expectNoText`, `expectNotText`, `notContainsText`, `toNotContainText`, `toHaveNoText`, `expectEval`, `assertEval`, `expectExpression`, `assertExpression`, `fill`, `pressSequentially`, `check`, `uncheck`, `focus`, `blur`, `selectText`, `dblclick`, `doubleClick`, `rightClick`, `contextClick`, `tap`, `touchTap`, `selectOption`, `dispatchEvent`, `keyDown`, `keyUp`, `insertText`, `setClipboard`, `writeClipboard`, `readClipboard`, `clearClipboard`, `mouseMove`, `mouseDown`, `mouseUp`, `scrollTo`, `scrollBy`, `wheel`, `waitForSelector`, `waitForFunction`, `waitForTimeout`, `waitForEvent`, `reload`, `goBack`, `goForward`, `waitForUrl`, `waitForTitle`, `toHaveURL`, `toHaveTitle`, `waitForLoadState`, `waitForNavigation`, `url`, `title`, `content`, `setContent`, `textContent`, `innerText`, `inputValue`, `getAttribute`, `evaluateOnSelector`, `evalOnSelector`, `evaluateAll`, `evalAll`, `captureConsole`, `waitForConsole`, `expectNoConsole`, `toHaveNoConsole`, `captureDialogs`, `setDialogBehavior`, `onDialog`, `handleDialog`, `dialogBehavior`, `waitForDialog`, `localStorage`, `sessionStorage`, `cookie`, `apiRequest`, `storageState`, `newContext`, `useContext`, `closeContext`, `listWorkers`, `workerEvaluate`, `workerIntercept`, `addInitScript`, `evaluateOnNewDocument`, `addScriptTag`, `addStyleTag`, `exposeFunction`, `exposeBinding`, `startCoverage`, `stopCoverage`, `capturePageErrors`, `waitForPageError`, `setGeolocation`, `grantPermissions`, `clearPermissions`, `setJavaScriptEnabled`, `bypassCSP`, `serviceWorkers`, `setExtraHTTPHeaders`, `clearExtraHTTPHeaders`, `setHttpCredentials`, `clearHttpCredentials`, `setProxy`, `clearProxy`, `setOffline`, `route`, `intercept`, `routeWebSocket`, `waitForWebSocket`, `waitForWebSocketMessage`, `waitForRequest`, `waitForRequestFinished`, `waitForRequestFailed`, `waitForResponse`, `readFile`, `fixture`, `writeFile`, `appendFile`, `expectFile`, `expectVisible`, `expectHidden`, `waitForVisible`, `waitForHidden`, `expectEnabled`, `expectDisabled`, `expectAttached`, `expectDetached`, `expectEditable`, `expectEmpty`, `expectFocused`, `expectInViewport`, `expectValue`, `expectValues`, `expectAttribute`, `expectClass`, `expectId`, `expectCSS`, `expectProperty`, `expectAccessibleName`, `expectRole`, `expectChecked`, `expectCount`, `toBeVisible`, `toBeHidden`, `toBeEnabled`, `toBeDisabled`, `toBeAttached`, `toBeDetached`, `toBeEditable`, `toBeEmpty`, `toBeFocused`, `toBeInViewport`, `toHaveValue`, `toHaveValues`, `toHaveAttribute`, `toHaveClass`, `toHaveId`, `toHaveCSS`, `toHaveJSProperty`, `toHaveAccessibleName`, `toHaveRole`, `toBeChecked`, `toHaveCount`, `printPdf`, `uploadFiles`, `setInputFiles`, `selectFile`, `expectScreenshot`, `toHaveScreenshot`, `dragTo`, `openTab`, `waitForTab`, and `waitForPopup`.
- Uses the selected browser automation protocol through the active CMG endpoint: Chrome DevTools Protocol for Chrome and Edge, WebDriver BiDi for Firefox.
- Browser JavaScript dialogs are handled explicitly. CMG does not silently remove, accept, or dismiss dialogs through the browser protocol. Add `captureDialogs` or `setDialogBehavior` before the action that opens an `alert`, `confirm`, or `prompt`.
- When `--gif` is provided, captures the visible page viewport after visual actions and writes an animated GIF. The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- Script-level `gif "name" { ... }`, `recordVideo "name" { ... }`, and `screencast "name" { ... }` blocks record only the wrapped actions when `--gif` is not provided. When `--gif` is provided, the whole script is recorded and nested block recordings are suppressed.
- Selector actions support the same rich locators as `cmg run`, including `text=`, `role=`, `label=`, `testid=`, `placeholder=`, `alt=`, `title=`, `xpath=`, `first=`, `last=`, `nth=selector|index`, `hasText=selector|text`, and `visible=`. Pointer-aware actions resolve the locator to a temporary element marker before moving the virtual pointer.
- GIF recording adds a virtual pointer in the browser page. The pointer is visible live during recording and is captured in the GIF frames.
- GIF pointer movement dispatches browser movement and hover events. This includes automatic pointer movement before `click`, `dblclick`, `doubleClick`, `rightClick`, `contextClick`, `tap`, `touchTap`, `type`, `fill`, `clear`, `hover`, `select`, `selectOption`, `check`, `uncheck`, `focus`, `blur`, `selectText`, and `dragAndDrop`, not only drag movement.
- `moveMouse` is available only inside scripts run with `--gif`; there is no one-off CLI `browser control moveMouse` command.
- If the script fails, CMG still writes a partial GIF containing frames captured before the failure.

## Stdout

Successful steps write a pass log:

```text
PASS 001 navigate C:\Projects\CMG\index.html
NAVIGATED 001 file:///C:/Projects/CMG/index.html
PASS 002 waitForElement #openProfileDialog
```

Capture actions also write result lines:

```text
HTML 003 <button id="openProfileDialog" type="button">Open profile dialog</button>
SCREENSHOT 004 C:\Projects\CMG\profile-dialog.png
SCREENSHOT 005 data:image/png;base64,<base64-png-data>
EVALUATE 006 CMG Browser Control Test Page
EXPECT_EVAL 006 true
URL 007 https://example.com/profile
TITLE 008 CMG Browser Control Test Page
CONTENT 009 <html>...</html>
CONTENT_SET 010 length=16
MOUSE_EVENT 011 dblclick #save
MOUSE_EVENT 012 contextmenu #menu
TAP 013 #touchTarget
TAB 0 id=... title="..." url="..."
TAB_OPENED 011 https://example.com
TAB_COUNT 012 2
API 013 200 https://example.com/health
API_BODY 013 ok
STORAGE_STATE 014 saved C:\Projects\CMG\demo-output\auth.json
UPLOAD 015 1
VISUAL 016 diff=0
EMULATE 017 width height userAgent
GEOLOCATION 018 51.5,-0.1 accuracy=10
PERMISSIONS 019 geolocation,notifications
PERMISSIONS_CLEARED 020
JAVASCRIPT_ENABLED 021 false
CSP_BYPASS 022 true
SERVICE_WORKERS 023 block
RELOADED 024 https://example.com
URL 025 https://example.com/checkout
LOAD_STATE 026 complete
SELECTOR 027 #ready
FUNCTION 028 true
WAIT_TIMEOUT 029 250
KEY_DOWN 030 Shift
TEXT_INSERTED 031 3
KEY_UP 032 Shift
CLIPBOARD_SET 033 5
CLIPBOARD 034 hello
CLIPBOARD_CLEARED 035
MOUSE_MOVED 033 400,300
MOUSE_DOWN 034 400,300
MOUSE_UP 035 400,300
LOCAL_STORAGE 036 set token
SESSION_STORAGE 037 get token abc
COOKIE 038 set mode
DOWNLOAD 039 C:\Projects\CMG\demo-output\report.csv
CONSOLE_CAPTURE 040
CONSOLE 041 info: settings saved
CONSOLE_OK 041 level=error
PAGE_ERROR_CAPTURE 042
PAGE_ERROR 043 error: Cannot read properties of null
DIALOG_CAPTURE 044
DIALOG_BEHAVIOR 045 accept
DIALOG 046 {"type":"alert","message":"Saved","accepted":true}
INIT_SCRIPT 047 ...
EXPOSED_FUNCTION 048 cmgAdd
GIF_BLOCK_SUPPRESSED 049
SET 050 pageTitle CMG Browser Control Test Page
RETRY 050 attempt=1 failed=Line 51: assertText failed. Expected text 'Ready' was not found. Actual text: 'Waiting'.
RETRY 050 success attempt=2
HEADERS_SET 050 2
HTTP_CREDENTIALS_SET 051 agent
HTTP_CREDENTIALS_CLEARED 052
PROXY_SET 053 https://proxy.local/?url=
PROXY_CLEARED 054
OFFLINE 055 true
ROUTE 056 /api/profile
REQUEST 057 {"method":"GET","url":"/api/profile","type":"fetch","body":""}
REQUEST_FAILED 058 {"method":"GET","url":"/api/down","type":"fetch","mocked":true,"error":"profile service unavailable"}
RESPONSE 059 {"url":"/api/profile","status":200,"mocked":true}
ROUTES_CLEARED 060
WEBSOCKET_ROUTE 061 /socket
WEBSOCKET 062 {"url":"/socket","routed":true}
WEBSOCKET_MESSAGE 063 {"url":"/socket","data":"ready","routed":true}
WEBSOCKET_ROUTES_CLEARED 064
HAR_EXPORTED 065 C:\Projects\CMG\demo-output\network.har
HAR_REPLAY 066 routes=1 C:\Projects\CMG\demo-output\network.har
FRAME 067 frameClick
FRAME_EVALUATE 068 Checkout
CLOCK 069 1700000000000
TICK 070 250 now=1700000000250
CLOCK_RESTORED 071
CONTEXT_CLEARED 072
CONTEXT_RESET 073
ACCESSIBILITY 074 C:\Projects\CMG\demo-output\a11y.json
ACCESSIBLE 075 role=button name="Save"
CONTEXT_CREATED 076 id=... target=... url="about:blank"
CONTEXT_ACTIVE 077 ...
CONTEXT_CLOSED 078 ...
WORKER 079 id=... type=worker title="worker.js" url="https://example.com/worker.js"
WORKER_INTERCEPT 080 routes=1 /api/profile
COVERAGE_STARTED 081 js=true css=true
COVERAGE 082 C:\Projects\CMG\demo-output\coverage.json
FILE_READ 083 payload C:\Projects\CMG\fixtures\payload.json
FILE_WRITTEN 084 C:\Projects\CMG\demo-output\result.txt
FILE_APPENDED 085 C:\Projects\CMG\demo-output\result.txt
FILE_OK 086 C:\Projects\CMG\demo-output\result.txt
PDF 087 C:\Projects\CMG\demo-output\page.pdf
GIF C:\Projects\CMG\demo-output\dialog-flow.gif
```

## Stderr

Failure output includes the script line number and action:

```text
Line 4: waitForElement failed. No element matched selector '#missing'.
```

## Exit Codes

- `0`: Script completed successfully.
- `1`: Browser is not running, script cannot be read, script syntax is invalid, or an action fails.

## Example

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000
click "#openProfileDialog"
waitForElement "#profileDialog[open]"
type "#profileName" "CMG Test Profile"
delay 500
screenshot "#profileDialog" output="profile-dialog.png"
assertText "#lastDialogAction" "None"
```

Message bar example:

```text
showMessageBar "Opening the profile dialog"
```

Complex drag example:

```text
dragAndDrop "[data-command='browser launch']" {
  delay 200
  hover "#lastDialogAction"
  drop "#dropQueue"
}
```

Drag edge-autoscroll example:

```text
dragAndDrop ".card" {
  moveMouse "bottom"
  delay 1500
  drop "#target"
}
```

More syntax and action details are documented in the [scripting guide](../../../scripting/index.md).
