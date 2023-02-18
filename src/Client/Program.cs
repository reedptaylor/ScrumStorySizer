using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ScrumStorySizer.Library.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<IVotingService>(sp => new VotingService($"{builder.HostEnvironment.BaseAddress}votehub"));

await builder.Build().RunAsync();