using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DotnetMultiverse.SoundConverter;
using DotnetMultiverse.SoundConverter.AudioHandlers;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped<AudioHandler>();
builder.Services.AddScoped<Mp3Handler>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();