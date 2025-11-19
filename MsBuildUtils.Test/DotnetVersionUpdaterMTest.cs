using System;
using System.Collections.Generic;
using System.Text;

namespace MsBuildUtils.Test
{
    public class DotnetVersionUpdaterMTest
    {
        // TODO
        /// <summary>
        /// Uses project files in SampleData folder
        /// to test DotnetVersionUpdaterM
        /// with the follwing parameters
        /// case1:
        ///     oldVersions: {"net8.0"} newVersion "net10.0" 
        /// case2:
        ///     oldVersions: {"net9.0"} newVersion "net10.0" 
        /// case3:
        ///     oldVersions: {"net8.0", net9.0"} newVersion "net10.0" 
        /// </summary>
        [Fact]
        public void CanUpdateProjectDotnetVersion()
        {
        }
    }
}
