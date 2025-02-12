using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DotnetMultiverse.PWA.SoundConverter;
using DotnetMultiverse.Shared.SoundConverter;
using DotnetMultiverse.Shared.SoundConverter.AudioHandlers;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<AudioHandler>();
builder.Services.AddSingleton<Mp3Handler>();
builder.Services.AddSingleton<IConversionScheduler, ConversionScheduler>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
