using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DotnetMultiverse.PWA.SoundConverter;
using DotnetMultiverse.Shared.SoundConverter;
using DotnetMultiverse.Shared.SoundConverter.AudioHandlers;
using DotnetMultiverse.Shared.SoundConverter.ConversionScheduler;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped<AudioHandler>()
    .AddScoped<Mp3Handler>()
    .AddScoped<IConversionScheduler, ConversionScheduler>()
    .AddMudServices();

await builder.Build().RunAsync();
