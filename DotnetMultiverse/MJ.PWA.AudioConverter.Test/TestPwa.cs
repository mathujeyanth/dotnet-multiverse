using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright.Xunit;
using MJ.ServerApp.CustomDevServer.Startup;
using Xunit;
using Xunit.Abstractions;

namespace MJ.PWA.AudioConverter.Test;

public class TestPwa : PageTest, IAsyncDisposable
{
    private const string AppUrl = "http://localhost:5070";

    private readonly Task _appTask;
    private readonly CancellationTokenSource _cts = new();
    private readonly ITestOutputHelper _testOutputHelper;

    public TestPwa(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var pwaDebugAssembly = typeof(_Imports).Assembly;
        var pwaAssemblyName = pwaDebugAssembly.GetName().Name!;
        var pwaAssemblyPath =
            new DirectoryInfo(Path.GetDirectoryName(pwaDebugAssembly.Location)!).Parent!.Parent!.Parent!.Parent!
                .ToString()!;
        var applicationPath = Path.Combine(pwaAssemblyPath, pwaAssemblyName, "bin", "Release", "net9.0", "publish");
        _testOutputHelper.WriteLine($"Assembly name: {pwaDebugAssembly}");
        _testOutputHelper.WriteLine($"Application Path: {applicationPath}");
        _testOutputHelper.WriteLine($"PWA Path: {pwaAssemblyPath}");
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", AppUrl);

        _appTask = WebApplication
            .CreateBuilder(new WebApplicationOptions
            {
                WebRootPath = Path.Combine(applicationPath, "wwwroot")
            })
            .AddApp(applicationPath, pwaAssemblyName)
            .Build()
            .UseApp()
            .RunAsync(_cts.Token);
        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
    }

    [Fact]
    public async Task Should_ReachPage_When_ClientIsOffline()
    {
        // ARRANGE
        await Page.Context.SetOfflineAsync(true);
        // Make sure the page is not cached.
        try
        {
            await Page.GotoAsync(AppUrl);
            throw new Exception("The page has been cached");
        }
        catch (Microsoft.Playwright.PlaywrightException e) when (e.Message.Contains("net::ERR_INTERNET_DISCONNECTED"))
        {
        }

        await Page.Context.SetOfflineAsync(false);
        await Page.GotoAsync(AppUrl);
        await Expect(Page).ToHaveTitleAsync(new Regex("DotnetMultiverse"));
        // Wait for page to have been downloaded.
        await Task.Delay(TimeSpan.FromSeconds(5));

        // ACT
        await Page.Context.SetOfflineAsync(true);
        await Page.GotoAsync(AppUrl);

        // ASSERT
        await Expect(Page).ToHaveTitleAsync(new Regex("DotnetMultiverse"));
    }

    public new async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        await _appTask.WaitAsync(TimeSpan.FromSeconds(3));
        _appTask.Dispose();
        _cts.Dispose();
        await base.DisposeAsync();
    }
}