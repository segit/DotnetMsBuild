using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace MsBuildUtils.Test
{
    public class NugetPmPrjRefCentralyzer
    {
        /// <summary>
        /// Tests the <see cref="MsBuildUtils.NugetPmPrjRefCentralyzer.MoveVersionsToCentralStore"/> method
        /// using sample data files to ensure PackageReferences from the project file
        /// are properly added as PackageVersion elements to Directory.Packages.props
        /// </summary>
        [Fact]
        public void CanMovePrjRefsVersioning()
        {
            var sampleProj = Path.GetFullPath("SampleData/Ase.Cmd/Ase.Cmd.csproj");
            var sampleProps = Path.GetFullPath("SampleData/Directory.Packages.props");

            Assert.True(File.Exists(sampleProj), $"Sample project not found: {sampleProj}");
            Assert.True(File.Exists(sampleProps), $"Sample Directory.Packages.props not found: {sampleProps}");

            var tmpProj = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csproj");
            var tmpProps = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".props");

            File.Copy(sampleProj, tmpProj);
            File.Copy(sampleProps, tmpProps);

            try
            {
                // Load original project to count PackageReferences with Version attributes
                var originalProjDoc = XDocument.Load(tmpProj);
                var originalNs = originalProjDoc.Root?.Name.Namespace ?? XNamespace.None;
                var originalPackageReferencesWithVersion = originalProjDoc.Descendants(originalNs + "PackageReference")
                    .Where(pr => pr.Attribute("Include") != null && pr.Attribute("Version") != null)
                    .ToList();
                var originalCount = originalPackageReferencesWithVersion.Count;

                Assert.True(originalCount > 0, "Sample project should have PackageReferences with Version attributes");

                var result = MsBuildUtils.NugetPmPrjRefCentralyzer.MoveVersionsToCentralStore(tmpProj, tmpProps);

                Assert.True(result, "Expected method to return true indicating packages were added");

                // Verify PackageVersion elements were added to Directory.Packages.props
                var propsDoc = XDocument.Load(tmpProps);
                var ns = propsDoc.Root?.Name.Namespace ?? XNamespace.None;
                var packageVersions = propsDoc.Descendants(ns + "PackageVersion").ToList();

                Assert.NotEmpty(packageVersions);
                Assert.Equal(originalCount, packageVersions.Count);

                // Verify Version attributes were removed from PackageReference elements
                var projDoc = XDocument.Load(tmpProj);
                var projNs = projDoc.Root?.Name.Namespace ?? XNamespace.None;
                var packageReferencesWithVersion = projDoc.Descendants(projNs + "PackageReference")
                    .Where(pr => pr.Attribute("Include") != null && pr.Attribute("Version") != null)
                    .ToList();

                Assert.Empty(packageReferencesWithVersion);

                // Verify all PackageReferences still exist (just without Version attribute)
                var allPackageReferences = projDoc.Descendants(projNs + "PackageReference")
                    .Where(pr => pr.Attribute("Include") != null)
                    .ToList();

                Assert.True(allPackageReferences.Count >= originalCount, "PackageReference elements should still exist");

                // Verify each original package has a corresponding PackageVersion in props file
                foreach (var originalPackageRef in originalPackageReferencesWithVersion)
                {
                    var packageName = originalPackageRef.Attribute("Include")?.Value;
                    var version = originalPackageRef.Attribute("Version")?.Value;

                    var matchingPackageVersion = packageVersions.FirstOrDefault(pv =>
                        pv.Attribute("Include")?.Value == packageName &&
                        pv.Attribute("Version")?.Value == version);

                    Assert.NotNull(matchingPackageVersion);
                }

                var resultSecondCall = MsBuildUtils.NugetPmPrjRefCentralyzer.MoveVersionsToCentralStore(tmpProj, tmpProps);
                Assert.False(resultSecondCall, "Expected method to return false when packages already exist");
            }
            finally
            {
                try { File.Delete(tmpProj); } catch { }
                try { File.Delete(tmpProps); } catch { }
            }
        }
    }
}
