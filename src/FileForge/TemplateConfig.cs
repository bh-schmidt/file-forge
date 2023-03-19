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
            public string Name { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string Type { get; set; } = VariableTypes.Default;
            public string[]? Dependencies { get; set; }
            public string? Condition { get; set; }
            public string? Required { get; set; }
            public string[]? Options { get; set; }
        }

        public class PathConfig
        {
            private static readonly char[] illegalCharacters = new[] { '@', '%', '&', ':', '"', '\'', '<', '>', '|', '~', '`', '#', '^', '+', '=', '{', '}', '[', ']', ';', '!', };
            private string pattern = null!;
            public string Action { get; set; } = PathActions.Default;
            public string FileExists { get; set; } = FileExistsActions.Default;
            public string FolderExists { get; set; } = FolderExistsActions.Default;
            public string? Condition { get; set; }

            public Regex Regex { get; private set; } = null!;
            public string Pattern
            {
                get
                {
                    return pattern;
                }
                set
                {
                    if (string.IsNullOrEmpty(value))
                        throw new Exception(); //to do

                    if (value.Any(c => illegalCharacters.Contains(c)))
                        throw new Exception();//to do

                    var compiledPattern = new StringBuilder(Directory.GetCurrentDirectory())
                        .Append(Path.DirectorySeparatorChar)
                        .Append(value)
                        .Replace(@"\", @"[\]")
                        .Replace("/", @"[\/]")
                        .Replace("*", @"[^.\/]*")
                        .Replace("?", @".{1}")
                        .Replace("$", @"[$]")
                        .ToString();

                    pattern = value;
                    Regex = new Regex(compiledPattern);
                }
            }
        }
    }
}
