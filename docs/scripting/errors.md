# `.cmgscript` Errors

Scripts stop on the first error.

When `--gif <path>` is used, CMG still writes a partial GIF with the frames captured before the error, as long as recording had started.

Browser JavaScript dialogs and leave-page prompts are automatically accepted while CMG is connected to a page. If a page repeatedly opens prompts, the action may still fail from timeout or a browser protocol error after CMG accepts the prompts it sees.

## Browser Not Running

```text
No CMG-controlled Chrome instance is running. Run 'cmg browser launch' first.
```

For Firefox, the equivalent error is:

```text
No CMG-controlled Firefox instance is running. Run 'cmg --firefox browser launch' first.
```

For Edge, the equivalent error is:

```text
No CMG-controlled Edge instance is running. Run 'cmg --edge browser launch' first.
```

Start the selected browser first:

```powershell
cmg browser launch
cmg --edge browser launch
cmg --firefox browser launch
```

## Missing Script File

```text
Script file 'flow.cmgscript' was not found.
```

Check the path passed to `--file`.

## Navigation Failed

Invalid URLs and missing path-like local targets fail the `navigate` action:

```text
Line 1: navigate failed. Cannot navigate to invalid URL
```

```text
Line 1: navigate failed. Navigation target path 'C:\Projects\CMG\missing.html' was not found.
```

For local files, pass an existing path. For web pages, pass a valid URL including the scheme, such as `https://example.com`.

## Invalid Syntax

```text
Line 3: unterminated quoted string.
```

Common causes:

- Missing closing quote.
- Using inline comments. Only full-line comments are supported.
- Forgetting to quote arguments with spaces.
- Opening a block without a closing `}`.
- Closing a block with `}` when no block is open.

## Invalid Block Action

```text
Line 4: dragAndDrop failed. Block dragAndDrop requires a drop action.
```

Complex `dragAndDrop` blocks must contain exactly one `drop` action and no actions after it.

Unsupported child actions fail clearly:

```text
Line 4: dragAndDrop failed. Action 'type' is not supported inside block dragAndDrop.
```

## Unknown Action

```text
Line 2: doThing failed. Unknown action 'doThing'.
```

Check [actions.md](actions.md) for the supported action list.

## Planned But Not Implemented

Some parity actions are reserved in the new runner DSL before their browser protocol implementation is complete:

```text
Line 4: evaluate failed. CMG action 'intercept' is planned but not implemented in this slice.
```

This is intentional. The action failed because it is not available yet, not because the page behaved incorrectly.

Invalid locator matches fail before the pointer action runs:

```text
STEP FAIL line=7 action=click reason=Line 7: evaluate failed. No element matched locator role=button.
```

## Runner Step Failure

`cmg run` prints step diagnostics on stderr:

```text
STEP FAIL line=8 action=click reason=Line 8: click failed. No element matched selector '#missing'.
```

JSON and HTML reports include the same reason, the test name, output lines, and any GIF path connected to the failure.

## Invalid Shard

```text
--shard must use index/count with 1 <= index <= count.
```

Examples of valid values: `1/2`, `2/2`, `3/5`.

## Missing Element

```text
Line 4: click failed. No element matched selector '#missing'.
```

Use `waitForElement` before actions that require an element.

## Element Outside Viewport

User-like actions such as `click`, `type`, `clear`, `hover`, `select`, and `dragAndDrop` do not scroll automatically:

```text
Line 3: click failed. Element '#dropQueue' is outside the current viewport. Run scrollIntoView first if this movement should scroll the page.
```

Add an explicit `scrollIntoView "<selector>"` step before the action. For drag-and-drop, make sure the source and target can both fit in the current viewport, or use `setViewport` before scrolling.

## `moveMouse` Without GIF

```text
Line 3: moveMouse failed. moveMouse requires script GIF recording. Run the script with --gif <path>.
```

`moveMouse` exists only for `browser control script --gif` runs. It is not available as a one-off CLI command and does not run in non-GIF scripts.

## Invalid `moveMouse` Target

```text
Line 3: moveMouse failed. Unknown moveMouse alias 'lower'. Supported aliases: center, top, bottom, left, right, topLeft, topRight, bottomLeft, bottomRight.
```

Use a supported alias or pass viewport-relative coordinates:

```text
moveMouse "bottom"
moveMouse x=100 y=200
moveMouse selector=".content-area" edge=bottom
```

Coordinates outside the visible viewport fail:

```text
Line 3: moveMouse failed. moveMouse target (2000, 2000) is outside the current viewport 1280x720.
```

Element-edge targeting requires both a selector and an edge:

```text
Line 3: moveMouse failed. moveMouse element-edge targeting requires edge=<top|bottom|left|right|center|topLeft|topRight|bottomLeft|bottomRight>.
```

## Failed Assertion

```text
Line 5: assertText failed. Expected text 'Ready' was not found. Actual text: 'Loading'.
```

The assertion checks whether the element text contains the expected text.

## API Request Failure

```text
STEP FAIL line=3 action=apiRequest reason=Expected status 200, got 500.
```

`apiRequest` failures include the reason in stderr and reports. The step output includes `API` and `API_BODY` lines when a response was received.

## Network Wait Failure

```text
STEP FAIL line=6 action=waitForResponse reason=Line 6: evaluate failed. Timed out waiting for response /api/profile.
```

Install a matching `route` or make sure the page performs a matching `fetch()` before the timeout expires.

## Storage State Failure

```text
STEP FAIL line=4 action=storageState reason=Storage state file 'auth.json' was not found.
```

Use `storageState save path="..."` before loading a state file.

## Undefined Variable

```text
Line 3: click failed. Variable 'target' is not defined.
```

Define variables before using them:

```text
set target "#openProfileDialog"
click "${target}"
```
