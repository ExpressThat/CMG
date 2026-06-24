# `.cmgscript` Errors

Scripts stop on the first error.

When `--gif <path>` is used, CMG still writes a partial GIF with the frames captured before the error, as long as recording had started.

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

## Missing Element

```text
Line 4: click failed. No element matched selector '#missing'.
```

Use `waitForElement` before actions that require an element.

## Failed Assertion

```text
Line 5: assertText failed. Expected text 'Ready' was not found. Actual text: 'Loading'.
```

The assertion checks whether the element text contains the expected text.

## Undefined Variable

```text
Line 3: click failed. Variable 'target' is not defined.
```

Define variables before using them:

```text
set target "#openProfileDialog"
click "${target}"
```
