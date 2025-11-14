using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyRedirectRemover
{
    internal static class AssemblyRedirectVersionChanger
    {
        // TODO:
        /// <summary>
        /// Modifies the assembly binding redirect in the specified configuration file.
        /// Example:
        ///     Original Redirect:
        ///           <dependentAssembly>
        ///             <assemblyIdentity name = "System.ValueTuple" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        ///             <bindingRedirect oldVersion = "0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
        ///           </dependentAssembly>
        ///     Resulting Redirect
        ///           <dependentAssembly>
        ///             <assemblyIdentity name = "System.ValueTuple" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        ///             <bindingRedirect oldVersion = "0.0.0.0-4.0.5.0" newVersion="4.0.0.0" />
        ///           </dependentAssembly>
        ///     Parameters:
        ///         assemblyShortName = "System.ValueTuple"
        ///         range = "0.0.0.0-4.0.5.0"
        ///         newVersion = "4.0.0.0"
        /// </summary>
        /// <remarks>This method updates the assembly binding redirect in the configuration file located
        /// at the specified path. Ensure that the file is accessible and writable before calling this method.</remarks>
        /// <param name="path">The file path to the configuration file where the assembly redirect will be changed.</param>
        /// <param name="assemblyShortName">The short name of the assembly for which the redirect is being modified.</param>
        /// <param name="range">The version range that the redirect applies to.</param>
        /// <param name="redirect">The new version to which the assembly should be redirected.</param>
        internal static void ChangeAssemblyRedirect(this string path, string assemblyShortName, string range, string redirect)
        {
        }
    }
}
