using FileForge.Constants;

namespace FileForge.Maps
{
    public class ParameterMap
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ParameterType Type { get; set; } = null!;
        public string[]? Dependencies { get; set; }
        public string? Condition { get; set; }
        public string? Required { get; set; }
        public string[]? Options { get; set; }

        public object? Value { get; set; }
    }
}
