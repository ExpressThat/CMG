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
- Supports the same parity actions as the structured runner DSL, including `keyDown`, `keyUp`, `insertText`, `mouseMove`, `mouseDown`, `mouseUp`, `waitForSelector`, `waitForFunction`, `waitForTimeout`, `reload`, `goBack`, `goForward`, `waitForUrl`, `waitForLoadState`, `captureDialogs`, `setDialogBehavior`, `waitForDialog`, `localStorage`, `sessionStorage`, `cookie`, `apiRequest`, `storageState`, `newContext`, `useContext`, `closeContext`, `listWorkers`, `workerEvaluate`, `workerIntercept`, `addInitScript`, `evaluateOnNewDocument`, `startCoverage`, `stopCoverage`, `capturePageErrors`, `waitForPageError`, `setGeolocation`, `grantPermissions`, `clearPermissions`, `setExtraHTTPHeaders`, `clearExtraHTTPHeaders`, `setOffline`, `route`, `intercept`, `waitForRequest`, `waitForRequestFailed`, `waitForResponse`, `readFile`, `fixture`, `writeFile`, `appendFile`, `expectFile`, `printPdf`, `uploadFiles`, `expectScreenshot`, `openTab`, and `waitForTab`.
- Uses the selected browser automation protocol through the active CMG endpoint: Chrome DevTools Protocol for Chrome and Edge, WebDriver BiDi for Firefox.
- Browser JavaScript dialogs are handled explicitly. Add `captureDialogs` or `setDialogBehavior` before the action that opens an `alert`, `confirm`, or `prompt`.
- When `--gif` is provided, captures the visible page viewport after visual actions and writes an animated GIF. The `set` variable action is logged but does not add a standalone frame because it has no page-visible effect.
- GIF recording adds a virtual pointer in the browser page. The pointer is visible live during recording and is captured in the GIF frames.
- GIF pointer movement dispatches browser movement and hover events. This includes automatic pointer movement before `click`, `type`, `clear`, `hover`, `select`, and `dragAndDrop`, not only drag movement.
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
TAB 0 id=... title="..." url="..."
TAB_OPENED 007 https://example.com
TAB_COUNT 008 2
API 009 200 https://example.com/health
API_BODY 009 ok
STORAGE_STATE 010 saved C:\Projects\CMG\demo-output\auth.json
UPLOAD 011 1
VISUAL 012 diff=0
EMULATE 013 width height userAgent
GEOLOCATION 014 51.5,-0.1 accuracy=10
PERMISSIONS 015 geolocation,notifications
PERMISSIONS_CLEARED 016
RELOADED 017 https://example.com
URL 018 https://example.com/checkout
LOAD_STATE 019 complete
SELECTOR 020 #ready
FUNCTION 021 true
WAIT_TIMEOUT 022 250
KEY_DOWN 023 Shift
TEXT_INSERTED 024 3
KEY_UP 025 Shift
MOUSE_MOVED 026 400,300
MOUSE_DOWN 027 400,300
MOUSE_UP 028 400,300
LOCAL_STORAGE 029 set token
SESSION_STORAGE 030 get token abc
COOKIE 031 set mode
DOWNLOAD 032 C:\Projects\CMG\demo-output\report.csv
CONSOLE_CAPTURE 033
CONSOLE 034 info: settings saved
PAGE_ERROR_CAPTURE 035
PAGE_ERROR 036 error: Cannot read properties of null
DIALOG_CAPTURE 037
DIALOG_BEHAVIOR 038 accept
DIALOG 039 {"type":"alert","message":"Saved","accepted":true}
INIT_SCRIPT 040 ...
HEADERS_SET 041 2
OFFLINE 042 true
ROUTE 043 /api/profile
REQUEST 044 {"method":"GET","url":"/api/profile","type":"fetch","body":""}
REQUEST_FAILED 045 {"method":"GET","url":"/api/down","type":"fetch","error":"Failed to fetch"}
RESPONSE 046 {"url":"/api/profile","status":200,"mocked":true}
ROUTES_CLEARED 047
HAR_EXPORTED 048 C:\Projects\CMG\demo-output\network.har
HAR_REPLAY 049 routes=1 C:\Projects\CMG\demo-output\network.har
FRAME 050 frameClick
FRAME_EVALUATE 051 Checkout
CLOCK 052 1700000000000
TICK 053 250 now=1700000000250
CLOCK_RESTORED 054
CONTEXT_CLEARED 055
CONTEXT_RESET 056
ACCESSIBILITY 057 C:\Projects\CMG\demo-output\a11y.json
ACCESSIBLE 058 role=button name="Save"
CONTEXT_CREATED 059 id=... target=... url="about:blank"
CONTEXT_ACTIVE 060 ...
CONTEXT_CLOSED 061 ...
WORKER 062 id=... type=worker title="worker.js" url="https://example.com/worker.js"
WORKER_INTERCEPT 063 routes=1 /api/profile
COVERAGE_STARTED 064 js=true css=true
COVERAGE 065 C:\Projects\CMG\demo-output\coverage.json
FILE_READ 066 payload C:\Projects\CMG\fixtures\payload.json
FILE_WRITTEN 067 C:\Projects\CMG\demo-output\result.txt
FILE_APPENDED 068 C:\Projects\CMG\demo-output\result.txt
FILE_OK 069 C:\Projects\CMG\demo-output\result.txt
PDF 070 C:\Projects\CMG\demo-output\page.pdf
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
