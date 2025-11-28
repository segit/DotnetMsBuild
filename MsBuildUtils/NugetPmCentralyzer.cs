using System;
using System.Collections.Generic;
using System.Text;

namespace MsBuildUtils
{
    public class NugetPmCentralyzer
    {
        // TODO:
        // Implement the method as described.
        // Then implement the test case as described in TODO section 
        // in the 
        // MsBuildUtils.Test\NuGetPackagesVersioningCentralizerTest.cs


        /// <summary>
        /// Prepares project to switch to Centralized Package Management (CPM)
        /// https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management
        /// 
        /// Given the path to the csProj project file and 
        /// Directory.Packages.props file
        /// it loops through all PackageReference elements present in project file.
        /// Fore each PackageReference it adds PackageVersion element
        /// to the Directory.Packages.props file if the one does not exists yet.
        /// Examples of elements
        ///     PackageReference element:
        ///         <PackageReference Include="SomeRandomPackage" Version="X.X.XX" />
        ///     PackageVersion elemement:
        ///         <PackageVersion Include="SomeRandomPackage" Version="X.X.XX" />
        /// </summary>
        /// <param name="csProj">Path to MsBuild SDK style .csproj file</param>
        /// <param name="packagesProps">Path MsBuild Directory.Packages.props</param>
        /// <returns></returns>
        public static bool MoveNugetPackageVersionsToDirectoryPackagetPropsProps(string csProj, string packagesProps)
        {
            throw new NotImplementedException();
        }
    }
}
