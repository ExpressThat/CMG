using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CMG;

public static class DependencyInjection
{
    public static IServiceCollection AddCmgCli(this IServiceCollection services)
    {
        services.AddSingleton<CliApplication>();
        services.AddSingleton<ICommandTreeBuilder, CommandTreeBuilder>();
        services.AddSingleton<BrowserCommandBuilder>();
        services.AddSingleton<BrowserControlCommandBuilder>();
        services.AddSingleton<IBrowserCommandHandler, BrowserCommandHandler>();
        services.AddSingleton<IBrowserControlCommandHandler, BrowserControlCommandHandler>();
        services.AddSingleton<IBrowserController, BrowserController>();
        services.AddSingleton<IBrowserControlService, BrowserControlService>();
        services.AddSingleton<ChromeDevToolsClient>();
        services.AddSingleton<BrowserScriptParser>();
        services.AddSingleton<BrowserScriptRunner>();
        services.AddSingleton<BrowserStateStore>();

        return services;
    }
}
