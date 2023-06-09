using FileForge.Constants;

namespace FileForge.Maps
{
    public class FolderMap
    {
        public string Path { get; set; } = null!;
        public PathAction Action { get; set; } = null!;
        public FolderExistsAction? FolderExists { get; set; } = null!;
        public string? Condition { get; set; }

        public FolderMap? Parent { get; set; }
        public TemplateConfig? TemplateConfig { get; set; }
        public Dictionary<string, FolderMap> Folders { get; set; } = new();
        public Dictionary<string, FileMap> Files { get; set; } = new();
        public Dictionary<string, ParameterMap> Parameters { get; set; } = new();

        public bool ParameterExists(string name)
        {
            if (Parameters.ContainsKey(name))
                return true;

            if (Parent is null)
                return false;

            return Parent.ParameterExists(name);
        }

        public object? GetParameter(string name)
        {
            return Parameters.GetValueOrDefault(name)?.Value ?? Parent?.GetParameter(name);
        }

        public IEnumerable<ParameterMap> GetParameters()
        {
            return Parameters
                .Select(e => e.Value)
                .Concat(Parent?.GetParameters() ?? Enumerable.Empty<ParameterMap>());
        }

        public IEnumerable<ParameterMap> GetAnsweredParameters()
        {
            return GetParameters().Where(e => e.Value is not null);
        }
    }
}
