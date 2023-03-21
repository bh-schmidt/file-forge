using FileForge.Maps;
using System.Text;
using System.Text.RegularExpressions;

namespace FileForge.Setup
{
    public class PathVariableInjector
    {
        private static readonly Regex variableRegex = new("[$][(]([^()]+)[)]", RegexOptions.Multiline | RegexOptions.RightToLeft | RegexOptions.Compiled);

        public static string InjectVariables(string relativePath, FolderMap folder)
        {
            var matches = variableRegex.Matches(relativePath);
            if (!matches.Any())
                return relativePath;

            var builder = new StringBuilder(relativePath);

            foreach (var match in matches.DistinctBy(m => m.Groups[1].Value))
            {
                var pattern = match.Captures[0].Value;
                var variableName = match.Groups[1].Value;
                var variable = folder.GetVariable(variableName);

                if (variable is null || variable is not string stringVariable)
                    continue;

                if (stringVariable.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
                    throw new Exception(); // to do

                builder.Replace(pattern, stringVariable);
            }

            return builder.ToString();
        }
    }
}
