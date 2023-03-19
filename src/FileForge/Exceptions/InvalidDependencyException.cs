namespace FileForge.Exceptions
{
    public class InvalidDependencyException : Exception
    {
        private readonly string variableName;
        private readonly IEnumerable<string> invalidDependencies;

        public InvalidDependencyException(string variableName, IEnumerable<string> invalidDependencies)
        {
            this.variableName = variableName;
            this.invalidDependencies = invalidDependencies;
        }
    }
}
