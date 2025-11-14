using System.Xml;

namespace AssemblyRedirectRemover
{
    internal static class AssemblyRedirectsRemover
    {
       
        /// <summary>
        /// Removes assembly redirect 
        /// <dependentAssembly>
        ///   <assemblyIdentity name = "System.ValueTuple" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        ///   <bindingRedirect oldVersion = "0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
        /// </dependentAssembly>
        /// from the file if found.
        /// see sample of the file:
        /// SamplesData/App.Config
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assemblyShortName">assembly short name e.g. System.ValueTuple</param>
        internal static void RemoveAssemblyRedirect(this string path, string assemblyShortName)
        {
            if (!File.Exists(path))
                return;

            if (string.IsNullOrWhiteSpace(assemblyShortName))
                return;

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
                    return; // No assembly binding section found

                // Find dependentAssembly nodes that match the specified assembly name
                var dependentAssemblyNodes = assemblyBindingNode.SelectNodes("asm:dependentAssembly", namespaceManager)
                ?? assemblyBindingNode.SelectNodes("dependentAssembly");
        
                if (dependentAssemblyNodes != null)
                {
                    // Convert to array to avoid modification during iteration
                    var nodesToRemove = new List<XmlNode>();

                    foreach (XmlNode dependentAssemblyNode in dependentAssemblyNodes)
                    {
                        // Find the assemblyIdentity node within this dependentAssembly
                        var assemblyIdentityNode = dependentAssemblyNode.SelectSingleNode("asm:assemblyIdentity", namespaceManager)
                        ?? dependentAssemblyNode.SelectSingleNode("assemblyIdentity");
                        
                        if (assemblyIdentityNode?.Attributes?["name"]?.Value?.Equals(assemblyShortName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            nodesToRemove.Add(dependentAssemblyNode);
                        }
                    }

                    // Remove the matching nodes
                    foreach (XmlNode node in nodesToRemove)
                    {
                        assemblyBindingNode.RemoveChild(node);
                    }
                }

                // If assemblyBinding is now empty, consider removing it and its parent runtime node if it's also empty
                var remainingDependentAssemblies = assemblyBindingNode.SelectNodes("asm:dependentAssembly", namespaceManager)
                ?? assemblyBindingNode.SelectNodes("dependentAssembly");
        
                if (remainingDependentAssemblies == null || remainingDependentAssemblies.Count == 0)
                {
                    // Check if assemblyBinding has any other child nodes besides dependentAssembly
                    bool hasOtherContent = false;
                    foreach (XmlNode childNode in assemblyBindingNode.ChildNodes)
                    {
                        if (childNode.NodeType == XmlNodeType.Element && 
                            !childNode.Name.EndsWith("dependentAssembly", StringComparison.OrdinalIgnoreCase))
                        {
                            hasOtherContent = true;
                            break;
                        }
                    }

                    if (!hasOtherContent && assemblyBindingNode.InnerText.Trim().Length == 0)
                    {
                        var runtimeNode = assemblyBindingNode.ParentNode;
                        runtimeNode?.RemoveChild(assemblyBindingNode);

                        // If runtime node is now empty (only whitespace), remove it too
                        if (runtimeNode != null)
                        {
                            bool runtimeHasOtherContent = false;
                            foreach (XmlNode childNode in runtimeNode.ChildNodes)
                            {
                                if (childNode.NodeType == XmlNodeType.Element)
                                {
                                    runtimeHasOtherContent = true;
                                    break;
                                }
                            }

                            if (!runtimeHasOtherContent && runtimeNode.InnerText.Trim().Length == 0)
                            {
                                runtimeNode.ParentNode?.RemoveChild(runtimeNode);
                            }
                        }
                    }
                }

                // Save the modified document
                xmlDoc.Save(path);
            }
            catch (Exception ex)
            {
                // Log the error or rethrow based on your error handling strategy
                throw new InvalidOperationException($"Failed to remove assembly redirects for '{assemblyShortName}' from {path}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Removes all assembly redirects from the configuration file
        /// </summary>
        /// <param name="path">Path to the configuration file</param>
        internal static void RemoveAllAssemblyRedirects(this string path)
        {
            if (!File.Exists(path))
                return;

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
                    return; // No assembly binding section found

                // Remove all dependentAssembly nodes
                var dependentAssemblyNodes = assemblyBindingNode.SelectNodes("asm:dependentAssembly", namespaceManager)
                ?? assemblyBindingNode.SelectNodes("dependentAssembly");
                if (dependentAssemblyNodes != null)
                {
                    // Convert to array to avoid modification during iteration
                    var nodesToRemove = dependentAssemblyNodes.Cast<XmlNode>().ToArray();

                    foreach (XmlNode node in nodesToRemove)
                    {
                        assemblyBindingNode.RemoveChild(node);
                    }
                }

                // If assemblyBinding is now empty, consider removing it and its parent runtime node if it's also empty
                if (!assemblyBindingNode.HasChildNodes || assemblyBindingNode.InnerText.Trim().Length == 0)
                {
                    var runtimeNode = assemblyBindingNode.ParentNode;
                    runtimeNode?.RemoveChild(assemblyBindingNode);

                    // If runtime node is now empty (only whitespace), remove it too
                    if (runtimeNode != null &&
                        !runtimeNode.HasChildNodes &&
                        runtimeNode.InnerText.Trim().Length == 0)
                    {
                        runtimeNode.ParentNode?.RemoveChild(runtimeNode);
                    }
                }

                // Save the modified document
                xmlDoc.Save(path);
            }
            catch (Exception ex)
            {
                // Log the error or rethrow based on your error handling strategy
                throw new InvalidOperationException($"Failed to remove all assembly redirects from {path}: {ex.Message}", ex);
            }
        }
    }
}
