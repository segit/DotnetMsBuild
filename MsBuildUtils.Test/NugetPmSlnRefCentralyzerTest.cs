using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace MsBuildUtils.Test
{
    public class NugetPmSlnRefCentralyzerTest
    {
        /// <summary>
        /// Tests the <see cref="MsBuildUtils.NugetPmSlnRefCentralizer.MovePackageVersionsToCentralStore(string)"/> method
        /// using sample data files to ensure PackageReferences from the project file
        /// are properly added as PackageVersion elements to Directory.Packages.props
        /// </summary>
        [Fact]
        public void CanMoveSlnRefsVersioning()
        {
            var sampleSln = Path.GetFullPath("SampleData/Ase.WebApi.slnx");
            var sampleProps = Path.GetFullPath("SampleData/Directory.Packages.props");

            Assert.True(File.Exists(sampleSln), $"Sample solution not found: {sampleSln}");
            Assert.True(File.Exists(sampleProps), $"Sample Directory.Packages.props not found: {sampleProps}");

            // Create temp directory for test
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Copy solution file
                var tmpSln = Path.Combine(tempDir, "Ase.WebApi.slnx");
                File.Copy(sampleSln, tmpSln);

                // Copy Directory.Packages.props
                var tmpProps = Path.Combine(tempDir, "Directory.Packages.props");
                File.Copy(sampleProps, tmpProps);

                // Copy all project files that exist in SampleData
                var sampleDataDir = Path.GetFullPath("SampleData");
                var projectFiles = new[]
                {
                    "Ase.Cmd/Ase.Cmd.csproj",
                    "Ase.Abstractions/Ase.Abstractions.csproj",
                    "Ase.Infrastructure/Ase.Infrastructure.csproj",
                    "Ase.Models/Ase.Models.csproj"
                };

                var copiedProjects = new List<string>();
                var allOriginalPackages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                
                foreach (var projectFile in projectFiles)
                {
                    var sourcePath = Path.Combine(sampleDataDir, projectFile);
                    if (File.Exists(sourcePath))
                    {
                        var targetPath = Path.Combine(tempDir, projectFile);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                        File.Copy(sourcePath, targetPath);
                        copiedProjects.Add(targetPath);
                        
                        // Collect all unique packages from source
                        var projDoc = XDocument.Load(sourcePath);
                        var ns = projDoc.Root?.Name.Namespace ?? XNamespace.None;
                        var packageRefs = projDoc.Descendants(ns + "PackageReference")
                            .Where(pr => pr.Attribute("Include") != null && pr.Attribute("Version") != null);
                        
                        foreach (var pkgRef in packageRefs)
                        {
                            var packageName = pkgRef.Attribute("Include")?.Value;
                            var version = pkgRef.Attribute("Version")?.Value;
                            if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(version))
                            {
                                // Store first occurrence of each package
                                if (!allOriginalPackages.ContainsKey(packageName))
                                {
                                    allOriginalPackages[packageName] = version;
                                }
                            }
                        }
                    }
                }

                Assert.NotEmpty(copiedProjects);
                Assert.True(allOriginalPackages.Count > 0, "Sample projects should have PackageReferences with Version attributes");

                // Execute the method
                var centralizer = new MsBuildUtils.NugetPmSlnRefCentralizer();
                centralizer.MovePackageVersionsToCentralStore(tmpSln);

                // Verify Directory.Packages.props has PackageVersion elements
                var propsDoc = XDocument.Load(tmpProps);
                var propsNs = propsDoc.Root?.Name.Namespace ?? XNamespace.None;
                var packageVersions = propsDoc.Descendants(propsNs + "PackageVersion").ToList();

                Assert.NotEmpty(packageVersions);
                Assert.Equal(allOriginalPackages.Count, packageVersions.Count);

                // Verify all projects no longer have Version attributes in PackageReference elements
                foreach (var projectPath in copiedProjects)
                {
                    var projDoc = XDocument.Load(projectPath);
                    var ns = projDoc.Root?.Name.Namespace ?? XNamespace.None;
                    var packageReferencesWithVersion = projDoc.Descendants(ns + "PackageReference")
                        .Where(pr => pr.Attribute("Include") != null && pr.Attribute("Version") != null)
                        .ToList();

                    Assert.Empty(packageReferencesWithVersion);
                }

                // Verify unique package names (no duplicates)
                var packageNames = packageVersions
                    .Select(pv => pv.Attribute("Include")?.Value)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                Assert.Equal(packageNames.Count, packageNames.Distinct(StringComparer.OrdinalIgnoreCase).Count());
                
                // Verify all original packages are present in Directory.Packages.props
                foreach (var originalPackage in allOriginalPackages)
                {
                    var matchingPackageVersion = packageVersions.FirstOrDefault(pv =>
                        string.Equals(pv.Attribute("Include")?.Value, originalPackage.Key, StringComparison.OrdinalIgnoreCase));
                    
                    Assert.NotNull(matchingPackageVersion);
                }
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}
