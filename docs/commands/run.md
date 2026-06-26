# `run`

Runs CMG DSL test files.

```powershell
cmg run <path> [options]
```

`<path>` can be one `.cmgscript` file or a directory. Directories are searched recursively for `.cmgscript` files.

`cmg run` accepts only the new CMG DSL. V1 flat scripts are intentionally unsupported and fail with a migration error. Wrap actions in `test`/`it`/`specify` or `suite`/`describe`/`context` blocks and follow `docs/scripting/migration.md` when migrating old scripts.

The runner supports line-level `import "path"` statements. Relative imports resolve from the importing file's directory before parsing. Top-level macros from the file or imported files are registered before each test, and suite-level macros are registered before tests in that suite.

Runner hooks include `beforeAll`, `afterAll`, `beforeEach`, and `afterEach`. Once hooks run for the first or last non-skipped selected test in their file or suite scope, so grep/tag/only/shard selection controls which scopes execute setup and teardown.

## Options

- `--gif <directory>` / `-gif <directory>`: Record GIFs for the entire execution of each test.
- `--report-json <file>`: Write a JSON test report.
- `--report-html <file>`: Write an HTML test report.
- `--report-junit <file>`: Write a JUnit XML test report.
- `--trace <directory>`: Write per-test trace JSON files.
- `--grep <text>`: Run tests whose names contain the text.
- `--tag <tag>`: Run tests with a matching `tag=` option.
- `--retries <count>`: Retry failed tests this many times.
- `--max-failures <count>`: Stop scheduling tests after this many failed tests. `0` disables fail-fast behavior.
- `--repeat-each <count>`: Run each selected test this many times. Values below `1` are treated as `1`.
- `--list`: List selected tests without connecting to a browser or running actions.
- `--shard <index/count>`: Run a deterministic shard, for example `1/3`.
- `--chrome`: Use Chrome. This is the default.
- `--edge`: Use Microsoft Edge.
- `--firefox`: Use Firefox.

## Output

Stdout prints one parseable line per test:

```text
TEST PASS <name>
TEST FAIL <name>
TEST SKIP <name>
RUN STOP maxFailures=<count>
TEST LIST <run|skip> <name>
```

Failures may include action output before the failing test line. Skipped tests do not run actions or produce GIFs. `RUN STOP maxFailures=<count>` means `--max-failures` stopped the run after the threshold was reached. `TEST LIST` lines are emitted by `--list` and show the selected schedule without browser execution. Stderr contains the final error when one is available.

When a step fails, stderr also includes:

```text
STEP FAIL line=<line> action=<action> reason=<reason>
```

Reports and traces include per-test status, output, and per-step diagnostics so agents can explain why a run failed. JSON reports include `status` values such as `passed`, `failed`, and `skipped`; JUnit reports emit `<skipped>` nodes for skipped tests.

## GIF Behavior

GIF recording is optional.

- With `--gif` or `-gif`, CMG records the whole execution of each test.
- When command-level GIF recording is active, script-level `gif { ... }`, `recordVideo { ... }`, and `screencast { ... }` blocks do not create nested recordings; their actions are flattened into the whole-test GIF.
- Without command-level GIF recording, script-level `gif "name" { ... }`, `recordVideo "name" { ... }`, or `screencast "name" { ... }` records only the wrapped block.
- If `--max-failures` stops the run, GIFs and reports include only tests that actually ran before the stop.
- With `--repeat-each`, each repeat is a separate scheduled test with a distinct name such as `checkout [repeat 2/3]`, so per-test GIFs, traces, reports, retries, and sharding remain deterministic.

All recorded actions use CMG's virtual pointer, pointer/mouse event dispatch, captions, and drag ghost behavior.

Shared pointer, keyboard, assertion, and non-visual actions such as `goto`, `visit`, `viewport`, `setViewportSize`, `tap`, `touchTap`, `keyDown`, `keyUp`, `insertText`, `pressSequentially`, `contains`, `containsText`, `toContainText`, `waitForText`, `toHaveText`, `expectEval`, `assertEval`, `expectExpression`, `assertExpression`, `fail`, `waitForVisible`, `waitForHidden`, `toBeVisible`, `toBeHidden`, `toBeEnabled`, `toBeDisabled`, `toBeAttached`, `toBeDetached`, `toBeEditable`, `toBeEmpty`, `toBeFocused`, `toBeInViewport`, `toHaveValue`, `toHaveValues`, `toHaveAttribute`, `toHaveClass`, `toHaveId`, `toHaveCSS`, `toHaveJSProperty`, `toHaveAccessibleName`, `toHaveRole`, `toBeChecked`, `toHaveCount`, `setClipboard`, `writeClipboard`, `readClipboard`, `clearClipboard`, `mouseMove`, `mouseDown`, `mouseUp`, `scrollTo`, `scrollBy`, `wheel`, `waitForSelector`, `waitForFunction`, `waitForTimeout`, `waitForEvent`, `reload`, `goBack`, `goForward`, `waitForUrl`, `waitForTitle`, `toHaveURL`, `toHaveTitle`, `waitForLoadState`, `waitForNavigation`, `url`, `title`, `content`, `setContent`, `textContent`, `innerText`, `inputValue`, `getAttribute`, `evaluateOnSelector`, `evalOnSelector`, `evaluateAll`, `evalAll`, `apiRequest`, `waitForRequest`, `waitForRequestFinished`, `waitForRequestFailed`, `routeWebSocket`, `waitForWebSocket`, `waitForWebSocketMessage`, `captureDialogs`, `setDialogBehavior`, `onDialog`, `handleDialog`, `dialogBehavior`, `waitForDialog`, `localStorage`, `sessionStorage`, `cookie`, `newContext`, `useContext`, `closeContext`, `listWorkers`, `workerEvaluate`, `workerIntercept`, `addInitScript`, `evaluateOnNewDocument`, `exposeFunction`, `exposeBinding`, `startCoverage`, `stopCoverage`, `capturePageErrors`, `waitForPageError`, `setGeolocation`, `grantPermissions`, `clearPermissions`, `setJavaScriptEnabled`, `bypassCSP`, `serviceWorkers`, `setExtraHTTPHeaders`, `clearExtraHTTPHeaders`, `setHttpCredentials`, `clearHttpCredentials`, `setProxy`, `clearProxy`, `setOffline`, `readFile`, `fixture`, `writeFile`, `appendFile`, `expectFile`, `expectVisible`, `expectHidden`, `expectEnabled`, `expectDisabled`, `expectAttached`, `expectDetached`, `expectEditable`, `expectEmpty`, `expectFocused`, `expectInViewport`, `expectValues`, `expectClass`, `expectId`, `expectCSS`, `expectProperty`, `expectAccessibleName`, `expectRole`, `uploadFiles`, `setInputFiles`, `selectFile`, `expectScreenshot`, `toHaveScreenshot`, `dragTo`, and `printPdf` are also available in `cmg run`. Non-visual actions do not move the virtual pointer, but their output and failure reasons are included in stdout, reports, and traces.

Control-flow and reuse actions `if`, `elseif`, `else`, `switch`, `case`, `default`, `for`, `repeat`, `while`, `until`, `doWhile`, `doUntil`, `retry`, `foreach`, `foreachSelector`, `break`, `continue`, `try`, `catch`, `finally`, `macro`, `call`, and `return` are available inside tests, hooks, steps, and GIF blocks. They can be nested, and pointer-aware actions inside them use the same virtual pointer behavior as top-level actions.

`contains "text"` checks the page body. `contains "<selector>" "text"`, `containsText`, and `waitForText` check a selector or rich locator and accept `timeout=<milliseconds>`. Successful text checks emit the normal test/step pass output; failed checks include the expected and actual text in the step failure reason.

## Exit Codes

- `0`: All tests passed.
- `1`: At least one test failed, no script files matched, the selected browser is invalid, or the selected browser is not running.

## Examples

```powershell
cmg browser launch
cmg run demo-scripts
cmg run tests\flows --gif artifacts\gifs
cmg run checkout.cmgscript --report-json artifacts\checkout.json --report-html artifacts\checkout.html
cmg run checkout.cmgscript --trace artifacts\traces
cmg run tests\flows --grep checkout --tag smoke --retries 2 --shard 1/3
cmg run tests\flows --max-failures 1
cmg run tests\flows --repeat-each 3
cmg run tests\flows --list --grep checkout
```

Use runner options on test declarations for provider-style focus and skip behavior:

```text
test "checkout" only=true {
  click "#pay"
}

test "legacy flow" skip=true reason="Disabled until the legacy page is removed" {
  click "#legacy"
}
```

When any selected test has `only=true`, `cmg run` runs only focused tests. `skip=true` records `TEST SKIP <name>` and a skipped report entry without running actions.
