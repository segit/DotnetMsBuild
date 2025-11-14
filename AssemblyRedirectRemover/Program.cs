using AssemblyRedirectRemover;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var hab = Host.CreateApplicationBuilder(args);

    
var host = hab.Build();
var conf = host.Services.GetRequiredService<IConfiguration>();
var lf = host.Services.GetRequiredService<IHostApplicationLifetime>();


string path = "C:\\tlx\\wsp8";
var list = await path.GetConfigFiles()
        .ConfigureAwait(false);

//path = "./SampleData/App.config";
//path.ChangeAssemblyRedirect("System.ValueTuple", "0.0.0.0-4.0.5.0", "4.0.0.0"); ;


//foreach (var item in list)
//{
//    item.ChangeAssemblyRedirect("System.ValueTuple", "0.0.0.0-4.0.5.0", "4.0.0.0");
//    Console.WriteLine(item);
//}


//foreach(var item in list)
//{
//    item.RemoveAssemblyRedirect("System.ValueTuple");
//    Console.WriteLine(item);
//}

//path = "./SampleData/App.config";
//path.RemoveAssemblyRedirect("System.ValueTuple");
Console.WriteLine("Done!");
