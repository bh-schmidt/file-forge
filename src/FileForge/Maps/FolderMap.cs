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
        public Dictionary<string, VariableMap> Variables { get; set; } = new();

        public bool VariableExists(string name)
        {
            if (Variables.ContainsKey(name))
                return true;

            if (Parent is null)
                return false;

            return Parent.VariableExists(name);
        }

        public object? GetVariable(string name)
        {
            return Variables.GetValueOrDefault(name)?.Answer ?? Parent?.GetVariable(name);
        }

        public IEnumerable<VariableMap> GetVariables()
        {
            return Variables
                .Select(e => e.Value)
                .Concat(Parent?.GetVariables() ?? Enumerable.Empty<VariableMap>());
        }

        public IEnumerable<VariableMap> GetAnsweredVariables()
        {
            return GetVariables().Where(e => e.Answer is not null);
        }
    }
}
