namespace FileForge.Exceptions
{
    public class InvalidFieldException : Exception
    {
        public InvalidFieldException(string fieldName, string? fieldValue) : base($"The '{fieldName}' don't accept the value '{fieldValue}'")
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }

        public InvalidFieldException(string fieldName, IEnumerable<string?>? fieldValues) : base($"The '{fieldName}' don't accept the values ({string.Join(", ", fieldValues ?? Array.Empty<string>())})")
        {
            FieldName = fieldName;
            FieldValues = fieldValues;
        }

        public string FieldName { get; }
        public string? FieldValue { get; }
        public IEnumerable<string?>? FieldValues { get; }
    }
}
