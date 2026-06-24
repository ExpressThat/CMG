using CMG.Commands;

namespace CMG;

public sealed class CliApplication
{
    private readonly ICommandTreeBuilder commandTreeBuilder;

    public CliApplication(ICommandTreeBuilder commandTreeBuilder)
    {
        this.commandTreeBuilder = commandTreeBuilder;
    }

    public int Run(string[] args)
    {
        var rootCommand = commandTreeBuilder.Build();

        return rootCommand.Parse(args).Invoke();
    }
}
