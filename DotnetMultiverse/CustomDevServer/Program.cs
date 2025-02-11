using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using CustomDevServer.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var applicationPath = args.SkipWhile(a => a != "--applicationpath").Skip(1).First();
var applicationDirectory = Path.GetDirectoryName(applicationPath)!;
var name = Path.ChangeExtension(applicationPath, ".staticwebassets.runtime.json");
name = !File.Exists(name) ? Path.ChangeExtension(applicationPath, ".StaticWebAssets.xml") : name;
var endpointsManifest = Path.ChangeExtension(applicationPath, ".staticwebassets.endpoints.json");

var inMemoryConfiguration = new Dictionary<string, string?>
{
    // [WebHostDefaults.EnvironmentKey] = "Development",
    ["Logging:LogLevel:Microsoft"] = "Warning",
    ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information",
    [WebHostDefaults.StaticWebAssetsKey] = name,
    ["staticAssets"] = endpointsManifest,
    ["ApplyCopHeaders"] = args.Contains("--apply-cop-headers").ToString(),
    ["AllowedHosts"] = "*"
};

builder.Configuration.AddInMemoryCollection(inMemoryConfiguration);
builder.Configuration.AddJsonFile(Path.Combine(applicationDirectory, "blazor-devserversettings.json"), optional: true, reloadOnChange: true);

builder.Services.AddRouting();
builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

var configuration = app.Configuration;
app.AddMiddleware(configuration);

app.Run();