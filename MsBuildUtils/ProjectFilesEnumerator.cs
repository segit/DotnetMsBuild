using System.Xml.Linq;

namespace MsBuildUtils;

public static class ProjectFilesEnumerator
{
    /// <summary>
    /// Enumerates the project files within the specified solution.
    /// </summary>
    /// <param name="solution">The path to the solution file in slnx format. Must be a valid file path.</param>
    /// <returns>An enumerable collection of project file paths contained in the solution.</returns>
    public static IEnumerable<string> Enumerate(string solution)
    {
        var doc = XDocument.Load(solution);
        // Find all <Project Path="..." /> elements anywhere in the document
        return doc.Descendants("Project")
                  .Select(e => e.Attribute("Path")?.Value)
                  .Where(path => !string.IsNullOrEmpty(path))!;
    }
}
