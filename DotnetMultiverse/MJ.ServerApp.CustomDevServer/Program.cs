using System.IO;
using System.Linq;
using MJ.ServerApp.CustomDevServer.Startup;
using Microsoft.AspNetCore.Builder;

var applicationPath = args.SkipWhile(a => a != "--applicationpath").Skip(1).First();
var assemblyName = args.SkipWhile(a => a != "--assemblyname").Skip(1).First();
WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        WebRootPath = Path.Combine(applicationPath, "wwwroot")
    })
    .AddApp(applicationPath, assemblyName)
    .Build()
    .UseApp()
    .Run();