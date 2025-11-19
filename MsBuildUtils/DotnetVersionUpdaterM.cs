using System;
using System.Collections.Generic;
using System.Text;

namespace MsBuildUtils
{
    public static class DotnetVersionUpdaterM
    {
        //TODO
        /// <summary>
        /// Updates the .NET version in the specified project file to the given version.
        /// It updates only version <TargetFramework></TargetFramework> tabs if the current version
        /// matches any of the specified old versions.
        /// It ignores <TargetFrameworks></TargetFrameworks> tabs.
        /// </summary>
        /// <remarks>This method modifies the project file in place. Ensure that the specified file is
        /// writable and that the provided .NET version string is compatible with the project.</remarks>
        /// <param name="projFile">The path to the project file to be updated. Must not be null or empty.</param>
        /// <param name="newVersion">The new .NET version to set in the project file. Must be a valid .NET version string.</param>
        public static void UpdateDotnetVerisonTo(string projFile, string[] oldVersions, string newVersion)
        {
        }
    }
}
