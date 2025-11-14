using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

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
            if (!File.Exists(path))
                return;

            if (string.IsNullOrWhiteSpace(assemblyShortName))
                throw new ArgumentException("Assembly short name cannot be null or empty.", nameof(assemblyShortName));

            if (string.IsNullOrWhiteSpace(range))
                throw new ArgumentException("Version range cannot be null or empty.", nameof(range));

            if (string.IsNullOrWhiteSpace(redirect))
                throw new ArgumentException("New version cannot be null or empty.", nameof(redirect));

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                // Add namespace manager for XML namespace handling
                var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                namespaceManager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                // Find the runtime/assemblyBinding node
                var assemblyBindingNode = xmlDoc.SelectSingleNode("//runtime/asm:assemblyBinding", namespaceManager)
                    ?? xmlDoc.SelectSingleNode("//runtime/assemblyBinding");

                if (assemblyBindingNode == null)
                {
                    // Assembly binding section not found - we could create it, but for now just return
                    return;
                }

                // Find dependentAssembly nodes that match the specified assembly name
                var dependentAssemblyNodes = assemblyBindingNode.SelectNodes("asm:dependentAssembly", namespaceManager)
                    ?? assemblyBindingNode.SelectNodes("dependentAssembly");

                if (dependentAssemblyNodes != null)
                {
                    foreach (XmlNode dependentAssemblyNode in dependentAssemblyNodes)
                    {
                        // Find the assemblyIdentity node within this dependentAssembly
                        var assemblyIdentityNode = dependentAssemblyNode.SelectSingleNode("asm:assemblyIdentity", namespaceManager)
                            ?? dependentAssemblyNode.SelectSingleNode("assemblyIdentity");

                        if (assemblyIdentityNode?.Attributes?["name"]?.Value?.Equals(assemblyShortName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            // Found the matching assembly, now find the bindingRedirect node
                            var bindingRedirectNode = dependentAssemblyNode.SelectSingleNode("asm:bindingRedirect", namespaceManager)
                                ?? dependentAssemblyNode.SelectSingleNode("bindingRedirect");

                            if (bindingRedirectNode != null)
                            {
                                // Update the existing binding redirect
                                if (bindingRedirectNode.Attributes?["oldVersion"] != null)
                                {
                                    bindingRedirectNode.Attributes["oldVersion"].Value = range;
                                }

                                if (bindingRedirectNode.Attributes?["newVersion"] != null)
                                {
                                    bindingRedirectNode.Attributes["newVersion"]!.Value = redirect;
                                }
                            }
                            else
                            {
                                //// Create a new binding redirect node if it doesn't exist
                                //bindingRedirectNode = xmlDoc.CreateElement("bindingRedirect", assemblyBindingNode.NamespaceURI);

                                //var oldVersionAttr = xmlDoc.CreateAttribute("oldVersion");
                                //oldVersionAttr.Value = range;
                                //bindingRedirectNode.Attributes.Append(oldVersionAttr);

                                //var newVersionAttr = xmlDoc.CreateAttribute("newVersion");
                                //newVersionAttr.Value = redirect;
                                //bindingRedirectNode.Attributes.Append(newVersionAttr);

                                //dependentAssemblyNode.AppendChild(bindingRedirectNode);
                            }

                            // Found and modified the assembly, we can break out of the loop
                            break;
                        }
                    }
                }

                // Save the modified document
                xmlDoc.Save(path);
            }
            catch (Exception ex)
            {
                // Log the error or rethrow based on your error handling strategy
                throw new InvalidOperationException($"Failed to change assembly redirect for '{assemblyShortName}' in {path}: {ex.Message}", ex);
            }
        }
    }
}
