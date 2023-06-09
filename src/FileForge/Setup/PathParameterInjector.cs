using FileForge.Maps;
using System.Text;
using System.Text.RegularExpressions;

namespace FileForge.Setup
{
    public class PathParameterInjector
    {
        private static readonly Regex parameterRegex = new("[$][(]([^()]+)[)]", RegexOptions.Multiline | RegexOptions.RightToLeft | RegexOptions.Compiled);

        public static string InjectParameter(string relativePath, FolderMap folder)
        {
            var matches = parameterRegex.Matches(relativePath);
            if (!matches.Any())
                return relativePath;

            var builder = new StringBuilder(relativePath);

            foreach (var match in matches.DistinctBy(m => m.Groups[1].Value))
            {
                var pattern = match.Captures[0].Value;
                var parameterName = match.Groups[1].Value;
                var parameter = folder.GetParameter(parameterName);

                if (parameter is null || parameter is not string stringParameter)
                    continue;

                if (stringParameter.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
                    throw new Exception(); // to do

                builder.Replace(pattern, stringParameter);
            }

            return builder.ToString();
        }
    }
}
