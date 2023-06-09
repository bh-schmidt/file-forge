namespace FileForge.Exceptions
{
    public class InvalidTargetDirectoryException : Exception
    {
        public InvalidTargetDirectoryException(string? directory) : base($"The target directory '{directory}' is invalid.")
        {
        }
    }
}
