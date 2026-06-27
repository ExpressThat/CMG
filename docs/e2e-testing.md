# E2E Testing

CMG has a real browser-backed E2E test project at `tests/CMG.E2E.Tests`.

The E2E project runs the CLI as an external process with:

```powershell
dotnet test tests\CMG.E2E.Tests\CMG.E2E.Tests.csproj /p:UseSharedCompilation=false
```

For repeat local runs after a successful build, use:

```powershell
dotnet test tests\CMG.E2E.Tests\CMG.E2E.Tests.csproj --no-build
```

## What It Does

- Launches the built CMG app as an external process.
- Builds CMG once through the E2E test project's `ProjectReference`; individual E2E CLI calls reuse the built apphost (`CMG.exe` when available) rather than invoking `dotnet run`.
- Starts Chrome headless with `cmg browser --port <free-port> launch --headless`.
- Uses an isolated temporary `LOCALAPPDATA` directory for the shared browser E2E fixture so tests do not touch a developer's normal CMG browser state.
- Starts a tiny local static HTTP fixture server for the shared browser fixture for origin-sensitive behavior such as cookies, network, workers, and future request tests.
- Reuses the same C# fixture server for API command tests through `/api/echo`, `/api/status/<code>`, and `/api/slow`.
- Drives the browser through real CLI commands, direct `.cmgscript` files, and `cmg run` test files.
- Writes and verifies real screenshots, GIFs, traces, JSON reports, HTML reports, and JUnit reports.
- Uses `tests/CMG.E2E.Tests/Fixtures/index.html` as the main page fixture.

## Fixture Page

The fixture page is intentionally broad. It includes:

- Form controls, checkboxes, radios, selects, textareas, and file inputs.
- Buttons, text targets, hidden/visible states, editable content, classes, styles, and accessibility labels.
- Hover targets, drag/drop targets, scroll containers, canvas, iframes, dialogs, local storage, fetch, and workers.

Add page affordances here when a new command or script action needs real browser state.

## Coverage Direction

The project is intended to grow until every command, every script action, and every meaningful failure path has E2E coverage. Unit tests still cover parser/lowering edge cases, but command/action behavior should graduate into E2E coverage when it depends on a browser, artifacts, process state, reports, GIFs, or real CLI output.

Command behavior ownership is tracked in [`docs/e2e-command-coverage.md`](e2e-command-coverage.md). Script action behavior ownership is tracked in [`docs/e2e-script-action-coverage.md`](e2e-script-action-coverage.md). The E2E suite enforces that every documented command and every backticked action heading is mapped to at least one owning E2E file, and that each row declares valid coverage dimensions such as success, failure, artifact, GIF, trace, report, network, pointer, and state coverage. This keeps the remaining coverage work visible as the CLI and DSL grow.

Keep E2E tests explicit and scenario-shaped:

- One C# test file per command or feature area.
- C# files must stay under 250 lines. `E2eFileSizeGuardTests` enforces this limit.
- Large `.cmgscript` scenarios may exceed 250 lines when needed, but keep one scenario per file.
- If a script is correct and an E2E test exposes an unexpected failure, fix the CMG root cause rather than forcing the script around the bug.
- Prefer real headless browser checks over mocks.
- Verify parseable stdout, stderr, exit codes, and generated artifacts.

## Speed Notes

- `dotnet test` builds CMG once through the E2E project reference. The fixture then reuses the built apphost for individual CLI calls.
- `--no-build` is the fastest local loop when code has already been built.
- Browser E2E test classes use `IClassFixture<CmgBrowserFixture>`. Each browser-backed class owns its own Chrome process, remote debugging port, browser profile, `LOCALAPPDATA` root, output directory, and fixture server.
- The E2E assembly allows up to eight xUnit workers with `MaxParallelThreads = 8`. xUnit schedules classes automatically; at most eight browser fixtures should be active at once, and each fixture cleans up its own browser process and temporary workspace.
- `E2eParallelismGuardTests` enforces this model so shared browser collections and accidental serial assembly settings cannot be reintroduced silently.
- Fixture CLI calls automatically scope `browser` commands with `browser --port <fixture-port>` and `run` commands with `--browser-port <fixture-port>`.
- The fixture closes the selected browser port at disposal and falls back to killing any process ids left in CMG state files before deleting its workspace.
- `BrowserFixtureIsolationE2eTests` verifies the fixture can launch, drive, close, and remove a browser-backed temporary workspace.
- Non-browser E2E classes, such as help coverage and local file commands, use a lightweight CLI fixture and can also run within the eight-worker cap.
- Keep independent browser scenarios in separate E2E classes when they do not need to share browser state. Smaller feature-shaped classes make focused local runs faster and easier to reason about.

## Current Seed Coverage

The first E2E slice covers:

- Browser lifecycle launch/close behavior, custom-port validation, no-running close output, and ignored close arguments.
- App attach validation failures, successful attach to a real debugging endpoint, app launch validation failures, missing-executable output, and missing-debug-endpoint output.
- Every documented command page under `docs/commands/`, including nested command groups and leaf commands, exposes working external `--help` output without creating browser state or requiring a launched browser.
- A broad browser action-surface script covering navigation, input, assertions, dialogs, frames, storage, emulation, clocks, looping, branching, try/catch, and trace output.
- Artifact and state actions for visual assertions, screenshots, PDFs, coverage, storage state, local file actions, and tab control.
- Browser-control CLI behavior for waits, runtime getters, input, assertions, storage, cookies, context/emulation, isolated browser contexts, clock, accessibility, frame actions/getters/failure reasons, tabs, and capture artifacts.
- Direct script and runner isolated browser-context actions for context creation, id capture, listing, switching, closing, storage isolation, trace output, and missing-context diagnostics.
- Runner frame actions for frame click/fill/type/hover, waits, text assertions, evaluation, getters, trace output, missing frame-wait diagnostics, and exact failure-step diagnostics.
- Browser-control clock/accessibility aliases for `restoreClock`, `accessibilitySnapshot`, `expectAccessible`, and accessibility failure reasons.
- Direct script and runner clock/accessibility actions for fake time, clock restore, accessibility snapshots, accessible-role checks, trace output, and validation failure diagnostics.
- Browser-control context aliases for JavaScript toggling, service-worker mode, context clear, and context reset.
- Direct script and runner environment actions for locale, timezone, media, geolocation, permission grants, permission clearing, trace output, and validation failure diagnostics.
- Direct script and runner context policy actions for JavaScript blocking, CSP bypass, service-worker blocking, context clearing/reset, trace output, and validation failure diagnostics.
- Browser-control storage lifecycle commands for local/session storage remove/clear, cookie attributes/remove/clear, storage-state save/load, and validation failures.
- Direct script and runner storage actions for local/session storage, cookies, storage-state save/load, trace output, and validation failure diagnostics.
- Browser-control wait command aliases for selector states, function waits, fixed waits, auto waits, and selector-state failure reasons.
- Browser-control navigation/runtime aliases for `expectUrl`, `waitForNavigation`, `waitForNetworkIdle`, `evalOnSelector`, and `evalAll`.
- Runner navigation and wait actions for navigation aliases, history, reload, URL/title/load/network waits, selector/function/fixed waits, auto wait aliases, persistent and scoped timeout defaults, trace output, and exact failure-step diagnostics.
- Browser-control provider-style assertion aliases for text absence, state assertions, values, attributes, class/id/CSS/property, accessibility name, role, checked state, and counts.
- Browser-control assertion alias coverage for direct CLI text, body text, state, eval, value, attribute, accessibility, checked, and count variants.
- Direct script assertion aliases for state, value, multi-value, attribute, class/id/CSS/property, accessibility name, role, checked, count, text presence/absence, eval assertions, and failure diagnostics.
- Runner assertion aliases for state, value, multi-value, attribute, class/id/CSS/property, accessibility name, role, checked, count, text presence/absence, trace output, and exact failure-step diagnostics.
- Runtime setup actions for init scripts, exposed functions/bindings, script/style tag injection, generated page content, HTML reads, and bounding boxes.
- Runner runtime setup actions for init scripts, exposed functions/bindings, script/style tags, generated content, element HTML, bounding boxes, trace output, and failure diagnostics.
- Browser-control network commands for routes, HAR export/replay, network waits, mocked failures, headers, and offline mode.
- Direct script and runner network actions for routes, HAR export/replay, request/response waits, mocked failures, trace output, and failure diagnostics.
- Browser-control network environment aliases for extra headers, HTTP credentials, proxy rewrites, mocked responses, and validation failures.
- Direct script and runner network environment actions for extra headers, HTTP credentials, proxy rewrites, offline mode, trace output, and validation failure diagnostics.
- Browser-control WebSocket commands for route installation, socket waits, message waits, generic event waits, and route clearing against a real WebSocket handshake.
- Direct script and runner WebSocket actions for route installation, socket waits, message waits, generic `waitForEvent` aliases, route clearing, trace output, and validation failure output.
- Browser-control event commands for console messages, page errors, generic event waits, dialog capture, dialog behavior, and absence assertions.
- Browser-control event aliases for console capture/waits, page-error capture/waits, generic `waitForEvent`, and dialog `onDialog`/`handleDialog`.
- Direct script and runner event actions for console capture/waits, page-error capture/waits, generic `waitForEvent` including download waits, dialog handling, absence assertions, trace output, and matcher validation failure diagnostics.
- Browser-control coverage commands for start/stop aliases, file-backed coverage output, stdout JSON output, and validation failures.
- Runner coverage actions for file-backed coverage output and per-step coverage failure diagnostics.
- Browser-control tab alias commands for listing, opening, waiting, popup waits, activation, closing, and tab-count failure reasons.
- Browser-control capture commands for element screenshots, direct element screenshot output, page clips, temporary styles, masks, visual assertion aliases, first-run baseline creation failures, PDF options, and validation failures.
- Runner artifact, page, file, and tab actions for screenshots, visual checks, PDFs, file round trips, page getters/content replacement, tab waits, trace output, and failure diagnostics.
- Browser-control advanced input commands for clipboard shims, tap/touch tap, mouse movement/buttons, scroll/wheel, custom events, file upload aliases, and download wait success/failure behavior.
- Direct script input actions for clipboard shims, multi-file uploads, upload aliases, block drag/drop, download waits, and missing upload-file failure output.
- Runner advanced input actions for clipboard shims, tap/touch tap, pointer/keyboard/form aliases, simple and block drag/drop, scroll/wheel, upload aliases, download waits, trace output, and failure diagnostics.
- Browser-control input aliases for double/right click, sequential typing, selection, blur, key down/up, hotkeys, select options, drag aliases, low-level mouse aliases, scroll aliases, and select-file upload.
- Browser-control worker commands for real worker listing, waiting, evaluation, interception, and missing-worker wait failure output.
- Direct script and runner worker actions for real worker listing, waits, evaluation, interception, and missing-worker failure output.
- Local `files` command success and failure behavior.
- API request command behavior for query parameters, headers, JSON/form/raw bodies, basic auth, `--ok`, status matching, output files, response-header assertions, validation failures, and timeout failures.
- Script and runner `apiRequest` action behavior for query parameters, headers, status matching, output files, JSON bodies, and step failure diagnostics.
- Navigation aliases and page state commands, including `goto`, `visit`, history back/forward, reload, URL/title waits and assertions, page content reads, `setContent`, runtime text reads, input, assertions, screenshots, element HTML, and failure reasons.
- Page utility aliases for viewport sizing, captions/message bars, temporary highlights, and explicit delays.
- Direct script execution from files and stdin with GIF, trace output, command-line `--base-url`, timeout defaults, `--var`/`--env` variables, plus stdin script validation that does not require a launched browser.
- Direct script and runner import expansion for validation, imported macros, returned values, and missing-import failure output.
- GIF block actions for direct `gif`, provider-style `recordVideo`/`screencast` aliases, and command-level GIF suppression in direct scripts and `cmg run`.
- Script trace actions for partial trace files, command-level trace suppression, command-level failure traces, and failure-time partial trace output.
- Runner trace actions for partial trace files and failure-time partial trace output inside structured tests.
- Dialog handling, variables, macros, logic, `set` capture, and block `return`.
- Direct script and runner macro scoping for nested macros, parent-scope lookup, local variable isolation, `set { call ... }`, block `return`, trace output, and macro failure diagnostics.
- Direct script `skip` and soft assertion continuation/final-failure behavior.
- Runner runtime `skip` and soft assertion continuation/final-failure behavior across stdout, traces, and JSON reports.
- Scoped locator behavior for rich locator filters, shadow DOM locators, `within`, and scoped `foreachSelector` in direct scripts and `cmg run`.
- Advanced script control flow for `step` blocks, `for`, `foreach`, `foreachJson`, `foreachSelector`, `while`, `until`, `doWhile`, `doUntil`, `break`, `continue`, `retry`, `toPass`, scoped timeout blocks, trace output, and child-failure diagnostics.
- Runner control flow for `step` blocks, nested loops, selector/JSON iteration, `switch`, `try`/`catch`/`finally`, `retry`, `toPass`, scoped timeouts, trace output, and child-failure diagnostics.
- Structured `cmg run` with reports, traces, optional GIFs, parsed JSON/HTML/JUnit report contents, per-test trace contents, tags, `--var`/`--env` variables, command-line base URLs, list mode, hooks, parameterized tests, runtime skips, soft assertions, retries, and max-failure stopping.
- Runner config/project execution for relative artifact paths, project names, project variables, CLI variable overrides, repeat scheduling, sharding, and missing-project failure behavior.
- Runner pre-browser validation failures for invalid config JSON, invalid config field types, invalid shard values, invalid browser ports, unmatched script paths, direct-script migration guidance, syntax errors, and missing imports.
- Runner provider declarations for `.only`, `.skip`, `.fixme`, `.todo`, suite skip inheritance, and report annotations across JSON, HTML, and JUnit artifacts.

Worker evaluation and interception are covered against real headless Chrome workers created from the fixture server after CMG initializes worker support.
