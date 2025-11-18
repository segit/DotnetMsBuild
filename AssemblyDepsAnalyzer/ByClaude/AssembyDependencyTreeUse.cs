using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDepsAnalyzer
{
    internal static class AssembyDependencyTreeUse
    {
        internal static void Run(string[] args)
        {
            // Simple usage - build and display the tree
            var tree = AssemblyDependencyTree.BuildFromAppDomain();
            Console.WriteLine(tree.ToTreeString());

            // Get statistics
            var stats = tree.GetStatistics();
            Console.WriteLine(stats);

            // Find specific assembly dependencies
            var myAssembly = tree.FindNode("MyAssembly");
            foreach (var dep in myAssembly.Dependencies)
            {
                Console.WriteLine($"Depends on: {dep.FullDisplayName}");
            }

            Console.WriteLine("Hello, World!");
        }
    }
}
