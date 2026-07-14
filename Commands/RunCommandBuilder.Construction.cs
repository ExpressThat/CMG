using CMG.Runner;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private readonly ICmgRunCommandHandler handler;

    public RunCommandBuilder(ICmgRunCommandHandler handler) => this.handler = handler;
}
