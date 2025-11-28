using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MsBuildUtils
{
    public class NugetPmSlnRefCentralizer
    {
        /// <summary>
        /// Removes Version attribute from all PackageReference elements in all project files
        /// references by the solution. Ensures PackageVersion element exists in in Directory.Packages.props. 
        /// </summary>
        /// <param name="sln">The file path of the solution whose version files are to be moved. Cannot be null or empty.</param>
        public void MovePackageVersionsToCentralStore(string sln)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sln);

            if (!File.Exists(sln))
                throw new FileNotFoundException($"Solution file not found.", sln);

            var solutionDir = Path.GetDirectoryName(sln);
            if (string.IsNullOrEmpty(solutionDir))
                throw new InvalidOperationException($"Could not determine solution directory from: {sln}");

            var packagesPropsPath = Path.Combine(solutionDir, "Directory.Packages.props");
            if (!File.Exists(packagesPropsPath))
                throw new FileNotFoundException($"Directory.Packages.props file not found in solution directory.", packagesPropsPath);

            var projectFiles = ProjectFilesEnumerator.EnumerateAsFi(sln);

            foreach (var projectFile in projectFiles)
            {
                if (projectFile.Exists && projectFile.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        NugetPmPrjRefCentralyzer.MoveVersionsToCentralStore(projectFile.FullName, packagesPropsPath);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to process project file {projectFile.FullName}: {ex.Message}", ex);
                    }
                }
            }
        }
    }
}
