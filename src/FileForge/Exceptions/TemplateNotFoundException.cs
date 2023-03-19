namespace FileForge.Exceptions
{
    public class TemplateNotFoundException : Exception
    {
        public TemplateNotFoundException(string directory) : base($"The directory '{directory}' doesn't point to a template.")
        {
        }
    }
}
