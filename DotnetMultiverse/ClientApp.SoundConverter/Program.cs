using Module.SoundConverter;
using Module.SoundConverter.AudioHandlers;
using Module.SoundConverter.ConversionScheduler;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false);

builder.Services
    .AddMudServices()
    .AddSerilog(x =>
    {
        x.ReadFrom.Configuration(builder.Configuration);
    })
    .AddScoped<AudioHandler>()
    .AddScoped<Mp3Handler>()
    .AddScoped<IConversionScheduler, ConversionScheduler>();

await builder.Build().RunAsync();