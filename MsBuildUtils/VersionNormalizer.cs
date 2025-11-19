using System;
using System.Text.RegularExpressions;

namespace MsBuildUtils
{
    internal static class VersionNormalizer
    {
        /// <summary>
        /// Normalize a version string to an MSBuild TargetFramework moniker-like value.
        /// Examples:
        ///  - "net10" -> "net10"
        ///  - "10" or "10.0" -> "net10"
        ///  - "net10.0" -> "net10.0"
        ///  - other strings are returned as-is trimmed
        /// </summary>
        public static string Normalize(string v)
        {
            if (v is null)
                return string.Empty;

            v = v.Trim();
            if (v.StartsWith("net", StringComparison.OrdinalIgnoreCase))
                return v;

            if (Regex.IsMatch(v, "^\\d+(\\.\\d+)?$"))
            {
                return "net" + v;
            }

            return v;
        }
    }
}
