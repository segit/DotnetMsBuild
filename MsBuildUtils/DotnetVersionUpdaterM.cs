using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace MsBuildUtils
{
    public static class DotnetVersionUpdaterM
    {
        //TODO: fix the method to match updated requirements
        /// <summary>
        /// Updates the .NET version in the specified project file to the given version.
        /// It updates only version <TargetFramework></TargetFramework> tabs if the current version
        /// matches any of the specified old versions.
        /// It ignores <TargetFrameworks></TargetFrameworks> tabs.
        /// If the change was made the method return true, otherwise false.
        /// </summary>
        /// <remarks>This method modifies the project file in place. Ensure that the specified file is
        /// writable and that the provided .NET version string is compatible with the project.</remarks>
        /// <param name="projFile">The path to the project file to be updated. Must not be null or empty.</param>
        /// <param name="newVersion">The new .NET version to set in the project file. Must be a valid .NET version string.</param>
        public static bool UpdateDotnetVerisonTo(string projFile, string[] oldVersions, string newVersion)
        {
            if (string.IsNullOrWhiteSpace(projFile))
                throw new ArgumentException("Project file path must be provided.", nameof(projFile));

            if (oldVersions == null || oldVersions.Length == 0)
                throw new ArgumentException("At least one old version must be provided.", nameof(oldVersions));

            if (string.IsNullOrWhiteSpace(newVersion))
                throw new ArgumentException("New version must be provided.", nameof(newVersion));

            if (!File.Exists(projFile))
                throw new FileNotFoundException("Project file not found.", projFile);

            var normalizedNew = VersionNormalizer.Normalize(newVersion);
            var normalizedOld = oldVersions.Select(VersionNormalizer.Normalize)
                                           .Where(x => !string.IsNullOrEmpty(x))
                                           .Select(x => x.ToLowerInvariant())
                                           .ToHashSet();

            try
            {
                var doc = XDocument.Load(projFile);
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                var targetFrameworkElements = doc.Descendants(ns + "TargetFramework").ToList();

                var changed = false;

                foreach (var tf in targetFrameworkElements)
                {
                    var val = (tf.Value ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(val))
                        continue;

                    var norm = VersionNormalizer.Normalize(val).ToLowerInvariant();
                    if (normalizedOld.Contains(norm))
                    {
                        tf.Value = normalizedNew;
                        changed = true;
                    }
                }

                if (changed)
                {
                    doc.Save(projFile);
                    return true;
                }
                else
                {
                    // Per updated requirements, do not throw when nothing was changed — return false.
                    return false;
                }
            }
            catch (Exception ex) when (!(ex is ArgumentException) && !(ex is FileNotFoundException))
            {
                throw new InvalidOperationException($"Failed to update project file '{projFile}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loops through all projects in the solution and uses DotnetVersionUpdaterM.UpdateDotnetVersionTo function
        /// to update project versions.
        /// </summary>
        /// <param name="slnxFile"></param>
        /// <param name="newVersion"></param>
        public static void UpdateAllDotnetVersionsTo(string slnxFile, string[] oldVersions, string newVersion)
        {
            if (string.IsNullOrWhiteSpace(slnxFile))
                throw new ArgumentException("Solution file path must be provided.", nameof(slnxFile));

            if (oldVersions == null || oldVersions.Length == 0)
                throw new ArgumentException("At least one old version must be provided.", nameof(oldVersions));

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
                    // Call updater and ignore cases where nothing was changed (returns false).
                    UpdateDotnetVerisonTo(combined, oldVersions, newVersion);
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
