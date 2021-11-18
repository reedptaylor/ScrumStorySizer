using Microsoft.AspNetCore.ResponseCompression;
using ScrumStorySizer.Server;
using ScrumStorySizer.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSingleton<CacheService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();
else
    app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.Context.Request.Path.HasValue && (ctx.Context.Request.Path.Value.Contains("/api") || ctx.Context.Request.Path.Value.Contains("/devops")
            || ctx.Context.Request.Path.Value.EndsWith("site.css") || ctx.Context.Request.Path.Value.EndsWith("site.js")))
            ctx.Context.Response.Headers.Add("Cache-Control", "no-cache");
    }
});

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapHub<VoteHub>("/votehub");
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.Run();