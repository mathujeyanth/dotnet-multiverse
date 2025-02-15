using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MJ.PWA.SoundConverter;
using MJ.Module.SoundConverter;
using MJ.Module.SoundConverter.AudioHandlers;
using MJ.Module.SoundConverter.ConversionScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
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
