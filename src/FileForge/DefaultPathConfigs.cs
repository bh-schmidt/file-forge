using FileForge.Constants;
using System.Text.RegularExpressions;

namespace FileForge
{
    public class DefaultPathConfigs
    {
        private static readonly List<TemplateConfig.PathConfig> _defaults = new();

        static DefaultPathConfigs()
        {
            var processFiles = new[]
            {
                @"_template-config.json"
            };

            var ignoresFolders = new[] {
                @".git",
                @"node_modules",
                @"vs",
                @"bin",
                @"obj"
            };

            foreach (var processFile in processFiles)
                AddIgnored(GetFileRegex(processFile), PathAction.Process);

            foreach (var ignoredFolder in ignoresFolders)
                AddIgnored(GetFolderRegex(ignoredFolder), PathAction.Ignore);
        }

        public static TemplateConfig.PathConfig? GetDefault(string path)
        {
            return _defaults.FirstOrDefault(e => e.Regex.IsMatch(path));
        }

        private static string GetFileRegex(string fileName)
        {
            var regex = fileName.Replace(".", "[.]");
            return $@"^^(.*[\/\\])?{regex}$";
        }

        private static string GetFolderRegex(string folderName)
        {
            var regex = folderName.Replace(".", "[.]");
            return $@"^(.*[\/\\])?{regex}([\/\\].*)?$";
        }

        private static void AddIgnored(string pattern, PathAction action)
        {
            var config = new TemplateConfig.PathConfig()
            {
                Pattern = pattern,
                Action = action,
                Condition = null,
                Regex = new Regex(pattern)
            };

            _defaults.Add(config);
        }
    }
}
