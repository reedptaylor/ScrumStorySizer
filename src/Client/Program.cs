using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ScrumStorySizer.Library.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<IVotingService>(sp => new ClientVotingService($"{builder.HostEnvironment.BaseAddress}votehub"));

await builder.Build().RunAsync();