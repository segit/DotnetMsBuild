using AssemblyRedirectRemover;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var hab = Host.CreateApplicationBuilder(args);

    
var host = hab.Build();
var conf = host.Services.GetRequiredService<IConfiguration>();
var lf = host.Services.GetRequiredService<IHostApplicationLifetime>();

string path = "C:\\tlx\\wsp1";
var list = await path.GetConfigFiles()
        .ConfigureAwait(false);
foreach(var item in list)
{
    Console.WriteLine(item);
}

path = "./SampleData/App.config";
path.RemoveAssemblyRedirect("System.ValueTuple");
Console.WriteLine("Done!");
