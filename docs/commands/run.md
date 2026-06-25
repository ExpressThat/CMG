# `run`

Runs CMG DSL test files.

```powershell
cmg run <path> [options]
```

`<path>` can be one `.cmgscript` file or a directory. Directories are searched recursively for `.cmgscript` files.

## Options

- `--gif <directory>` / `-gif <directory>`: Record GIFs for the entire execution of each test.
- `--report-json <file>`: Write a JSON test report.
- `--report-html <file>`: Write an HTML test report.
- `--report-junit <file>`: Write a JUnit XML test report.
- `--chrome`: Use Chrome. This is the default.
- `--edge`: Use Microsoft Edge.
- `--firefox`: Use Firefox.

## Output

Stdout prints one parseable line per test:

```text
TEST PASS <name>
TEST FAIL <name>
```

Failures may include action output before the failing test line. Stderr contains the final error when one is available.

When a step fails, stderr also includes:

```text
STEP FAIL line=<line> action=<action> reason=<reason>
```

Reports include per-test output and per-step diagnostics so agents can explain why a run failed.

## GIF Behavior

GIF recording is optional.

- With `--gif` or `-gif`, CMG records the whole execution of each test.
- When command-level GIF recording is active, script-level `gif { ... }` blocks do not create nested recordings; their actions are flattened into the whole-test GIF.
- Without command-level GIF recording, script-level `gif "name" { ... }` records only the wrapped block.

All recorded actions use CMG's virtual pointer, pointer/mouse event dispatch, captions, and drag ghost behavior.

## Exit Codes

- `0`: All tests passed.
- `1`: At least one test failed, no script files matched, the selected browser is invalid, or the selected browser is not running.

## Examples

```powershell
cmg browser launch
cmg run demo-scripts
cmg run tests\flows --gif artifacts\gifs
cmg run checkout.cmgscript --report-json artifacts\checkout.json --report-html artifacts\checkout.html
```
