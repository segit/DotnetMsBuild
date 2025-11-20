using AssemblyRedirectRemover;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsBuildUtils;

var hab = Host.CreateApplicationBuilder(args);

    
var host = hab.Build();
var conf = host.Services.GetRequiredService<IConfiguration>();
var lf = host.Services.GetRequiredService<IHostApplicationLifetime>();

string? asep = Environment.GetEnvironmentVariable("asep");
ArgumentException.ThrowIfNullOrWhiteSpace(asep);
string sln = Path.GetFullPath($"{asep}/src/Ase.WebApi.slnx");
DotnetVersionUpdaterM
    .UpdateAllDotnetVersionsTo(sln, new string[] { "8.0", "9.0" }, "net10.0");
Console.WriteLine("Done!");
