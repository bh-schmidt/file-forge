namespace FileForge.Exceptions
{
    public class InvalidPatternException : Exception
    {
        public InvalidPatternException(string? pattern)
        {
            Pattern = pattern;
        }

        public string? Pattern { get; }
    }
}
