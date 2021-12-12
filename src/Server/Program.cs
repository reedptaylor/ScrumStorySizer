using Markdig;
using Microsoft.AspNetCore.ResponseCompression;
using ScrumStorySizer.Server;
using ScrumStorySizer.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 256000;
});

builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy")); // Use reverse proxy to allow clients to make requests to DevOps

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

app.MapGet("release-notes", async (context) => // Map release notes endpoint
{
    if (!bool.TryParse(context.Request.Query["markdown"], out bool rawMarkdown))
        rawMarkdown = false;

    string path = Environment.GetEnvironmentVariable("RELEASE_NOTES_PATH"); // Set path from environment if running from Docker
    if (string.IsNullOrEmpty(path))
        path = "../../release-notes";

    // List<Task<string>> fileTasks = new();TODO
    // System.IO.Directory.GetFiles($"{path}/versions")
    //     .OrderByDescending(f => f)
    //     .ToList()
    //     .ForEach(async f => 
    //     {
    //         fileTasks.Add(System.IO.File.ReadAllTextAsync(f));
    //     });

    // await Task.WhenAll(fileTasks);
    string markdown = await System.IO.File.ReadAllTextAsync($"{path}/release-notes.md");

    if (rawMarkdown)
    {
        context.Response.ContentType = "text/markdown";
        await context.Response.WriteAsync(markdown); // Return raw markdown
    }
    else
    {
        context.Response.ContentType = "text/html"; // Set to display page as HTML
        string htmlTemplate = await System.IO.File.ReadAllTextAsync($"{path}/release-notes-template.html");
        await context.Response.WriteAsync(string.Format(htmlTemplate, Markdown.ToHtml(markdown))); // Convert markdown to html and inject into template
    }

});

app.MapRazorPages();
app.MapControllers();
app.MapHub<VoteHub>("/votehub");
app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.Run();