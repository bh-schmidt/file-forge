namespace FileForge.Maps
{
    public class FileMap
    {
        public string Path { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? FileExists { get; set; } = null!;
        public string? Condition { get; set; }

        public FolderMap? Parent { get; set; }
    }
}
