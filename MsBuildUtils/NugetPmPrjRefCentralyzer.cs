using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MsBuildUtils
{
    public class NugetPmPrjRefCentralyzer
    {
        /// <summary>
        /// Prepares project to switch to Centralized Package Management (CPM)
        /// https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management
        /// 
        /// Given the path to the csProj project file and 
        /// Directory.Packages.props file
        /// it loops through all PackageReference elements present in project file.
        /// Fore each PackageReference it adds PackageVersion element
        /// to the Directory.Packages.props file if the one does not exists yet.
        /// Then it removes the Version attribute from the PackageReverence element
        /// in a  project file.
        /// 
        /// Examples of elements
        ///     PackageReference element:
        ///         <PackageReference Include="SomeRandomPackage" Version="X.X.XX" />
        ///     PackageVersion elemement:
        ///         <PackageVersion Include="SomeRandomPackage" Version="X.X.XX" />
        /// </summary>
        /// <param name="csProj">Path to MsBuild SDK style .csproj file</param>
        /// <param name="packagesProps">Path MsBuild Directory.Packages.props</param>
        /// <returns>True if any PackageVersion elements were added, false otherwise</returns>
        public static bool MoveVersionsToCentralStore(string csProj, string packagesProps)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(csProj);
            ArgumentException.ThrowIfNullOrWhiteSpace(packagesProps);

            if (!File.Exists(csProj))
                throw new FileNotFoundException($"Project file not found.", csProj);

            if (!File.Exists(packagesProps))
                throw new FileNotFoundException($"Directory.Packages.props file not found.", packagesProps);

            try
            {
                var projDoc = XDocument.Load(csProj);
                var propsDoc = XDocument.Load(packagesProps);

                var projNs = projDoc.Root?.Name.Namespace ?? XNamespace.None;
                var propsNs = propsDoc.Root?.Name.Namespace ?? XNamespace.None;

                var packageReferences = projDoc.Descendants(projNs + "PackageReference")
                    .Where(pr => pr.Attribute("Include") != null && pr.Attribute("Version") != null)
                    .ToList();

                if (packageReferences.Count == 0)
                    return false;

                var propsItemGroup = propsDoc.Descendants(propsNs + "ItemGroup").FirstOrDefault();
                if (propsItemGroup == null)
                {
                    propsItemGroup = new XElement(propsNs + "ItemGroup");
                    propsDoc.Root?.Add(propsItemGroup);
                }

                var existingPackageVersions = propsItemGroup.Elements(propsNs + "PackageVersion")
                    .Select(pv => pv.Attribute("Include")?.Value)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                bool added = false;
                bool removed = false;
                foreach (var packageRef in packageReferences)
                {
                    var packageName = packageRef.Attribute("Include")?.Value;
                    var version = packageRef.Attribute("Version")?.Value;

                    if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(version))
                        continue;

                    if (!existingPackageVersions.Contains(packageName))
                    {
                        var packageVersion = new XElement(propsNs + "PackageVersion");
                        packageVersion.SetAttributeValue("Include", packageName);
                        packageVersion.SetAttributeValue("Version", version);
                        propsItemGroup.Add(packageVersion);
                        existingPackageVersions.Add(packageName);
                        added = true;
                    }

                    // Remove the Version attribute from the PackageReference element
                    packageRef.Attribute("Version")?.Remove();
                    removed = true;
                }

                if (added)
                {
                    propsDoc.Save(packagesProps);
                }
                if (removed)
                {
                    projDoc.Save(csProj);
                }

                return added || removed;
            }
            catch (Exception ex) when (!(ex is ArgumentException) && !(ex is FileNotFoundException))
            {
                throw new InvalidOperationException($"Failed to process package references: {ex.Message}", ex);
            }
        }
    }
}
