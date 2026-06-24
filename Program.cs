using CMG;
using Microsoft.Extensions.DependencyInjection;

namespace CMG;

public static class Program
{
    public static int Main(string[] args)
    {
        using var services = new ServiceCollection()
            .AddCmgCli()
            .BuildServiceProvider();

        return services
            .GetRequiredService<CliApplication>()
            .Run(args);
    }
}
