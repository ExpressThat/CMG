# `run`

Runs CMG DSL test files.

```powershell
cmg run <path> [options]
```

`<path>` can be one `.cmgscript` file or a directory. Directories are searched recursively for `.cmgscript` files.

Use `--config <file>` to load repeatable runner defaults from a JSON file. CLI options override config values, and `--var` / `--env` override values from the config `variables` object. Relative artifact paths inside the config resolve from the config file's directory. Config files can define named `projects` for cross-browser CI matrices; select one with `--project <name>`.

`cmg run` executes structured `.cmgscript` tests. Top-level browser actions must be wrapped in `test`/`it`/`specify` or `suite`/`describe`/`context` blocks. Direct browser-control scripts run with [`browser control script`](browser/control/script.md). See the [migration guide](../scripting/migration.md) when moving a direct script into the runner.

The runner supports line-level `import "path"` statements. Relative imports resolve from the importing file's directory before parsing. Top-level macros from the file or imported files are registered before each test, and suite-level macros are registered before tests in that suite.

Runner hooks include `beforeAll`, `afterAll`, `beforeEach`, and `afterEach`. Once hooks run for the first or last non-skipped selected test in their file or suite scope, so grep/tag/only/shard selection controls which scopes execute setup and teardown.

Parameterized runner tests use `test.each`, `it.each`, or `specify.each` with `values=`, `each=`, or `json=` data. They expand during planning into normal test cases before grep, tag filtering, `only`, retries, repeats, sharding, reports, traces, and per-test GIF paths are calculated.

Runner declarations can include report annotations with `owner=`, `issue=`, `link=`, `requirement=`, `note=`, or `annotation.<type>=<description>`. Suite annotations cascade to child tests and are written to JSON, HTML, and JUnit reports.

Runner declarations can also include initial variables as `var.<name>=<value>`. Suite variables cascade to child tests, and test-level variables override suite values. Command-line `--var` and `--env` values are injected before declaration variables, so a test declaration can intentionally override a command-provided default.

Relative navigation targets can be resolved with command-line `--base-url` or declaration `baseUrl=` / `baseURL=`. Suite base URLs cascade to child tests, and test-level base URLs override suite and command values.

## Options

- `--gif <directory>` / `-gif <directory>`: Record GIFs for the entire execution of each test.
- `--gif-quality <highest|high|medium|low>`: GIF palette/encoding quality for `--gif`. Defaults to `highest`.
- `--pointer-duration <milliseconds>`: Default virtual pointer movement duration for command-level `--gif` recordings. Must be zero or greater.
- `--pointer-speed <slow|normal|fast|instant|multiplier>`: Default virtual pointer speed for command-level `--gif` recordings. Multipliers use the `1.5x` form. DSL block and action options can still override this.
- `--pointer-easing <linear|ease-in|ease-out|ease-in-out|spring>`: Default virtual pointer easing for command-level `--gif` recordings.
- `--click-pulse <ring|ripple|dot|crosshair|none>`: Default click/tap/drop pulse style for command-level `--gif` recordings. Defaults to `ring`.
- `--gif-hold-after-action <milliseconds>`: Default post-action hold for command-level `--gif` recordings. Defaults to `350`; use `0` to suppress automatic post-action holds.
- `--config <file>`: JSON run config file. CLI options override config values.
- `--project <name>`: Named project from the run config. Project values override global config values, and CLI options override both.
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
- `--timeout <milliseconds>`: Default timeout for timeout-capable waits, event waits, downloads, network waits, worker waits, tab waits, API requests, and assertions that do not set `timeout=`.
- `--navigation-timeout <milliseconds>`: Default timeout for navigation actions and navigation waits.
- `--assertion-timeout <milliseconds>`: Default timeout for assertions. Overrides `--timeout` for assertion actions.
- `--base-url <url>`: Absolute base URL used to resolve relative `navigate`, `goto`, `visit`, `openTab`, and `newContext url=` targets in every selected test.
- `--browser-port <port>`: Remote debugging port for the browser instance used by this run. Use this with browsers launched through `cmg browser --port <port> launch`.
- `--auto-launch`: Launch the selected browser automatically when no CMG-controlled browser is running. The launch uses the selected browser and `--browser-port` value with the same defaults as `cmg browser launch`.
- `--headless`: Launch the selected browser in headless mode when `--auto-launch` starts a browser. Chrome and Edge receive `--headless=new`; Firefox receives `--headless`.
- `--var <name=value>`: Initial variable for every selected test. Can be repeated. Later entries with the same name replace earlier entries.
- `--env <name=value>`: Alias for `--var`, intended for CI and agent-provided environment values.
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

Failures may include action output before the failing test line. Declaration-skipped tests do not run actions or produce GIFs. Runtime `skip "reason"` stops the current test, preserves output and GIF frames captured before the skip, and records `TEST SKIP <name>`. `RUN STOP maxFailures=<count>` means `--max-failures` stopped the run after the threshold was reached. `TEST LIST` lines are emitted by `--list` and show the selected schedule without browser execution. Stderr contains the final error when one is available.
Parameterized tests print and report their expanded names, for example `TEST LIST run opens profile`. Project runs include the project name in brackets, for example `TEST LIST run [firefox-smoke] checkout`.

When a step fails, stderr also includes:

```text
STEP FAIL line=<line> action=<action> reason=<reason>
```

When a file cannot be parsed, imported, or planned into a runnable test, stdout still prints `TEST FAIL <file>` and stderr includes:

```text
TEST ERROR <file> reason=<reason>
```

Invalid run config files fail before browser connection or test listing. Stderr names the config problem, for example `Run config option 'retries' must be an integer.` or `Run config '<path>' was not valid JSON. ...`.

If no selected CMG browser is running, stderr tells the caller which launch command to run, for example `No CMG-controlled Chrome instance is running. Run 'cmg browser launch' first.` Use `--auto-launch` when the runner should start the selected browser automatically, and add `--headless` when that auto-launched browser should not open a visible window.

Reports and traces include per-test status, output, and per-step diagnostics so agents can explain why a run failed. JSON and HTML `steps` contain public executed runtime steps only; planned placeholders and generated internal evaluate/actionability/locator steps are omitted. Public report step sequences are contiguous per test, and output payload lines are renumbered to match their parent step. JSON step entries include separate `sequence`, `lineNumber`, `context`, and `action` fields; agents should use those fields instead of parsing human stdout strings. HTML reports render the same source-aware step order as a table. Traces keep lower-level raw diagnostics. JUnit reports emit `<skipped>` nodes for declaration-skipped tests and runtime skips.
Report annotations are emitted as `annotations` in JSON, visible list items in HTML, and JUnit `<property name="cmg.annotation.<type>" ... />` entries.

## GIF Behavior

GIF recording is optional.

- With `--gif` or `-gif`, CMG records the whole execution of each test.
- `--gif-quality` defaults to `highest`, using CMG's most color-faithful palette matching and dithering. Use `high`, `medium`, or `low` to trade color fidelity for smaller/faster GIF artifacts.
- `--pointer-duration`, `--pointer-speed`, and `--pointer-easing` set whole-test virtual pointer defaults when `--gif` is active.
- `--click-pulse` sets the whole-test click/tap/drop pulse style when `--gif` is active.
- `--gif-hold-after-action` sets the whole-test post-action hold duration when `--gif` is active.
- When command-level GIF recording is active, script-level `gif { ... }`, `recordVideo { ... }`, and `screencast { ... }` blocks do not create nested recordings; their actions are flattened into the whole-test GIF.
- Without command-level GIF recording, script-level `gif "name" { ... }`, `recordVideo "name" { ... }`, or `screencast "name" { ... }` records only the wrapped block.
- Without command-level GIF recording or an active script-level recording block, CMG does not inject the virtual pointer. Recording-only actions such as `pauseGif` and `moveMouse` are skipped and do not create pointer frames.
- If `--max-failures` stops the run, GIFs and reports include only tests that actually ran before the stop.
- With `--repeat-each`, each repeat is a separate scheduled test with a distinct name such as `checkout [repeat 2/3]`, so per-test GIFs, traces, reports, retries, and sharding remain deterministic.

All recorded actions use CMG's virtual pointer, pointer/mouse event dispatch, captions, and drag ghost behavior. Selector actions accept CMG rich locators and provider-style aliases. Every non-CSS locator resolves to the same temporary element marker used by the GIF recorder, so virtual pointer movement, pointer events, drag ghosts, and captions remain aligned with the chosen element.

Actions, locators, control flow, loops, macros, scoped variables, and `gif` blocks are shared with direct browser-control scripts unless a reference page says otherwise. Start with the [action index](../scripting/action-index.md), then use the [detailed action reference](../scripting/actions.md) for options and examples.

`contains "text"` and `notContains "text"` check the page body. `contains "<selector>" "text"`, `containsText`, `waitForText`, `notContainsText`, and the negative text aliases check a selector or rich locator and accept `timeout=<milliseconds>`, `match=contains|exact|regex`, and `ignoreCase=true`. Successful text checks emit the normal test/step pass output; failed checks include the expected and actual text in the step failure reason.

## Exit Codes

- `0`: All tests passed.
- `1`: At least one test failed, no script files matched, the selected browser is invalid, `--browser-port` is outside `1..65535`, the selected `--project` is missing or invalid, the selected browser is not running, or `--auto-launch` could not start it.

## Examples

```powershell
cmg browser launch
cmg run demo-scripts\20-runner-flow.cmgscript
cmg run tests\flows --gif artifacts\gifs
cmg run tests\flows --gif artifacts\gifs --gif-quality highest
cmg run tests\flows --gif artifacts\gifs --pointer-duration 600 --pointer-easing spring
cmg run tests\flows --gif artifacts\gifs --click-pulse ripple --gif-hold-after-action 700
cmg run checkout.cmgscript --report-json artifacts\checkout.json --report-html artifacts\checkout.html
cmg run checkout.cmgscript --trace artifacts\traces
cmg run tests\flows --grep checkout --tag smoke --retries 2 --shard 1/3
cmg run tests\flows --max-failures 1
cmg run tests\flows --repeat-each 3
cmg run tests\flows --list --grep checkout
cmg run tests\flows --timeout 10000 --navigation-timeout 15000 --assertion-timeout 5000
cmg run tests\flows --var user=Ada --env mode=demo
cmg run tests\flows --base-url https://example.test/app/
cmg browser --port 9333 launch --headless
cmg run tests\flows --browser-port 9333
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --list
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --project chrome-smoke
cmg run demo-scripts\147-run-config.cmgscript --config demo-scripts\run-config.example.json --project firefox-smoke
cmg run tests\flows --auto-launch
cmg run tests\flows --auto-launch --headless
```

Example config:

```json
{
  "gif": "../demo-output/runner-gifs",
  "trace": "../demo-output/traces",
  "reportJson": "../demo-output/run-report.json",
  "reportHtml": "../demo-output/run-report.html",
  "grep": "config",
  "tag": "smoke",
  "retries": 1,
  "maxFailures": 2,
  "repeatEach": 1,
  "shard": "1/1",
  "timeout": 10000,
  "navigationTimeout": 15000,
  "assertionTimeout": 5000,
  "baseUrl": "https://example.test/app/",
  "projects": [
    {
      "name": "chrome-smoke",
      "browser": "chrome",
      "baseUrl": "https://chrome.example.test/app/",
      "tag": "smoke",
      "variables": {
        "browserName": "chrome"
      }
    },
    {
      "name": "firefox-smoke",
      "browser": "firefox",
      "baseUrl": "https://firefox.example.test/app/",
      "tag": "smoke",
      "variables": {
        "browserName": "firefox"
      }
    }
  ],
  "variables": {
    "tenant": "demo",
    "mode": "config",
    "browserName": "default"
  }
}
```

Use runner options on test declarations for provider-style focus and skip behavior:

```text
test "checkout" only=true {
  click "#pay"
}

test "legacy flow" skip=true reason="Disabled until the legacy page is removed" {
  click "#legacy"
}

test.only "debug checkout" {
  click "#pay"
}

test.fixme "broken checkout"
test.todo "add refund coverage"

test.slow "slow checkout" {
  expectText "#status" "Saved"
}

describe.skip "legacy area" {
  it "old case" {
    click "#old"
  }
}

describe.slow "slow area" {
  it "inherits slow timeout policy" {
    waitForSelector "#eventual"
  }
}

test.each "opens ${page}" as=page values="profile,checkout" tag=smoke {
  click "#${page}"
}

test.each "opens ${case.name}" as=case json="[{\"name\":\"Profile\",\"selector\":\"#profile\"}]" {
  click "${case.selector}"
}

test "annotated checkout" owner=qa issue="BUG-7" annotation.requirement="REQ-1" {
  click "#checkout"
}

describe "tenant flow" var.tenant=demo {
  test "uses suite variables" {
    expect (${tenant} == "demo")
  }

  test "overrides suite variables" var.tenant=staging {
    expect (${tenant} == "staging")
  }
}

describe "relative navigation" baseUrl="https://example.test/app/" {
  test "opens profile" {
    navigate "profile"
  }
}
```

When any selected test has `only=true` or a `.only` declaration, `cmg run` runs only focused tests. `skip=true`, `.skip`, `.fixme`, and `.todo` record `TEST SKIP <name>` and a skipped report entry without running actions. Suite-level focus and skip declarations cascade to child tests. For script structure and style guidance, see [syntax](../scripting/syntax.md) and the [style guide](../scripting/style-guide.md).
`slow=true` or `.slow` scales inherited default wait, navigation, and assertion timeouts for that test by `3x`; `slow=<number>` uses a custom multiplier. Explicit action-level `timeout=` still wins.
