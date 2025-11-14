using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyRedirectRemover
{
    internal static class ConfigFilesTraverser
    {
        /// <summary>
        /// GetConfigFiles loop through directory
        /// and return all "*app.config*" and "*web.config*" files 
        /// excluding the ones in bin or obj directories.
        /// .git .vs .vscode directories should also be skipped
        /// TODO:
        /// add two specific files to the list 
        /// Fusion.One\Shell\Telexy.Fusion.Shell\Config\Telexy.Fusion.App.Deployer.exe.config
        /// Fusion.One\Shell\Telexy.Fusion.Shell\WebConfig.Master.template
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Task<IEnumerable<string>> GetConfigFiles(this string path)
        {
            var excludedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                    "bin", "obj", ".git", ".vs", ".vscode"
            };

            var result = GetConfigFilesWithSearchOption(path, excludedDirectories);
            return Task.FromResult(result);
        }

        private static IEnumerable<string> GetConfigFilesWithSearchOption(string directoryPath, HashSet<string> excludedDirectories)
        {
            var configFiles = new List<string>();

            try
            {
                // Check if directory exists
                if (!Directory.Exists(directoryPath))
                    return configFiles;

                // Get all config files using SearchOption.AllDirectories
                var allConfigFiles = Directory.GetFiles(directoryPath, "*config*", SearchOption.AllDirectories)
                .Where(file =>
                {
                    var fileName = Path.GetFileName(file).ToLowerInvariant();
                    return fileName.Contains("app.config", StringComparison.OrdinalIgnoreCase)
                    || fileName.Contains("web.config", StringComparison.OrdinalIgnoreCase);
                })
                  .Where(file =>
                  {
                      // Filter out files in excluded directories
                      var pathParts = Path.GetDirectoryName(file)?.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? Array.Empty<string>();
                      return !pathParts.Any(part => excludedDirectories.Contains(part));
                  });

                configFiles.AddRange(allConfigFiles);
            }
            catch
            {
                throw;
            }

            return configFiles;
        }
    }
}
