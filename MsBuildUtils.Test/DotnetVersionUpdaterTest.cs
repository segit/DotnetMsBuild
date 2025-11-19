using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace MsBuildUtils.Test;



public class DotnetVersionUpdaterTest
{
    /// <summary>
    /// Uses SampleData/Ase.Cmd/Ase.Cmd.csproj file
    /// to test DotnetVersionUpdater
    /// </summary>
    [Fact]
    public void CanUpdateProjectDotnetVersion()
    {
        string ver = "net10.0";
        var sample = Path.GetFullPath("SampleData/Ase.Cmd/Ase.Cmd.csproj");
        Assert.True(File.Exists(sample), $"Sample project not found: {sample}");

        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csproj");
        File.Copy(sample, tmp);

        try
        {
            // Update to version 10 (normalizes to "net10")
            MsBuildUtils.DotnetVersionUpdater.UpdateDotnetVerisonTo(tmp, ver);

            var doc = XDocument.Load(tmp);
            var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
            var tf = doc.Descendants(ns + "TargetFramework").FirstOrDefault();
            Assert.NotNull(tf);
            Assert.Equal(ver, tf.Value);
        }
        finally
        {
            try { File.Delete(tmp); } catch { }
        }
    }

    /// <summary>
    /// Uses SampleData/Ase.WebApi.slnx file
    /// to test DotnetVersionUpdater.UpdateAllDotnetVersionsTo
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

            MsBuildUtils.DotnetVersionUpdater.UpdateAllDotnetVersionsTo(tmpSln, ver);

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
