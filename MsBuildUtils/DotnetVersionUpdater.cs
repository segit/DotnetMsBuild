using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace MsBuildUtils
{
    public static class DotnetVersionUpdater
    {
        /// <summary>
        /// Updates the .NET version in the specified project file to the given version.
        /// </summary>
        /// <remarks>This method modifies the project file in place. Ensure that the specified file is
        /// writable and that the provided .NET version string is compatible with the project.</remarks>
        /// <param name="projFile">The path to the project file to be updated. Must not be null or empty.</param>
        /// <param name="newVersion">The new .NET version to set in the project file. Must be a valid .NET version string.</param>
        public static void UpdateDotnetVerisonTo(string projFile, string newVersion)
        {
            if (string.IsNullOrWhiteSpace(projFile))
                throw new ArgumentException("Project file path must be provided.", nameof(projFile));

            if (string.IsNullOrWhiteSpace(newVersion))
                throw new ArgumentException("New version must be provided.", nameof(newVersion));

            if (!File.Exists(projFile))
                throw new FileNotFoundException("Project file not found.", projFile);

            var normalized = VersionNormalizer.Normalize(newVersion);

            try
            {
                var doc = XDocument.Load(projFile);

                // Find all TargetFramework and TargetFrameworks elements
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                var targetFrameworkElements = doc.Descendants(ns + "TargetFramework").ToList();
               

                var changed = false;

                // Update single-target frameworks
                foreach (var tf in targetFrameworkElements)
                {
                    if (string.IsNullOrWhiteSpace(tf.Value) || !tf.Value.StartsWith("net", StringComparison.OrdinalIgnoreCase))
                    {
                        // Replace regardless to ensure the target is updated to requested runtime
                        tf.Value = normalized;
                        changed = true;
                    }
                    else
                    {
                        tf.Value = normalized;
                        changed = true;
                    }
                }

                #region ref
                /*
                var targetFrameworksElements = doc.Descendants(ns + "TargetFrameworks").ToList();

                // Update multi-target frameworks
                foreach (var tfs in targetFrameworksElements)
                {
                    var parts = tfs.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(p => p.Trim()).ToList();

                    for (int i = 0; i < parts.Count; i++)
                    {
                        var p = parts[i];
                        // If the entry looks like a .NET (SDK-style) TF, replace it. We detect by starting with "net".
                        if (p.StartsWith("net", StringComparison.OrdinalIgnoreCase))
                        {
                            parts[i] = normalized;
                            changed = true;
                        }
                    }

                    tfs.Value = string.Join(";", parts);
                }
                *
                /*
                if (!changed)
                {
                    // No existing TF elements found; attempt to add a TargetFramework element under the first PropertyGroup
                    var propertyGroup = doc.Descendants(ns + "PropertyGroup").FirstOrDefault();
                    if (propertyGroup != null)
                    {
                        propertyGroup.Add(new XElement(ns + "TargetFramework", normalized));
                        changed = true;
                    }
                }
                */
                #endregion

                if (changed)
                {
                    doc.Save(projFile);
                }
                else
                {
                    throw new InvalidOperationException("Unable to find or update target framework information in the project file.");
                }
            }
            catch (Exception ex) when (!(ex is ArgumentException) && !(ex is FileNotFoundException))
            {
                throw new InvalidOperationException($"Failed to update project file '{projFile}': {ex.Message}", ex);
            }
        }
    
        
         /// <summary>
        /// Loops through all projects in the solution and updates dotnet verision
        /// to the newVersion provided
        /// </summary>
        /// <param name="slnxFile"></param>
        /// <param name="newVersion"></param>
        public static void UpdateAllDotnetVersionsTo(string slnxFile, string newVersion)
        {
            if (string.IsNullOrWhiteSpace(slnxFile))
                throw new ArgumentException("Solution file path must be provided.", nameof(slnxFile));

            if (string.IsNullOrWhiteSpace(newVersion))
                throw new ArgumentException("New version must be provided.", nameof(newVersion));

            if (!File.Exists(slnxFile))
                throw new FileNotFoundException("Solution file not found.", slnxFile);

            var solutionDir = Path.GetDirectoryName(slnxFile) ?? string.Empty;
            var failures = new List<Exception>();

            foreach (var projPath in ProjectFilesEnumerator.Enumerate(slnxFile))
            {
                if (string.IsNullOrWhiteSpace(projPath))
                    continue;

                // Combine with solution directory to form an absolute path
                var combined = Path.GetFullPath(Path.Combine(solutionDir, projPath));

                if (!File.Exists(combined))
                {
                    // Skip missing project files
                    continue;
                }

                try
                {
                    UpdateDotnetVerisonTo(combined, newVersion);
                }
                catch (Exception ex)
                {
                    // Collect error and continue with other projects
                    failures.Add(new InvalidOperationException($"Failed to update project '{combined}': {ex.Message}", ex));
                }
            }

            if (failures.Any())
            {
                throw new AggregateException("One or more projects failed to update.", failures);
            }
        }
    }
}
