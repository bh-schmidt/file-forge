using FileForge.Constants;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace FileForge
{
    public class TemplateConfig
    {
        public const string FileName = "_template-config.json";
        public IEnumerable<VariableConfig> Variables { get; set; } = null!;
        public IEnumerable<PathConfig> Paths { get; set; } = null!;

        public static TemplateConfig? ReadTemplateConfig(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using var file = File.OpenText(filePath);
            var serializer = new JsonSerializer();
            return (TemplateConfig?)serializer.Deserialize(file, typeof(TemplateConfig));
        }

        public static TemplateConfig? GetTemplateConfig(string folderPath)
        {
            var filePath = Path.Combine(folderPath, FileName);
            if (!File.Exists(filePath))
                return null;

            using var file = File.OpenText(filePath);
            var serializer = new JsonSerializer();
            return (TemplateConfig?)serializer.Deserialize(file, typeof(TemplateConfig));
        }

        public class VariableConfig
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

            public Regex GetRegex(string templatePath)
            {
                if (string.IsNullOrEmpty(Pattern))
                    throw new Exception(); //to do

                if (Pattern.Any(c => illegalCharacters.Contains(c)))
                    throw new Exception();//to do

                // change to template directory
                var compiledPattern = new StringBuilder("^")
                    .Append(templatePath)
                    .Append(Path.DirectorySeparatorChar)
                    .Append(Pattern)
                    .Replace(@"**/", @"++")
                    .Replace(@"**\", @"++")
                    .Replace(@".", @"[.]")
                    .Replace(@"\", @"/")
                    .Replace(@"/", @"([\\]|[\/])")
                    .Replace(@"*", @"[^\\\/]*")
                    .Replace(@"++", @".*")
                    .Replace(@"?", @".{1}")
                    .Replace(@"$", @"[$]")
                    .Append('$')
                    .ToString();

                return new Regex(compiledPattern);
            }
        }
    }
}
