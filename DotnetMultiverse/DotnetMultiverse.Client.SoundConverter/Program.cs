using DotnetMultiverse.Shared.SoundConverter;
using DotnetMultiverse.Shared.SoundConverter.AudioHandlers;
using DotnetMultiverse.Shared.SoundConverter.ConversionScheduler;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services
    .AddMudServices()
    .AddScoped<AudioHandler>()
    .AddScoped<Mp3Handler>()
    .AddScoped<IConversionScheduler, ConversionScheduler>();

await builder.Build().RunAsync();