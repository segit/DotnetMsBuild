using System;
using System.Collections.Generic;
using System.Text;

namespace MsBuildUtils
{
    public class NugetPmSlnRefCentralizer
    {
        // TODO: 
        // implement the method using utility from NugetPmSlnRefCentralizer.cs
        // then implement test of the method in
        // MsBuildUtils.Test\NugetPmSlnRefCentralyzer.cs

        /// <summary>
        /// Removes Version attribute from all PackageReference elements in all project files
        /// references by the solution. Ensures PackageVersion element exists in in Directory.Packages.props. 
        /// </summary>
        /// <param name="sln">The file path of the solution whose version files are to be moved. Cannot be null or empty.</param>
        public void MovePackageVersionsToCentralStore(string sln)
        {

        }
    }
}
