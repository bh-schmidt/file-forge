using FileForge.Exceptions;
using FileForge.Helpers;

namespace FileForge.Constants
{
    public class FolderExistsAction
    {
        public static readonly FolderExistsAction None = "none";
        public static readonly FolderExistsAction Clear = "clear";
        public static readonly FolderExistsAction Default = None;

        private static readonly HashSet<string> allActions = EnumerationHelper
            .GetAll<FolderExistsAction>()
            .Select(e => e.Value)
            .Distinct()
            .ToHashSet();

        private FolderExistsAction(string value)
        {
            if (allActions is not null && !allActions.Contains(value))
                throw new InvalidFieldException("folder exists action", value);

            Value = value;
        }

        public string Value { get; set; }

        public static implicit operator FolderExistsAction(string value) => new FolderExistsAction(value);
        public static bool operator ==(FolderExistsAction var1, FolderExistsAction var2) => var1.Value == var2.Value;
        public static bool operator !=(FolderExistsAction var1, FolderExistsAction var2) => var1.Value != var2.Value;
    }
}
