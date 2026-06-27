using System.CommandLine;
using System.Net;
using CMG.Commands;
using CMG.Runner;

namespace CMG.Tests;

public sealed class ApiCommandBuilderTests
{
    [Fact]
    public void RequestCommand_MapsAdvancedOptionsToRunner()
    {
        HttpRequestMessage? request = null;
        var runner = new CmgApiRequestRunner(new HttpClient(new Handler(message =>
        {
            request = message;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok"), RequestMessage = message };
        })));
        var root = new RootCommand();
        root.Subcommands.Add(new ApiCommandBuilder(runner).Build());

        var exitCode = root.Parse("api request POST https://example.test --form user=agent --auth user:pass --query preview=true --ok").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal("POST", request?.Method.Method);
        Assert.Equal("Basic dXNlcjpwYXNz", request?.Headers.Authorization?.ToString());
        Assert.Contains("preview=true", request?.RequestUri?.Query);
        Assert.Equal("application/x-www-form-urlencoded", request?.Content?.Headers.ContentType?.MediaType);
    }

    private sealed class Handler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> handle;

        public Handler(Func<HttpRequestMessage, HttpResponseMessage> handle)
        {
            this.handle = handle;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(handle(request));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
            handle(request);
    }
}
