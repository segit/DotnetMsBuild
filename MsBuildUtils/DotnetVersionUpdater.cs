using System;
using System.Collections.Generic;
using System.Text;

namespace MsBuildUtils
{
    public static class DotnetVersionUpdater
    {
        // TODO
        /// <summary>
        /// Updates the .NET version in the specified project file to the given version.
        /// </summary>
        /// <remarks>This method modifies the project file in place. Ensure that the specified file is
        /// writable and that the provided .NET version string is compatible with the project.</remarks>
        /// <param name="projFile">The path to the project file to be updated. Must not be null or empty.</param>
        /// <param name="newVersion">The new .NET version to set in the project file. Must be a valid .NET version string.</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void UpdateDotnetVerisonTo(string projFile, string newVersion)
        {
            throw new NotImplementedException();
        }
    }
}
