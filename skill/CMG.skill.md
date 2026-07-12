---
name: cmg
description: Use CMG to control real Chrome, Edge, or Firefox browsers; run deterministic direct scripts or structured browser tests; diagnose failures; and produce screenshots, reports, traces, and pointer-accurate GIF evidence. Trigger for browser automation, AI browser control, repeatable UI journeys, visual bug reproduction, and test execution through the CMG CLI or DSL.
---

# CMG

CMG is a browser automation CLI built for AI agents. Use it for one-off browser actions, multi-step deterministic journeys, structured tests, and choreographed visual evidence.

## Why It Is Token-Efficient

CMG's execution model is token-efficient: send one compact `.cmgscript` and receive concise numbered results instead of repeatedly loading a large DOM or accessibility snapshot between individual actions. CMG executes already-decided steps locally without another model inference for each action.

This claim applies to execution, not to loading every CMG reference into context. This file is intentionally a short router. Do not load the entire `references/` tree. Open only the linked leaf document needed for the current task.

```text
click "getByRole=button|Clone"
fill "getByLabel=Repository URL" "https://github.com/example/repo.git"
click "getByRole=button|Start"
waitForText "#status" "Complete"
screenshotPage output="result.png"
```

Run the journey once:

```powershell
cmg browser control script --file journey.cmgscript
```

## Quick Contract

1. Start or select a browser.
   - Chrome: `cmg browser launch`
   - Edge: `cmg --edge browser launch`
   - Firefox: `cmg --firefox browser launch`
   - Multiple instances: place `--port` after `browser`, for example `cmg browser --port 9333 launch`.
2. Choose the smallest suitable surface.
   - One action: `cmg browser control <group> <command> ...`
   - Multi-step browser control: `cmg browser control script --file flow.cmgscript`
   - Generated script through stdin: `cmg browser control script --file -`
   - Structured tests, retries, reports, traces, sharding, or per-test GIFs: `cmg run <path>`
3. Parse results predictably.
   - Exit code `0`: success or an intentional skip.
   - Exit code `1`: validation, browser, action, assertion, or runner failure.
   - Stdout contains numbered `PASS`, action payload, artifact, and test-result lines.
   - Stderr explains failures with the action, source line, context, and reason.
4. Close browsers started for the task: `cmg browser close`.

Prefer file-backed scripts or `--file -` in PowerShell. Shell parsing makes nested quotes in `--inline` less reliable.

## Choose A Workflow

| Need | Command | Read only when needed |
| --- | --- | --- |
| First browser journey | `cmg browser control script --file flow.cmgscript` | [Quick start](references/docs/quick-start.md) |
| Exact CLI command or option | `cmg <group> <command> --help` | [Command index](references/docs/commands.md) |
| Script syntax, variables, logic, macros, loops | `.cmgscript` | [Syntax](references/docs/scripting/syntax.md) |
| Find an action family | Script or runner DSL | [Action index](references/docs/scripting/action-index.md) |
| Look up an exact DSL command | Script or runner DSL | [One file per command](references/docs/scripting/commands/index.md) |
| Read shared action behavior | Script or runner DSL | [Action topic leaves](references/docs/scripting/action-topics/index.md) |
| GIF recording and virtual pointer | `--gif` or `gif { ... }` | [GIF recording](references/docs/scripting/gif-recording.md) |
| Structured tests and reports | `cmg run <path>` | [Run command](references/docs/commands/run.md) |
| Failure diagnosis | stderr, report, or trace | [Errors](references/docs/scripting/errors.md) |
| Maintainable scripts | `.cmgscript` | [Style guide](references/docs/scripting/style-guide.md) |
| Runnable patterns | Demo files | [Demo index](references/demo-scripts/README.md) |
| Product overview and positioning | None | [Product overview](references/README.md) |
| Browse every packaged reference | None | [Reference tree](references/index.md) |

For an unfamiliar command or option, search before opening a large reference: `rg -n "<command>|<option>" references/docs`. Then open only the matching leaf page.

## Scripts And Tests

Use direct scripts for agent-controlled browsing, investigation, demos, and focused journeys. Use `cmg run` for suites, hooks, filtering, retries, parallel workers, browser projects, reports, traces, sharding, and per-test artifacts.

Both script types share actions, rich locators, variables, logic, macros, loops, scoped recording defaults, GIF blocks, virtual pointer behavior, and failure diagnostics.

```text
navigate "https://example.test"
waitForElement "getByRole=button|Continue"
click "getByRole=button|Continue"
expectVisible "#complete"
```

## Visual Evidence

- Whole journey: `cmg browser control script --file flow.cmgscript --gif artifacts/flow.gif`
- Focused section: `gif "checkout" { ... }`
- Test GIFs: `cmg run tests --gif artifacts/gifs`
- Whole-run `--gif` suppresses nested GIF files while retaining their actions in the whole recording.
- Recording-only actions skip when no recording is active.
- GIF movement uses CMG's live virtual pointer, browser pointer events, hover states, click pulses, drag ghosts, and captions.
- A GIF click pulse is visual-only; it does not activate the target a second time.

## Reliable Agent Usage

- Prefer stable locators such as `getByRole=`, `getByLabel=`, and `getByTestId=`.
- Quote rich locators containing spaces: `"getByLabel=Repository URL"`.
- Use `waitForElement` or a state assertion for dynamically mounted UI.
- `fill` and `type` support controlled inputs and reacquire rich-locator targets replaced during a framework render.
- User-like actions do not auto-scroll. Add `scrollIntoView` when the target is outside the viewport.
- Write binary artifacts with explicit `output=` paths to avoid base64 payloads on stdout.
- Use `listPageErrors` and `listConsole level=error` after risky actions. Diagnostics are armed at launch or attach.
- Browser dialogs are explicit. Configure `captureDialogs`, `setDialogBehavior`, `onDialog`, or `handleDialog` before the action that opens one; CMG does not silently remove or accept dialogs.
- For authentication, let the user sign in manually unless they explicitly request credential automation and provide a safe method.

## Reports And Artifacts

```powershell
cmg run tests `
  --gif artifacts/gifs `
  --report-json artifacts/report.json `
  --report-html artifacts/report.html `
  --trace artifacts/traces
```

For automation, prefer structured JSON report fields over parsing display text. Keep screenshots, GIFs, reports, and traces under an explicit workspace artifact directory.

## Reference Loading Rule

Start with this file and command `--help`. Open one reference leaf at a time. Follow the [reference tree](references/index.md) to reach every packaged Markdown file without scanning the directory. Use the generated action-topic leaves for scripting behavior; aliases that share semantics intentionally share one family page to avoid duplicated guidance and drift. Detailed CLI command pages remain one file per leaf command under `references/docs/commands/`, and runnable examples live under `references/demo-scripts/`.
