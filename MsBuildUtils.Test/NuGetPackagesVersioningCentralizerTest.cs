using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace MsBuildUtils.Test
{
    public class NuGetPackagesVersioningCentralizerTest
    {
        /// <summary>
        /// Tests the NugetPmCentralyzer.MoveNugetPackageVersionsToDirectoryPackagePropsProps method
        /// using sample data files to ensure PackageReferences from the project file
        /// are properly added as PackageVersion elements to Directory.Packages.props
        /// </summary>
        [Fact]
        public void CanCentralizePackagesVersioning()
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
                var result = NugetPmCentralyzer.MoveNugetPackageVersionsToDirectoryPackagePropsProps(tmpProj, tmpProps);

                Assert.True(result, "Expected method to return true indicating packages were added");

                var propsDoc = XDocument.Load(tmpProps);
                var ns = propsDoc.Root?.Name.Namespace ?? XNamespace.None;
                var packageVersions = propsDoc.Descendants(ns + "PackageVersion").ToList();

                Assert.NotEmpty(packageVersions);

                var projDoc = XDocument.Load(tmpProj);
                var projNs = projDoc.Root?.Name.Namespace ?? XNamespace.None;
                var packageReferences = projDoc.Descendants(projNs + "PackageReference")
                    .Where(pr => pr.Attribute("Include") != null && pr.Attribute("Version") != null)
                    .ToList();

                Assert.Equal(packageReferences.Count, packageVersions.Count);

                foreach (var packageRef in packageReferences)
                {
                    var packageName = packageRef.Attribute("Include")?.Value;
                    var version = packageRef.Attribute("Version")?.Value;

                    var matchingPackageVersion = packageVersions.FirstOrDefault(pv =>
                        pv.Attribute("Include")?.Value == packageName &&
                        pv.Attribute("Version")?.Value == version);

                    Assert.NotNull(matchingPackageVersion);
                }

                var resultSecondCall = NugetPmCentralyzer.MoveNugetPackageVersionsToDirectoryPackagePropsProps(tmpProj, tmpProps);
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
