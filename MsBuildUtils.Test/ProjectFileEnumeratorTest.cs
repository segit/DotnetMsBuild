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
}
