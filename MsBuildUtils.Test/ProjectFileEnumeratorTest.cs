namespace MsBuildUtils.Test;

public class ProjectFileEnumeratorTest
{

    [Fact]
    public void CanEnumerateSlnx()
    {
        var list = ProjectFilesEnumerator.Enumerate("SampleData/Ase.WebApi.slnx");
        var currenDir = Directory.GetCurrentDirectory();
        Assert.Contains("Ase.Abstractions/Ase.Abstractions.csproj", list);
        Assert.Contains("ConsoleApp/ConsoleApp.csproj", list); 
    }

    [Fact]
    public void CanEnumerateSlnxAsFi()
    {
        var list = ProjectFilesEnumerator.EnumerateAsFi("SampleData/Ase.WebApi.slnx");
        var proj1 = list.Where(fi => fi.Name == "Ase.Cmd.csproj").Single();
        Assert.True(proj1.Exists);

        var proj2 = list.Where(fi => fi.Name == "Ase.Ef.Core.csproj.csproj").Single();
        Assert.False(proj1.Exists);
    }
}
