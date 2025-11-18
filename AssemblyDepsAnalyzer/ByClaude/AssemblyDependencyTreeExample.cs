using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDepsAnalyzer
{
    using System;
    using System.Linq;

    namespace Telexy.Fusion.Runtime.Jet.Net.Compiler
    {
        /// <summary>
        /// Example usage of the assembly dependency tree builder
        /// </summary>
        public static class AssemblyDependencyTreeExample
        {
            /// <summary>
            /// Example method showing how to build and use the dependency tree
            /// </summary>
            public static void ExampleUsage()
            {
                // Build the dependency tree directly from AppDomain
                var tree = AssemblyDependencyTree.BuildFromAppDomain();

                // Get and display statistics
                var stats = tree.GetStatistics();
                Console.WriteLine("Assembly Dependency Tree Statistics:");
                Console.WriteLine(stats.ToString());
                Console.WriteLine();

                // Print the complete tree structure
                Console.WriteLine(tree.ToTreeString(includeVersions: true));

                // Find dependencies for a specific assembly
                var systemAssembly = tree.FindNode("System");
                if (systemAssembly != null)
                {
                    Console.WriteLine($"\nDependencies of {systemAssembly.DisplayName}:");
                    foreach (var dep in systemAssembly.Dependencies)
                    {
                        Console.WriteLine($"  - {dep.FullDisplayName}");
                    }

                    Console.WriteLine($"\nAssemblies that depend on {systemAssembly.DisplayName}:");
                    foreach (var dependent in systemAssembly.DependentAssemblies)
                    {
                        Console.WriteLine($"  - {dependent.FullDisplayName}");
                    }
                }

                // Check for circular references
                var circularRefs = tree.GetCircularReferences().ToList();
                if (circularRefs.Any())
                {
                    Console.WriteLine($"\nCircular References Detected ({circularRefs.Count}):");
                    foreach (var circularRef in circularRefs)
                    {
                        Console.WriteLine($"  - {circularRef.FullDisplayName}");
                    }
                }
                else
                {
                    Console.WriteLine("\nNo circular references detected.");
                }

                // Find the dependency path for a specific assembly
                FindDependencyChain(tree, "Telexy.Fusion.Runtime.Jet.Net");
            }

            /// <summary>
            /// Find and display the complete dependency chain for an assembly
            /// </summary>
            /// <param name="tree"></param>
            /// <param name="assemblyName"></param>
            public static void FindDependencyChain(AssemblyDependencyTree tree, string assemblyName)
            {
                var node = tree.FindNode(assemblyName);
                if (node == null)
                {
                    Console.WriteLine($"Assembly '{assemblyName}' not found in loaded assemblies.");
                    return;
                }

                Console.WriteLine($"\nComplete dependency chain for {assemblyName}:");
                PrintDependencyChain(node, 0, new System.Collections.Generic.HashSet<AssemblyTreeNode>());
            }

            /// <summary>
            /// Recursively prints dependency chain
            /// </summary>
            /// <param name="node"></param>
            /// <param name="level"></param>
            /// <param name="visited"></param>
            private static void PrintDependencyChain(AssemblyTreeNode node, int level,
                System.Collections.Generic.HashSet<AssemblyTreeNode> visited)
            {
                var indent = new string(' ', level * 2);
                Console.WriteLine($"{indent}{node.FullDisplayName}");

                if (visited.Contains(node))
                {
                    Console.WriteLine($"{indent}  [Already visited - circular reference]");
                    return;
                }

                visited.Add(node);

                foreach (var dependency in node.Dependencies.OrderBy(d => d.DisplayName))
                {
                    PrintDependencyChain(dependency, level + 1, visited);
                }

                visited.Remove(node);
            }

            /// <summary>
            /// Lists all assemblies by their dependency count
            /// </summary>
            /// <param name="tree"></param>
            public static void ListAssembliesByDependencyCount(AssemblyDependencyTree tree)
            {
                Console.WriteLine("\nAssemblies sorted by dependency count:");
                Console.WriteLine("======================================");

                var sortedAssemblies = tree.AllNodes
                    .OrderByDescending(n => n.Dependencies.Count)
                    .ThenBy(n => n.DisplayName);

                foreach (var assembly in sortedAssemblies)
                {
                    Console.WriteLine($"{assembly.FullDisplayName} - {assembly.Dependencies.Count} dependencies, {assembly.DependentAssemblies.Count} dependents");
                }
            }
        }
    }
}
