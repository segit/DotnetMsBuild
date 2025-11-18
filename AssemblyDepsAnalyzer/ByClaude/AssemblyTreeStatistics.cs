using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyDepsAnalyzer
{
    /// <summary>
    /// Statistics about the assembly dependency tree
    /// </summary>
    public class AssemblyTreeStatistics
    {
        /// <summary>
        /// Total number of assemblies in the tree
        /// </summary>
        public int TotalAssemblies { get; set; }

        /// <summary>
        /// Number of root assemblies (no dependencies in loaded set)
        /// </summary>
        public int RootAssemblies { get; set; }

        /// <summary>
        /// Number of circular references detected
        /// </summary>
        public int CircularReferences { get; set; }

        /// <summary>
        /// Maximum depth in the dependency tree
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Average depth in the dependency tree
        /// </summary>
        public double AverageDepth { get; set; }

        /// <summary>
        /// String representation of statistics
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Total Assemblies: {TotalAssemblies}, " +
                   $"Root Assemblies: {RootAssemblies}, " +
                   $"Circular References: {CircularReferences}, " +
                   $"Max Depth: {MaxDepth}, " +
                   $"Avg Depth: {AverageDepth:F2}";
        }
    }
}
