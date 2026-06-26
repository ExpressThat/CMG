# `browser control validateScript`

Validates a `.cmgscript` browser automation script without launching or connecting to a browser.

```powershell
cmg browser control validateScript --file <path>
cmg browser control validateScript --file -
```

## Options

- `--file <path>`: Path to a `.cmgscript` file.
- `--file -`: Read script text from stdin.

## Behavior

- Expands line-level `import "path"` statements before parsing.
- Resolves relative imports from the validated script file's directory.
- Parses block structure, quoted strings, options, nested control flow, macros, and weird whitespace formatting.
- Does not execute browser actions, JavaScript, assertions, macros, GIF recording, or network/file actions.
- Does not require `browser launch`, `--chrome`, `--edge`, or `--firefox`.

## Stdout

On success, writes one parseable line:

```text
SCRIPT VALID actions=<count>
```

`actions` is the number of top-level parsed script actions after import expansion.

## Stderr

On failure, writes one reason. Examples:

```text
Line 2: missing block close '}'.
Imported script 'C:\Projects\CMG\missing.cmgscript' was not found.
Line 4: unterminated quoted string.
```

## Exit Codes

- `0`: The script was read, imports expanded, and syntax parsed successfully.
- `1`: The script file was missing, an import failed, or syntax was invalid.

## Examples

```powershell
cmg browser control validateScript --file demo-scripts\48-weird-formatting.cmgscript
Get-Content .\flow.cmgscript | cmg browser control validateScript --file -
```
