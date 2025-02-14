using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using ServerApp.CustomDevServer.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var applicationPath = args.SkipWhile(a => a != "--applicationpath").Skip(1).First();
var assemblyName = args.SkipWhile(a => a != "--assemblyname").Skip(1).First();
var runtimeManifest = Directory.GetFiles(applicationPath, $"{assemblyName}.staticwebassets.runtime.json").SingleOrDefault();
var endpointsManifest = Directory.GetFiles(applicationPath, $"{assemblyName}.staticwebassets.endpoints.json").Single();
var inMemoryConfiguration = new Dictionary<string, string?>
{
    [WebHostDefaults.StaticWebAssetsKey] = runtimeManifest,
    ["staticAssets"] = endpointsManifest,
    ["ApplyCopHeaders"] = args.Contains("--apply-cop-headers").ToString(),
};
var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    WebRootPath = Path.Combine(applicationPath, "wwwroot")
});

builder.Configuration
    .AddInMemoryCollection(inMemoryConfiguration)
    .AddJsonFile(Path.Combine(Path.GetDirectoryName( typeof(Program).Assembly.Location)!, "appsettings.json"), optional: false);

builder.Services
    .AddSerilog(options => options.ReadFrom.Configuration(builder.Configuration))
    .AddRouting();

// Needed to serve when non-published wwwroot.
builder.WebHost.UseStaticWebAssets();

builder
    .Build()
    .AddMiddleware()
    .Run();

