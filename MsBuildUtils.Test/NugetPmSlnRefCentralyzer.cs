using System;
using System.Collections.Generic;
using System.Text;

namespace MsBuildUtils.Test
{
    public class NugetPmSlnRefCentralyzer
    {
        // TODO:
        // use files found in SampleData folder to implement the test 
        // use all available project files for test.

        /// <summary>
        /// Tests the <see cref="MsBuildUtils.NugetPmSlnRefCentralizer.MoveVersionsToCentralStore(string)"/> method
        /// using sample data files to ensure PackageReferences from the project file
        /// are properly added as PackageVersion elements to Directory.Packages.props
        /// </summary>
        [Fact]
        public void CanMoveSlnRefsVersioning()
        {
        }
    }
}
