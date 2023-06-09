namespace FileForge.Exceptions
{
    public class InvalidDependencyException : Exception
    {
        public InvalidDependencyException(string parameterName, IEnumerable<string> invalidDependencies)
        {
            ParameterName = parameterName;
            InvalidDependencies = invalidDependencies;
        }

        public string ParameterName { get; }
        public IEnumerable<string> InvalidDependencies { get; }
    }
}
