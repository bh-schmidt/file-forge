namespace FileForge.Exceptions
{
    public class DuplicateValueException : Exception
    {
        public DuplicateValueException(string valueName, string value) : base($"The '{valueName}' is duplicated ({value})")
        {
            ValueName = valueName;
            Value = value;
        }

        public string ValueName { get; }
        public string Value { get; }
    }
}
