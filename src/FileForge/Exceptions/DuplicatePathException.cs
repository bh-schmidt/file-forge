namespace FileForge.Exceptions
{
    public class DuplicatePathException : Exception
    {
        public DuplicatePathException(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
