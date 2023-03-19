namespace FileForge.Exceptions
{
    public class InvalidTemplateDirectoryException : Exception
    {
        public InvalidTemplateDirectoryException(string? directory) : base($"The template directory '{directory}' is invalid.")
        {
        }
    }
}
