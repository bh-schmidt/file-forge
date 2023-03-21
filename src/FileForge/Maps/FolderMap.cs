namespace FileForge.Maps
{
    public class FolderMap
    {
        public string Path { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? FolderExists { get; set; } = null!;
        public string? Condition { get; set; }

        public FolderMap? Parent { get; set; }
        public TemplateConfig? TemplateConfig { get; set; }
        public Dictionary<string, FolderMap> Folders { get; set; } = new();
        public Dictionary<string, FileMap> Files { get; set; } = new();
        public Dictionary<string, VariableMap> Variables { get; set; } = new();
        public Dictionary<string, object?> VariableValues { get; set; } = new();

        public bool VariableExists(string name)
        {
            if (Parent is null)
                return false;

            return Variables.ContainsKey(name) || Parent.VariableExists(name);
        }
    }
}
