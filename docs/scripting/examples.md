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
waitForElement "#dropQueue"
dragAndDrop "[data-command='browser launch']" "#dropQueue"
assertText "#dropQueue" "browser launch"
```

## Complex Drag And Drop

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#dropQueue"

dragAndDrop "[data-command='browser launch']" {
  delay 200
  hover "#lastDialogAction"
  delay 200
  hover "#dropQueue"
  drop "#dropQueue"
}

assertText "#dropQueue" "browser launch"
```

Run the complete example:

```powershell
dotnet run -- browser control script --file demo-scripts\07-complex-drag-flow.cmgscript --gif demo-output\complex-drag.gif
```

## Stdin

```powershell
@"
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog"
"@ | cmg browser control script --file -
```
