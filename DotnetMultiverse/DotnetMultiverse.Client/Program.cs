using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DotnetMultiverse.SoundConverter;
using DotnetMultiverse.SoundConverter.AudioHandlers;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddSingleton<AudioHandler>();
builder.Services.AddSingleton<Mp3Handler>();
builder.Services.AddMudServices();

builder.Services.AddMudServices();

await builder.Build().RunAsync();