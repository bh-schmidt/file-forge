using FileForge.Constants;
using FileForge.Exceptions;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace FileForge
{
    public class TemplateConfig
    {
        public const string FileName = "_template-config.json";

        public string FilePath { get; private set; } = null!;
        public IEnumerable<ParameterConfig> Parameters { get; set; } = null!;
        public IEnumerable<PathConfig> Paths { get; set; } = null!;

        private void SetFilePath(string filePath)
        {
            FilePath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(filePath);
            
            foreach (var path in Paths)
                path.SetRegex(directory!);
        }

        public static TemplateConfig? ReadTemplateConfig(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using var file = File.OpenText(filePath);
            var serializer = new JsonSerializer();
            var config = (TemplateConfig?)serializer.Deserialize(file, typeof(TemplateConfig));
            if (config is null)
                throw new InvalidTemplateFileException();

            config.SetFilePath(filePath);

            return config;
        }

        public static TemplateConfig? ReadTemplateConfigByDirectory(string directory)
        {
            var filePath = Path.GetFullPath(Path.Combine(directory, TemplateConfig.FileName));
            return ReadTemplateConfig(filePath);
        }

        public class ParameterConfig
        {
            public string? Name { get; set; } = null!;
            public string? Description { get; set; } = null!;
            public string? Type { get; set; }
            public string[]? Dependencies { get; set; }
            public string? Condition { get; set; }
            public string? Required { get; set; }
            public string[]? Options { get; set; }
        }

        public class PathConfig
        {
            private static readonly char[] illegalCharacters = new[] { '@', '%', '&', ':', '"', '\'', '<', '>', '|', '~', '`', '#', '^', '+', '=', '{', '}', '[', ']', ';', '!', };
            public string? Pattern { get; set; }
            public string? Action { get; set; }
            public string? FileExists { get; set; }
            public string? FolderExists { get; set; }
            public string? Condition { get; set; }

            public Regex Regex { get; set; } = null!;

            public void SetRegex(string directory)
            {
                if (string.IsNullOrEmpty(Pattern))
                    throw new InvalidFieldException("path pattern", Pattern);

                if (Pattern.Any(c => illegalCharacters.Contains(c)))
                    throw new InvalidFieldException("path pattern", Pattern);

                // change to template directory
                var compiledPattern = new StringBuilder("^")
                    .Append(directory)
                    .Append(Path.DirectorySeparatorChar)
                    .Append(Pattern)
                    .Replace(@"\", @"/")
                    .Replace(@"**/", @"$anything$")
                    .Replace(@"/", @"[\\\/]")
                    .Replace(@".", @"[.]")
                    .Replace(@"?", @".{1}")
                    .Replace(@"$anything$", @"(.+[\\\/])?")
                    .Replace(@"*", @"[^\\\/]*")
                    .Replace(@"$", @"[$]")
                    .Append('$')
                    .ToString();

                Regex = new Regex(compiledPattern);
            }
        }
    }
}
