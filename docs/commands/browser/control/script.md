# `browser control script`

Runs a `.cmgscript` browser automation script against the selected CMG-controlled browser instance. Chrome is the default. Use `--chrome` to select Chrome explicitly, `--edge` for Microsoft Edge, or `--firefox` for Firefox.

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
- Uses the selected browser automation protocol through the active CMG endpoint: Chrome DevTools Protocol for Chrome and Edge, WebDriver BiDi for Firefox.
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
