using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDepsAnalyzer
{
    /// <summary>
    /// Tree structure for assembly dependencies built directly from AppDomain
    /// </summary>
    public class AssemblyDependencyTree
    {
        private readonly Dictionary<string, AssemblyTreeNode> _nodeCache;
        private readonly List<AssemblyTreeNode> _rootNodes;

        /// <summary>
        /// Constructor
        /// </summary>
        public AssemblyDependencyTree()
        {
            _nodeCache = new Dictionary<string, AssemblyTreeNode>(StringComparer.OrdinalIgnoreCase);
            _rootNodes = new List<AssemblyTreeNode>();
        }

        /// <summary>
        /// Gets all root nodes (assemblies with no dependencies or dependencies not in AppDomain)
        /// </summary>
        public IEnumerable<AssemblyTreeNode> RootNodes => _rootNodes;

        /// <summary>
        /// Gets all nodes in the tree
        /// </summary>
        public IEnumerable<AssemblyTreeNode> AllNodes => _nodeCache.Values;

        /// <summary>
        /// Builds the dependency tree from currently loaded assemblies in AppDomain
        /// </summary>
        /// <returns></returns>
        public static AssemblyDependencyTree BuildFromAppDomain()
        {
            var tree = new AssemblyDependencyTree();

            // Get all loaded assemblies from AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyNames = assemblies.Select(a => a.GetName()).ToList();

            // Create a lookup for fast assembly resolution
            var assemblyLookup = assemblies.ToDictionary(a => a.GetName().Name,
                StringComparer.OrdinalIgnoreCase);

            // First pass: Create all nodes
            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName();
                var location = GetAssemblyLocation(assembly);
                tree.GetOrCreateNode(assemblyName, location);
            }

            // Second pass: Build relationships
            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.GetName();
                var parentNode = tree.GetOrCreateNode(assemblyName);

                // Get referenced assemblies for this assembly
                var referencedAssemblies = assembly.GetReferencedAssemblies();

                foreach (var referencedAssemblyName in referencedAssemblies)
                {
                    // Only create dependencies for assemblies that are actually loaded in AppDomain
                    if (assemblyLookup.ContainsKey(referencedAssemblyName.Name))
                    {
                        var dependencyAssembly = assemblyLookup[referencedAssemblyName.Name];
                        var dependencyName = dependencyAssembly.GetName();
                        var dependencyLocation = GetAssemblyLocation(dependencyAssembly);
                        var dependencyNode = tree.GetOrCreateNode(dependencyName, dependencyLocation);
                        tree.AddDependency(parentNode, dependencyNode);
                    }
                }
            }

            // Third pass: Calculate depths and identify roots
            tree.CalculateTreeStructure();

            return tree;
        }

        /// <summary>
        /// Gets assembly location safely
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static string GetAssemblyLocation(Assembly assembly)
        {
            try
            {
                return assembly.Location;
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets or creates a node for the given assembly name
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private AssemblyTreeNode GetOrCreateNode(AssemblyName assemblyName, string location = null)
        {
            var key = assemblyName.Name;
            if (!_nodeCache.TryGetValue(key, out var node))
            {
                node = new AssemblyTreeNode(assemblyName, location);
                _nodeCache[key] = node;
            }
            return node;
        }

        /// <summary>
        /// Adds a dependency relationship between two nodes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="dependency"></param>
        private void AddDependency(AssemblyTreeNode parent, AssemblyTreeNode dependency)
        {
            if (!parent.Dependencies.Contains(dependency))
            {
                parent.Dependencies.Add(dependency);
            }

            if (!dependency.DependentAssemblies.Contains(parent))
            {
                dependency.DependentAssemblies.Add(parent);
            }
        }

        /// <summary>
        /// Calculates tree structure, depths, and identifies circular references
        /// </summary>
        private void CalculateTreeStructure()
        {
            // Reset all nodes
            foreach (var node in _nodeCache.Values)
            {
                node.IsVisited = false;
                node.Depth = -1;
                node.IsCircularReference = false;
            }

            // Find root nodes (nodes with no dependent assemblies in the loaded set)
            _rootNodes.Clear();
            foreach (var node in _nodeCache.Values)
            {
                if (node.DependentAssemblies.Count == 0)
                {
                    _rootNodes.Add(node);
                }
            }

            // Calculate depths from each root
            foreach (var root in _rootNodes)
            {
                CalculateDepth(root, 0, new HashSet<AssemblyTreeNode>());
            }
        }

        /// <summary>
        /// Recursively calculates depth and detects circular references
        /// </summary>
        /// <param name="node"></param>
        /// <param name="depth"></param>
        /// <param name="visitPath"></param>
        private void CalculateDepth(AssemblyTreeNode node, int depth, HashSet<AssemblyTreeNode> visitPath)
        {
            if (visitPath.Contains(node))
            {
                // Circular reference detected
                node.IsCircularReference = true;
                return;
            }

            if (node.Depth < depth)
            {
                node.Depth = depth;
            }

            visitPath.Add(node);

            foreach (var dependency in node.Dependencies)
            {
                CalculateDepth(dependency, depth + 1, visitPath);
            }

            visitPath.Remove(node);
        }

        /// <summary>
        /// Finds a node by assembly name
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public AssemblyTreeNode FindNode(string assemblyName)
        {
            _nodeCache.TryGetValue(assemblyName, out var node);
            return node;
        }

        /// <summary>
        /// Gets all assemblies that depend on the specified assembly
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public IEnumerable<AssemblyTreeNode> GetDependentAssemblies(string assemblyName)
        {
            var node = FindNode(assemblyName);
            return node?.DependentAssemblies ?? Enumerable.Empty<AssemblyTreeNode>();
        }

        /// <summary>
        /// Gets all dependencies of the specified assembly
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public IEnumerable<AssemblyTreeNode> GetDependencies(string assemblyName)
        {
            var node = FindNode(assemblyName);
            return node?.Dependencies ?? Enumerable.Empty<AssemblyTreeNode>();
        }

        /// <summary>
        /// Gets all circular references in the tree
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AssemblyTreeNode> GetCircularReferences()
        {
            return _nodeCache.Values.Where(n => n.IsCircularReference);
        }

        /// <summary>
        /// Generates a text-based tree representation
        /// </summary>
        /// <param name="includeVersions"></param>
        /// <returns></returns>
        public string ToTreeString(bool includeVersions = true)
        {
            var sb = new StringBuilder();
            var visited = new HashSet<AssemblyTreeNode>();

            sb.AppendLine("Assembly Dependency Tree (AppDomain Loaded Assemblies):");
            sb.AppendLine("========================================================");

            foreach (var root in _rootNodes.OrderBy(r => r.DisplayName))
            {
                AppendNodeToString(sb, root, "", true, visited, includeVersions);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Recursively appends node to string representation
        /// </summary>
        private void AppendNodeToString(StringBuilder sb, AssemblyTreeNode node, string indent,
            bool isLast, HashSet<AssemblyTreeNode> visited, bool includeVersions)
        {
            sb.Append(indent);
            sb.Append(isLast ? "└── " : "├── ");

            var displayText = includeVersions ? node.FullDisplayName : node.DisplayName;
            if (node.IsCircularReference)
            {
                displayText += " [CIRCULAR]";
            }

            sb.AppendLine(displayText);

            if (visited.Contains(node))
            {
                // Already visited, don't recurse to avoid infinite loops
                return;
            }

            visited.Add(node);

            var childIndent = indent + (isLast ? "    " : "│   ");
            var dependencies = node.Dependencies.OrderBy(d => d.DisplayName).ToList();

            for (int i = 0; i < dependencies.Count; i++)
            {
                var isLastChild = i == dependencies.Count - 1;
                AppendNodeToString(sb, dependencies[i], childIndent, isLastChild, visited, includeVersions);
            }

            visited.Remove(node);
        }

        /// <summary>
        /// Gets statistics about the tree
        /// </summary>
        /// <returns></returns>
        public AssemblyTreeStatistics GetStatistics()
        {
            return new AssemblyTreeStatistics
            {
                TotalAssemblies = AllNodes.Count(),
                RootAssemblies = RootNodes.Count(),
                CircularReferences = GetCircularReferences().Count(),
                MaxDepth = AllNodes.Any() ? AllNodes.Max(n => n.Depth) : 0,
                AverageDepth = AllNodes.Any() ? AllNodes.Average(n => n.Depth) : 0
            };
        }
    }
}
