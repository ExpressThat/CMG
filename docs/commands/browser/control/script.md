# `browser control script`

Runs a `.cmgscript` browser automation script against the CMG-controlled Chrome instance.

```powershell
cmg browser control script --file <path>
cmg browser control script --file -
```

## Options

- `--file <path>`: Path to a `.cmgscript` file.
- `--file -`: Read script text from stdin.

## Behavior

- Requires a browser started with [`browser launch`](../launch.md).
- Executes actions in file order.
- Stops on the first failed action.
- Writes step logs and action outputs to stdout.
- Writes validation, parse, browser, and action errors to stderr.
- Uses Chrome DevTools Protocol through the active CMG remote debugging endpoint.

## Stdout

Successful steps write a pass log:

```text
PASS 001 navigate C:\Projects\CMG\index.html
PASS 002 waitForElement #openProfileDialog
```

Capture actions also write result lines:

```text
HTML 003 <button id="openProfileDialog" type="button">Open profile dialog</button>
SCREENSHOT 004 C:\Projects\CMG\profile-dialog.png
SCREENSHOT 005 data:image/png;base64,<base64-png-data>
EVALUATE 006 CMG Browser Control Test Page
TAB 0 id=... title="..." url="..."
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

More syntax and action details are documented in the [scripting guide](../../../scripting/index.md).
