# CMG Docs

CMG is a browser automation CLI for producing choreographed, human-readable, pointer-accurate visual evidence.

Start here:

1. [Top-level README](../README.md) for the product overview and first GIF/test walkthroughs.
2. [Quick Start](quick-start.md) for the three fastest paths: make a GIF, run a test, or use CMG from an agent.
3. [Script vs Runner](scripting/script-vs-runner.md) to choose direct browser-control scripts or `cmg run`.
4. [Scripting guide](scripting/index.md) for `.cmgscript` syntax, actions, GIF recording, errors, and style.
5. [Command reference](commands.md) for parseable CLI behavior.
6. [E2E testing](e2e-testing.md) for real headless browser coverage of commands, scripts, artifacts, and failures.
7. [Demo scripts](../demo-scripts/README.md) for runnable examples.

## Reference Structure

- `commands.md`: top-level command reference index.
- `commands/<group>/index.md`: command group documentation.
- `commands/<group>/<command>.md`: leaf command documentation.
- `scripting/action-index.md`: compact table of script action groups.
- `scripting/script-vs-runner.md`: comparison of direct browser-control scripts and structured runner tests.
- `scripting/examples.md`: guided learning examples for the first GIF, first test, visual evidence, variables, macros, and failures.
- `scripting/cookbook-reference.md`: full advanced cookbook for specific automation patterns.
- `scripting/actions.md`: detailed action reference.
- `scripting/style-guide.md`: conventions for maintainable CMG scripts and tests.
- `e2e-testing.md`: how to run and extend real CLI/browser E2E coverage.

Nested command groups use nested folders.
