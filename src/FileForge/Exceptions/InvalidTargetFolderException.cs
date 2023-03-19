namespace FileForge.Exceptions
{
    public class InvalidTargetFolderException : Exception
    {
        public InvalidTargetFolderException(string? directory) : base($"The target directory '{directory}' is invalid.")
        {
        }
    }
}
