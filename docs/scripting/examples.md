# `.cmgscript` Examples

Runnable demo scripts live in [`../../demo-scripts/`](../../demo-scripts/).

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

## Stdin

```powershell
@"
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog"
"@ | cmg browser control script --file -
```
