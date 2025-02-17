using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MJ.PWA.SoundConverter;
using MJ.Module.SoundConverter.ConversionScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MJ.Module.SoundConverter.AudioHandler;
using MJ.Module.SoundConverter.IAudioCreators;
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
    .AddScoped<IAudioHandler, AudioHandler>()
    .AddScoped<Mp3IAudioCreator>()
    .AddScoped<IConversionScheduler, ConversionScheduler>();

await builder.Build().RunAsync();
