using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDepsAnalyzer
{
    /// <summary>
    /// Represents a node in the assembly dependency tree
    /// </summary>
    public class AssemblyTreeNode
    {
        /// <summary>
        /// Assembly name
        /// </summary>
        public AssemblyName AssemblyName { get; set; }

        /// <summary>
        /// Assembly location path
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Direct dependencies of this assembly
        /// </summary>
        public List<AssemblyTreeNode> Dependencies { get; set; }

        /// <summary>
        /// Parent assemblies that depend on this one
        /// </summary>
        public List<AssemblyTreeNode> DependentAssemblies { get; set; }

        /// <summary>
        /// Depth level in the tree (0 = root)
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Whether this node has been visited during traversal
        /// </summary>
        public bool IsVisited { get; set; }

        /// <summary>
        /// Indicates if this is a circular reference
        /// </summary>
        public bool IsCircularReference { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AssemblyTreeNode()
        {
            Dependencies = new List<AssemblyTreeNode>();
            DependentAssemblies = new List<AssemblyTreeNode>();
        }

        /// <summary>
        /// Constructor with assembly name
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="location"></param>
        public AssemblyTreeNode(AssemblyName assemblyName, string location = null) : this()
        {
            AssemblyName = assemblyName;
            Location = location;
        }

        /// <summary>
        /// Gets display name for the assembly
        /// </summary>
        public string DisplayName => AssemblyName?.Name ?? "Unknown";

        /// <summary>
        /// Gets version string
        /// </summary>
        public string Version => AssemblyName?.Version?.ToString() ?? "Unknown";

        /// <summary>
        /// Gets full display text
        /// </summary>
        public string FullDisplayName => $"{DisplayName} ({Version})";
    }
}
