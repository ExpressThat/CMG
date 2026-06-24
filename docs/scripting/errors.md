# `.cmgscript` Errors

Scripts stop on the first error.

## Browser Not Running

```text
No CMG-controlled Chrome instance is running. Run 'cmg browser launch' first.
```

Start Chrome first:

```powershell
cmg browser launch
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
