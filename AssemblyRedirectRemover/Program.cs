using AssemblyRedirectRemover;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsBuildUtils;

var hab = Host.CreateApplicationBuilder(args);

    
var host = hab.Build();
var conf = host.Services.GetRequiredService<IConfiguration>();
var lf = host.Services.GetRequiredService<IHostApplicationLifetime>();

//string? asep = Environment.GetEnvironmentVariable("asep");
//ArgumentException.ThrowIfNullOrWhiteSpace(asep);
//string sln = Path.GetFullPath($"{asep}/src/Ase.WebApi.slnx");
//DotnetVersionUpdaterM
//    .UpdateAllDotnetVersionsTo(sln, new string[] { "8.0", "9.0" }, "net10.0");
/ new NugetPmSlnRefCentralizer()
//    .MovePackageVersionsToCentralStore(sln);
string? wsp = Environment.GetEnvironmentVariable("wsp8");
ArgumentException.ThrowIfNullOrWhiteSpace(wsp);
string path = Path.Combine(wsp, "Fusion.One");
DirectoryInfo dir = new DirectoryInfo(path);
AssemblyRedirectsRemover.RemoveAssemblyRedirectsInDirectory(path, "Microsoft.Extensions.AI");
Console.WriteLine("Done!");
