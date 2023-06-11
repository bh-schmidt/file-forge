using FileForge.Exceptions;
using FileForge.Helpers;

namespace FileForge.Constants
{
    public class FileExistsAction
    {
        public static readonly FileExistsAction Ask = "ask";
        public static readonly FileExistsAction Replace = "replace";
        public static readonly FileExistsAction Ignore = "ignore";
        public static readonly FileExistsAction Default = Ask;

        private static readonly HashSet<string> allActions =  EnumerationHelper
            .GetAll<FileExistsAction>()
            .Select(e => e.Value)
            .Distinct()
            .ToHashSet();
        
        private FileExistsAction(string value)
        {
            if (allActions is not null && !allActions.Contains(value))
                throw new InvalidFieldException("file exists action", value);

            Value = value;
        }

        public string Value { get; set; }

        public static implicit operator FileExistsAction(string value) => new FileExistsAction(value);
        public static bool operator ==(FileExistsAction var1, FileExistsAction var2) => var1.Value == var2.Value;
        public static bool operator !=(FileExistsAction var1, FileExistsAction var2) => var1.Value != var2.Value;
    }
}
