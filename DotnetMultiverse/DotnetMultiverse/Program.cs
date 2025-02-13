using System;
using System.IO;
using DotnetMultiverse.Components;
using DotnetMultiverse.Shared.SoundConverter;
using DotnetMultiverse.Shared.SoundConverter.AudioHandlers;
using DotnetMultiverse.Shared.SoundConverter.ConversionScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddRouting()
    .AddMudServices()
    .AddSingleton<AudioHandler>()
    .AddSingleton<Mp3Handler>()
    .AddSingleton<IConversionScheduler, ConversionScheduler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/_framework") && !ctx.Request.Path.StartsWithSegments("/_framework/blazor.server.js") && !ctx.Request.Path.StartsWithSegments("/_framework/blazor.web.js"))
    {
        var fileExtension = Path.GetExtension(ctx.Request.Path);
        if (string.Equals(fileExtension, ".js", StringComparison.OrdinalIgnoreCase) || string.Equals(fileExtension, ".mjs", StringComparison.OrdinalIgnoreCase))
        {
            // Browser multi-threaded runtime requires cross-origin policy headers to enable SharedArrayBuffer.
        }
    }
    ApplyCrossOriginPolicyHeaders(ctx);

    await next(ctx);
});

app.UseRouting()
    .UseStaticFiles(new StaticFileOptions
    {
        // In development, serve everything, as there's no other way to configure it.
        // In production, developers are responsible for configuring their own production server
        ServeUnknownFileTypes = true,
    });

app.MapFallbackToFile("index.html", new StaticFileOptions
{
    OnPrepareResponse = fileContext =>
    {
        // Avoid caching index.html during development.
        // When hot reload is enabled, a middleware injects a hot reload script into the response HTML.
        // We don't want the browser to bypass this injection by using a cached response that doesn't
        // contain the injected script. In the future, if script injection is removed in favor of a
        // different mechanism, we can delete this comment and the line below it.
        // See also: https://github.com/dotnet/aspnetcore/issues/45213
        fileContext.Context.Response.Headers[HeaderNames.CacheControl] = "no-store";
        ApplyCrossOriginPolicyHeaders(fileContext.Context);
    }
});

app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(DotnetMultiverse.Client.SoundConverter._Imports).Assembly);

app.Run();

static void ApplyCrossOriginPolicyHeaders(HttpContext httpContext)
{
    httpContext.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
    httpContext.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
}