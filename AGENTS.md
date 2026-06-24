# Agent Instructions

This repository contains a CLI intended to be called by AI agents.

## Documentation Requirement

Whenever you add, remove, rename, or change a CLI command, option, argument, output shape, exit code behavior, or required workflow, update the documentation in `docs/` in the same change.

At minimum:

- Update `docs/commands.md` if command groups are added, removed, or renamed.
- Update the relevant command group index under `docs/commands/`.
- Update or create one Markdown file per leaf command.
- Document every argument and option.
- Document stdout/stderr behavior that an agent caller would need to parse.
- Document meaningful exit codes.
- Add or update examples.

Documentation structure:

- `docs/commands.md`: top-level command reference index.
- `docs/commands/<group>/index.md`: command group documentation.
- `docs/commands/<group>/<command>.md`: leaf command documentation.
- For nested groups, add nested folders, for example `docs/commands/browser/control/index.md`.
- Scripting language changes must also update `docs/scripting/`, including syntax, actions, examples, and errors.
- When scripting examples change, keep `demo-scripts/` and `docs/scripting/examples.md` in sync.
- GIF recording changes must update `docs/scripting/gif-recording.md` and the `browser control script` command docs.

If a command is only scaffolded and does not perform real behavior yet, document that clearly.

## CLI Design Notes

- Commands are built with `System.CommandLine`.
- Command groups can be deeply nested.
- Prefer adding new command groups through dedicated command builder classes under `Commands/`.
- Keep behavior behind interfaces and services under the relevant domain folder, such as `Browser/`.
- Keep `Program.cs` thin. It should only build dependency injection and run the application.

## Verification

After changing commands, run:

```powershell
dotnet build /p:UseSharedCompilation=false
```

Also run the relevant `--help` command and at least one success/failure path for changed commands.
