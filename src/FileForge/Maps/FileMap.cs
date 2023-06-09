using FileForge.Constants;

namespace FileForge.Maps
{
    public class FileMap
    {
        public string Path { get; set; } = null!;
        public PathAction Action { get; set; } = null!;
        public FileExistsAction? FileExists { get; set; }
        public string? Condition { get; set; }

        public FolderMap? Parent { get; set; }

        public IEnumerable<ParameterMap> GetParameters()
        {
            return Parent?.GetParameters() ?? Enumerable.Empty<ParameterMap>();
        }
    }
}
