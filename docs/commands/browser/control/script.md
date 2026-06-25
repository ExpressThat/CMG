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
- Executes actions in file order.
- Stops on the first failed action.
- Writes step logs and action outputs to stdout.
- Writes validation, parse, browser, and action errors to stderr.
- Supports the same parity actions as the structured runner DSL, including `wait`, `caption`, `assertVisible`, `expectText`, `fill`, `check`, `uncheck`, `focus`, `blur`, `selectText`, `dblclick`, `rightClick`, `selectOption`, `keyDown`, `keyUp`, `insertText`, `mouseMove`, `mouseDown`, `mouseUp`, `waitForSelector`, `waitForFunction`, `waitForTimeout`, `reload`, `goBack`, `goForward`, `waitForUrl`, `waitForLoadState`, `waitForNavigation`, `url`, `title`, `content`, `setContent`, `captureDialogs`, `setDialogBehavior`, `waitForDialog`, `localStorage`, `sessionStorage`, `cookie`, `apiRequest`, `storageState`, `newContext`, `useContext`, `closeContext`, `listWorkers`, `workerEvaluate`, `workerIntercept`, `addInitScript`, `evaluateOnNewDocument`, `addScriptTag`, `addStyleTag`, `startCoverage`, `stopCoverage`, `capturePageErrors`, `waitForPageError`, `setGeolocation`, `grantPermissions`, `clearPermissions`, `setExtraHTTPHeaders`, `clearExtraHTTPHeaders`, `setOffline`, `route`, `intercept`, `waitForRequest`, `waitForRequestFinished`, `waitForRequestFailed`, `waitForResponse`, `readFile`, `fixture`, `writeFile`, `appendFile`, `expectFile`, `expectVisible`, `expectHidden`, `expectEnabled`, `expectDisabled`, `expectValue`, `expectAttribute`, `expectChecked`, `expectCount`, `printPdf`, `uploadFiles`, `expectScreenshot`, `openTab`, `waitForTab`, and `waitForPopup`.
- Uses the selected browser automation protocol through the active CMG endpoint: Chrome DevTools Protocol for Chrome and Edge, WebDriver BiDi for Firefox.
- Browser JavaScript dialogs are handled explicitly. CMG does not silently remove, accept, or dismiss dialogs through the browser protocol. Add `captureDialogs` or `setDialogBehavior` before the action that opens an `alert`, `confirm`, or `prompt`.
- When `--gif` is provided, captures the visible page viewport after visual actions and writes an animated GIF. The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- Selector actions support the same rich locators as `cmg run`, including `text=`, `role=`, `label=`, `testid=`, `placeholder=`, `alt=`, `title=`, and `xpath=`. Pointer-aware actions resolve the locator to a temporary element marker before moving the virtual pointer.
- GIF recording adds a virtual pointer in the browser page. The pointer is visible live during recording and is captured in the GIF frames.
- GIF pointer movement dispatches browser movement and hover events. This includes automatic pointer movement before `click`, `dblclick`, `rightClick`, `type`, `fill`, `clear`, `hover`, `select`, `selectOption`, `check`, `uncheck`, `focus`, `blur`, `selectText`, and `dragAndDrop`, not only drag movement.
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
URL 007 https://example.com/profile
TITLE 008 CMG Browser Control Test Page
CONTENT 009 <html>...</html>
CONTENT_SET 010 length=16
MOUSE_EVENT 011 dblclick #save
MOUSE_EVENT 012 contextmenu #menu
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
RELOADED 021 https://example.com
URL 022 https://example.com/checkout
LOAD_STATE 023 complete
SELECTOR 024 #ready
FUNCTION 025 true
WAIT_TIMEOUT 026 250
KEY_DOWN 027 Shift
TEXT_INSERTED 028 3
KEY_UP 029 Shift
MOUSE_MOVED 030 400,300
MOUSE_DOWN 031 400,300
MOUSE_UP 032 400,300
LOCAL_STORAGE 033 set token
SESSION_STORAGE 034 get token abc
COOKIE 035 set mode
DOWNLOAD 036 C:\Projects\CMG\demo-output\report.csv
CONSOLE_CAPTURE 037
CONSOLE 038 info: settings saved
PAGE_ERROR_CAPTURE 039
PAGE_ERROR 040 error: Cannot read properties of null
DIALOG_CAPTURE 041
DIALOG_BEHAVIOR 042 accept
DIALOG 043 {"type":"alert","message":"Saved","accepted":true}
INIT_SCRIPT 044 ...
HEADERS_SET 045 2
OFFLINE 046 true
ROUTE 047 /api/profile
REQUEST 048 {"method":"GET","url":"/api/profile","type":"fetch","body":""}
REQUEST_FAILED 049 {"method":"GET","url":"/api/down","type":"fetch","mocked":true,"error":"profile service unavailable"}
RESPONSE 050 {"url":"/api/profile","status":200,"mocked":true}
ROUTES_CLEARED 051
HAR_EXPORTED 052 C:\Projects\CMG\demo-output\network.har
HAR_REPLAY 053 routes=1 C:\Projects\CMG\demo-output\network.har
FRAME 054 frameClick
FRAME_EVALUATE 055 Checkout
CLOCK 056 1700000000000
TICK 057 250 now=1700000000250
CLOCK_RESTORED 058
CONTEXT_CLEARED 059
CONTEXT_RESET 060
ACCESSIBILITY 061 C:\Projects\CMG\demo-output\a11y.json
ACCESSIBLE 062 role=button name="Save"
CONTEXT_CREATED 063 id=... target=... url="about:blank"
CONTEXT_ACTIVE 064 ...
CONTEXT_CLOSED 065 ...
WORKER 066 id=... type=worker title="worker.js" url="https://example.com/worker.js"
WORKER_INTERCEPT 067 routes=1 /api/profile
COVERAGE_STARTED 068 js=true css=true
COVERAGE 069 C:\Projects\CMG\demo-output\coverage.json
FILE_READ 070 payload C:\Projects\CMG\fixtures\payload.json
FILE_WRITTEN 071 C:\Projects\CMG\demo-output\result.txt
FILE_APPENDED 072 C:\Projects\CMG\demo-output\result.txt
FILE_OK 073 C:\Projects\CMG\demo-output\result.txt
PDF 074 C:\Projects\CMG\demo-output\page.pdf
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
