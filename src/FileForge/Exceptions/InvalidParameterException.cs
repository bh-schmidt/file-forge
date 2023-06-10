namespace FileForge.Exceptions
{
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException(string parameter, string value) : base($"The parameter '{parameter}' value ({value}) is invalid")
        {
            Parameter = parameter;
            Value = value;
        }

        public string Parameter { get; }
        public string Value { get; }
    }
}
