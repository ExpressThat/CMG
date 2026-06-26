using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Commands;
using CMG.Runner;
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
        services.AddSingleton<RunCommandBuilder>();
        services.AddSingleton<ApiCommandBuilder>();
        services.AddSingleton<FilesCommandBuilder>();
        services.AddSingleton<IBrowserCommandHandler, BrowserCommandHandler>();
        services.AddSingleton<IBrowserControlCommandHandler, BrowserControlCommandHandler>();
        services.AddSingleton<ICmgRunCommandHandler, CmgRunCommandHandler>();
        services.AddSingleton<ICmgRunService, CmgRunService>();
        services.AddSingleton<CmgDslParser>();
        services.AddSingleton<CmgTestPlanner>();
        services.AddSingleton<CmgActionLowerer>();
        services.AddSingleton<CmgValidator>();
        services.AddSingleton<CmgApiRequestRunner>();
        services.AddSingleton<CmgStorageStateRunner>();
        services.AddSingleton<CmgVisualAssertionRunner>();
        services.AddSingleton<CmgUploadRunner>();
        services.AddSingleton<IBrowserController, BrowserController>();
        services.AddSingleton<IBrowserControlService, BrowserControlService>();
        services.AddSingleton<BrowserAutomationClientFactory>();
        services.AddSingleton<ChromeDevToolsClient>();
        services.AddSingleton<FirefoxBiDiClient>();
        services.AddSingleton<BrowserScriptParser>();
        services.AddSingleton<BrowserScriptValidator>();
        services.AddSingleton<BrowserScriptRunner>();
        services.AddSingleton<BrowserStateStore>();

        return services;
    }
}
