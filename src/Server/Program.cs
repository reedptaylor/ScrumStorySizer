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
    string format = context.Request.Query["format"];
    string specificVersion = context.Request.Query["version"];

    string path = Environment.GetEnvironmentVariable("RELEASE_NOTES_PATH"); // Set path from environment if running from Docker
    if (string.IsNullOrEmpty(path))
        path = "../../release-notes";

    List<Task<string>> fileTasks = new();
    IEnumerable<string> files = System.IO.Directory.GetFiles($"{path}/versions").OrderByDescending(f => f); // Get all version file paths 

    string markdown;
    if (!string.IsNullOrWhiteSpace(specificVersion))
    {
        if (specificVersion.Equals("latest", StringComparison.InvariantCultureIgnoreCase)) // Get only latest version
            markdown = await System.IO.File.ReadAllTextAsync(files.FirstOrDefault());
        else if (files.Any(f => f.EndsWith($"{specificVersion}.md"))) // Get specific version
            markdown = await System.IO.File.ReadAllTextAsync(files.FirstOrDefault(f => f.EndsWith($"{specificVersion}.md")));
        else // Version not found
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Release notes not found.");
            return;
        }
    }
    else
    {
        foreach (string file in files) // Get all versions
        {
            fileTasks.Add(System.IO.File.ReadAllTextAsync(file));
        }
        await Task.WhenAll(fileTasks);
        markdown = fileTasks.Select(t => t.Result).Aggregate((a, b) => a + Environment.NewLine + Environment.NewLine + b)?.Trim();
    }

    if ((format?.Equals("markdown", StringComparison.InvariantCultureIgnoreCase) ?? false) || (format?.Equals("md", StringComparison.InvariantCultureIgnoreCase) ?? false))
    {
        context.Response.ContentType = "text/markdown";
        await context.Response.WriteAsync(markdown); // Return raw markdown
    }
    else if (format?.Equals("simple", StringComparison.InvariantCultureIgnoreCase) ?? false) // Use this about page
    {
        context.Response.ContentType = "text/html"; // Set to display page as HTML
        await context.Response.WriteAsync(Markdown.ToHtml(markdown)); // Convert markdown to html
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