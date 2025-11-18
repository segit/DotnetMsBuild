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
    public void CanUpdateDotnetVersion()
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
}
