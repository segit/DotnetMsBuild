using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace MsBuildUtils.Test
{
    public class DotnetVersionUpdaterMTest
    {
        /// <summary>
        /// Uses project files in SampleData folder
        /// to test DotnetVersionUpdaterM
        /// with the follwing parameters
        /// case1:
        ///     oldVersions: {"net8.0"} newVersion "net10.0" 
        /// case2:
        ///     oldVersions: {"net9.0"} newVersion "net10.0" 
        /// case3:
        ///     oldVersions: {"net8.0", "net9.0"} newVersion "net10.0"
        /// </summary>
        [Fact]
        public void CanUpdateProjectDotnetVersion()
        {
            string ver = "net10.0";
            var sample = Path.GetFullPath("SampleData/Ase.Cmd/Ase.Cmd.csproj");
            Assert.True(File.Exists(sample), $"Sample project not found: {sample}");

            // case 1: no match -> expect false (no change)
            var tmp1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csproj");
            File.Copy(sample, tmp1);
            try
            {
                var result = MsBuildUtils.DotnetVersionUpdaterM.UpdateDotnetVerisonTo(tmp1, new[] { "net8.0" }, ver);
                Assert.False(result);
            }
            finally
            {
                try { File.Delete(tmp1); } catch { }
            }

            // case 2: single old version matching current (net9.0) -> update
            var tmp2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csproj");
            File.Copy(sample, tmp2);
            try
            {
                var result = MsBuildUtils.DotnetVersionUpdaterM.UpdateDotnetVerisonTo(tmp2, new[] { "net9.0" }, ver);
                Assert.True(result);

                var doc = XDocument.Load(tmp2);
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                var tf = doc.Descendants(ns + "TargetFramework").FirstOrDefault();
                Assert.NotNull(tf);
                Assert.Equal(ver, tf.Value);
            }
            finally
            {
                try { File.Delete(tmp2); } catch { }
            }

            // case 3: multiple old versions including current -> update
            var tmp3 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csproj");
            File.Copy(sample, tmp3);
            try
            {
                var result = MsBuildUtils.DotnetVersionUpdaterM.UpdateDotnetVerisonTo(tmp3, new[] { "net8.0", "net9.0" }, ver);
                Assert.True(result);

                var doc = XDocument.Load(tmp3);
                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                var tf = doc.Descendants(ns + "TargetFramework").FirstOrDefault();
                Assert.NotNull(tf);
                Assert.Equal(ver, tf.Value);
            }
            finally
            {
                try { File.Delete(tmp3); } catch { }
            }
        }

        /// <summary>
        /// Uses SampleData/Ase.WebApi.slnx file
        /// to test DotnetVersionUpdaterM.UpdateAllDotnetVersionsTo
        /// with the follwing parameters
        /// case1:
        ///     oldVersions: {"net8.0"} newVersion "net10.0"
        /// case2:
        ///     oldVersions: {"net9.0"} newVersion "net10.0"
        /// case3:
        ///     oldVersions: {"net8.0", "net9.0"} newVersion "net10.0"
        /// </summary>
        [Fact]
        public void CanUpdateSlnxDotnetVersion()
        {
            string ver = "net10.0";
            var sampleSln = Path.GetFullPath("SampleData/Ase.WebApi.slnx");
            Assert.True(File.Exists(sampleSln), $"Sample solution not found: {sampleSln}");

            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tmpDir);
            var tmpSln = Path.Combine(tmpDir, "Ase.WebApi.slnx");
            File.Copy(sampleSln, tmpSln);

            // Only copy a couple of project files referenced by the solution so the updater
            // has some files to operate on.
            var filesToCopy = new[] {
                "Ase.Cmd/Ase.Cmd.csproj",
                "Ase.Infrastructure/Ase.Infrastructure.csproj"
            };

            try
            {
                var sampleDir = Path.GetDirectoryName(sampleSln) ?? string.Empty;

                foreach (var rel in filesToCopy)
                {
                    var src = Path.GetFullPath(Path.Combine(sampleDir, rel));
                    var dest = Path.Combine(tmpDir, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? tmpDir);
                    File.Copy(src, dest);
                }

                // case 1: no old versions match -> expect no change
                MsBuildUtils.DotnetVersionUpdaterM.UpdateAllDotnetVersionsTo(tmpSln, new[] { "net8.0" }, ver);
                // Assert that TargetFramework is still net9.0 in all files
                foreach (var rel in filesToCopy)
                {
                    var dest = Path.Combine(tmpDir, rel);
                    var doc = XDocument.Load(dest);
                    var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                    var tf = doc.Descendants(ns + "TargetFramework").FirstOrDefault();
                    Assert.NotNull(tf);
                    Assert.Equal("net9.0", tf.Value);
                }

                // case 2: single old version matching current (net9.0) -> update
                MsBuildUtils.DotnetVersionUpdaterM.UpdateAllDotnetVersionsTo(tmpSln, new[] { "net9.0" }, ver);

                foreach (var rel in filesToCopy)
                {
                    var dest = Path.Combine(tmpDir, rel);
                    var doc = XDocument.Load(dest);
                    var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                    var tf = doc.Descendants(ns + "TargetFramework").FirstOrDefault();
                    Assert.NotNull(tf);
                    Assert.Equal(ver, tf.Value);
                }

                // Revert the copied files back to original sample so next case starts from net9.0 again
                foreach (var rel in filesToCopy)
                {
                    var src = Path.GetFullPath(Path.Combine(sampleDir, rel));
                    var dest = Path.Combine(tmpDir, rel);
                    File.Copy(src, dest, overwrite: true);
                }

                // case 3: multiple old versions including current -> update
                MsBuildUtils.DotnetVersionUpdaterM.UpdateAllDotnetVersionsTo(tmpSln, new[] { "net8.0", "net9.0" }, ver);

                foreach (var rel in filesToCopy)
                {
                    var dest = Path.Combine(tmpDir, rel);
                    var doc = XDocument.Load(dest);
                    var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
                    var tf = doc.Descendants(ns + "TargetFramework").FirstOrDefault();
                    Assert.NotNull(tf);
                    Assert.Equal(ver, tf.Value);
                }
            }
            finally
            {
                try { Directory.Delete(tmpDir, true); } catch { }
            }
        }
    }
}
