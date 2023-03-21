namespace FileForge.Maps
{
    public class VariableMap
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string[]? Dependencies { get; set; }
        public string? Condition { get; set; }
        public string? Required { get; set; }
        public string[]? Options { get; set; }

        public object? Answer { get; set; }
    }
}
