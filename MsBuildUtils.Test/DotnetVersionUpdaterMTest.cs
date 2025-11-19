using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace MsBuildUtils.Test
{
    public class DotnetVersionUpdaterMTest
    {
        // Implements test for DotnetVersionUpdaterM
        /// <summary>
        /// Uses project files in SampleData folder
        /// to test DotnetVersionUpdaterM
        /// with the follwing parameters
        /// case1:
        ///     oldVersions: {"net8.0"} newVersion "net10.0" -> should throw (no match)
        /// case2:
        ///     oldVersions: {"net9.0"} newVersion "net10.0" -> should update
        /// case3:
        ///     oldVersions: {"net8.0", "net9.0"} newVersion "net10.0" -> should update
        /// </summary>
        [Fact]
        public void CanUpdateProjectDotnetVersion()
        {
            string ver = "net10.0";
            var sample = Path.GetFullPath("SampleData/Ase.Cmd/Ase.Cmd.csproj");
            Assert.True(File.Exists(sample), $"Sample project not found: {sample}");

            // case 1: no match -> expect InvalidOperationException
            var tmp1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csproj");
            File.Copy(sample, tmp1);
            try
            {
                var ex = Record.Exception(() => MsBuildUtils.DotnetVersionUpdaterM.UpdateDotnetVerisonTo(tmp1, new[] { "net8.0" }, ver));
                Assert.NotNull(ex);
                Assert.IsType<InvalidOperationException>(ex);
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
                MsBuildUtils.DotnetVersionUpdaterM.UpdateDotnetVerisonTo(tmp2, new[] { "net9.0" }, ver);

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
                MsBuildUtils.DotnetVersionUpdaterM.UpdateDotnetVerisonTo(tmp3, new[] { "net8.0", "net9.0" }, ver);

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
    }
}
