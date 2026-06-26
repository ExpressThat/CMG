# CMG Quick Start

CMG is a browser automation CLI for producing choreographed, human-readable, pointer-accurate visual evidence. Start with one of these paths, then move into the reference docs when you need more control.

## How CMG Is Different

Playwright, Cypress, and Puppeteer are excellent automation libraries. CMG is a CLI and DSL built around a different first question: "Can a person or agent understand what happened?"

That is why GIF recording is a first-class workflow. Pointer-aware actions move CMG's virtual pointer, dispatch the browser events a page expects, show hover and drag states, and keep captions, reports, traces, and failure reasons attached to the same run.

Use CMG when the output should be evidence, not just a pass/fail result.

## Path 1: Make A GIF

Use this when you want a visual proof of what happened.

Create `demo.cmgscript`:

```text
navigate "C:\Projects\CMG\index.html"
waitForElement "#openProfileDialog" timeout=5000

step "Open the profile dialog" {
  click "#openProfileDialog"
}

type "#profileName" "CMG quick start"
```

Run it:

```powershell
cmg browser launch
cmg browser control script --file demo.cmgscript --gif demo.gif
```

Output includes a parseable `GIF demo.gif` line. If the script fails after frames have been captured, CMG still writes a partial GIF.

## Path 2: Run A Test

Use this when you want repeatable checks, reports, retries, traces, or per-test GIFs.

Create `profile.test.cmgscript`:

```text
suite "profile dialog" {
  beforeEach {
    navigate "C:\Projects\CMG\index.html"
    waitForElement "#openProfileDialog" timeout=5000
  }

  test "opens" tag=smoke {
    click "#openProfileDialog"
    expectVisible "#profileDialog"
    expectText "#lastDialogAction" "None"
  }
}
```

Run it:

```powershell
cmg run profile.test.cmgscript --gif artifacts\gifs --report-html artifacts\report.html
```

Stdout prints `TEST PASS`, `TEST FAIL`, or `TEST SKIP` lines. Reports and traces include step output and failure reasons.

## Path 3: Use From An Agent

Use this when an AI agent needs direct browser control with parseable feedback.

```powershell
cmg browser launch
cmg browser control validateScript --file demo.cmgscript
cmg browser control script --file demo.cmgscript --trace artifacts\agent.trace.json
```

Agent-friendly behavior:

- Commands use deterministic exit codes.
- Success output is parseable by line.
- Failures include the line, action, and reason.
- Unsupported actions fail explicitly.
- `--trace` captures step names, output lines, and failure details.
- `--gif` creates visual evidence humans can inspect after the agent run.

## What To Read Next

- [Script vs Runner](scripting/script-vs-runner.md): choose the right execution mode.
- [GIF Recording](scripting/gif-recording.md): understand virtual pointer behavior.
- [Action Index](scripting/action-index.md): find an action family quickly.
- [Style Guide](scripting/style-guide.md): write maintainable scripts and tests.
- [Command Reference](commands.md): parseable CLI details.
