using MJ.Module.AudioConverter.ConversionScheduler;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MJ.Module.AudioConverter.AudioHandler;
using MJ.Module.AudioConverter.IAudioCreators;
using MudBlazor.Services;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Configuration
    .AddJsonFile("appsettings.json", false);

builder.Services
    .AddMudServices()
    .AddSerilog(x => { x.ReadFrom.Configuration(builder.Configuration); })
    .AddScoped<IAudioHandler, AudioHandler>()
    .AddScoped<Mp3IAudioCreator>()
    .AddScoped<IConversionScheduler, ConversionScheduler>();

await builder.Build().RunAsync();