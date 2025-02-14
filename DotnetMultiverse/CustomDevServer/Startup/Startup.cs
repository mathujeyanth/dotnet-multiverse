using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace CustomDevServer.Startup;

internal static class Startup
{
    public static WebApplication AddMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        
        app.EnableConfiguredPathBase();

        var webHostEnvironment = app.Services.GetRequiredService<IWebHostEnvironment>();
        var applyCopHeaders = app.Configuration.GetValue<bool>("ApplyCopHeaders");
        app.Logger.LogDebug($"ApplyCopHeaders: {applyCopHeaders}");
        applyCopHeaders = true;
        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.StartsWithSegments("/_framework/blazor.boot.json"))
            {
                ctx.Response.Headers.Append("Blazor-Environment", webHostEnvironment.EnvironmentName);
            }
            else if (applyCopHeaders && ctx.Request.Path.StartsWithSegments("/_framework") && !ctx.Request.Path.StartsWithSegments("/_framework/blazor.server.js") && !ctx.Request.Path.StartsWithSegments("/_framework/blazor.web.js"))
            {
                var fileExtension = Path.GetExtension(ctx.Request.Path);
                if (string.Equals(fileExtension, ".js", StringComparison.OrdinalIgnoreCase) || string.Equals(fileExtension, ".mjs", StringComparison.OrdinalIgnoreCase))
                {
                    // Browser multi-threaded runtime requires cross-origin policy headers to enable SharedArrayBuffer.
                    ctx.ApplyCrossOriginPolicyHeaders();
                }
            }

            await next(ctx);
        });

        app.UseRouting();

        app.UseStaticFiles(new StaticFileOptions
        {
            // In development, serve everything, as there's no other way to configure it.
            // In production, developers are responsible for configuring their own production server
            ServeUnknownFileTypes = true
        });

        var manifest = app.Configuration["staticAssets"]!;
        app.MapStaticAssets(manifest);
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

                if (applyCopHeaders)
                {
                    // Browser multi-threaded runtime requires cross-origin policy headers to enable SharedArrayBuffer.
                    fileContext.Context.ApplyCrossOriginPolicyHeaders();
                }
            }
        });
        return app;
    }

    private static void EnableConfiguredPathBase(this WebApplication app)
    {
        var pathBase = app.Configuration.GetValue<string>("pathbase");
        if (string.IsNullOrEmpty(pathBase))
        {
            return;
        }

        app.UsePathBase(pathBase);

        // To ensure consistency with a production environment, only handle requests
        // that match the specified pathbase.
        app.Use((context, next) =>
        {
            if (context.Request.PathBase == pathBase)
            {
                return next(context);
            }

            context.Response.StatusCode = 404;
            return context.Response.WriteAsync($"The server is configured only to " +
                                               $"handle request URIs within the PathBase '{pathBase}'.");
        });
    }

    private static void ApplyCrossOriginPolicyHeaders(this HttpContext httpContext)
    {
        httpContext.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
        httpContext.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    }
}