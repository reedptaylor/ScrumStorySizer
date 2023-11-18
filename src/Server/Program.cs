using System.Diagnostics;
using Markdig;
using Microsoft.AspNetCore.ResponseCompression;
using ScrumStorySizer.Library;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using ScrumStorySizer.Server.Hubs;
using ScrumStorySizer.Server.Services;

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
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat( [ "application/octet-stream" ]);
});

// Use reverse proxy to allow clients to make requests to DevOps
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSingleton<VotingServiceData>();
builder.Services.AddSingleton<CommandService>();

#region Client DI

builder.Services.AddScoped<IVotingService, ServerVotingService>();

#endregion

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
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
    }
});

app.UseRouting();

app.MapGet("release-notes", async (context) => // Map release notes endpoint
{
    string format = context.Request.Query["format"];
    string specificVersion = context.Request.Query["version"];

    string path = $"{AppDomain.CurrentDomain.BaseDirectory}release-notes";

    // Get all version file paths
    List<Task<string>> fileTasks = [];
    IEnumerable<string> files = Directory.GetFiles($"{path}/versions").OrderByDescending(f =>
    {
        string fileName = Path.GetFileNameWithoutExtension(f);
        int semVerCardinality = fileName.Count(c => c == '.');
        while (semVerCardinality < 3)
        {
            fileName += ".0";
            semVerCardinality += 1;
        }
        return fileName;
    });

    string markdown;
    if (!string.IsNullOrWhiteSpace(specificVersion))
    {
        if (specificVersion.Equals("latest", StringComparison.InvariantCultureIgnoreCase)) // Get only latest version
            markdown = await File.ReadAllTextAsync(files.FirstOrDefault());
        else if (files.Any(f => f.EndsWith($"{specificVersion}.md"))) // Get specific version
            markdown = await File.ReadAllTextAsync(files.FirstOrDefault(f => f.EndsWith($"{specificVersion}.md")));
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
            fileTasks.Add(File.ReadAllTextAsync(file));
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
        string htmlTemplate = await File.ReadAllTextAsync($"{path}/release-notes-template.html");
        await context.Response.WriteAsync(string.Format(htmlTemplate, Markdown.ToHtml(markdown))); // Convert markdown to html and inject into template
    }
});

app.MapGet("uptime", async (context) => // Map uptime endpoint
{
    var process = new Process()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "stat",
            Arguments = "-c %y /proc/1",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        }
    };
    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    if (string.IsNullOrEmpty(error) && DateTime.TryParse(output, out DateTime startTime))
        await context.Response.WriteAsync((DateTime.UtcNow - startTime).ToRelativeTimeStamp());
    else
        await context.Response.WriteAsync("N/A");
});


app.MapRazorPages();
app.MapControllers();
app.MapHub<VoteHub>("/votehub");
app.MapReverseProxy();
app.MapFallbackToPage("/_Host");

app.Run();