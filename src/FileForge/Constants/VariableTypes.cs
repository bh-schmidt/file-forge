namespace FileForge.Constants
{
    public class VariableTypes
    {
        private static readonly HashSet<string> AllTypes = new HashSet<string> { Text, SingleSelect, MultiSelect, TextOption };

        public const string Default = Text;
        public const string Text = "text";
        public const string SingleSelect = "single-select";
        public const string MultiSelect = "multi-select";
        public const string TextOption = "text-option";

        public static bool IsValid(string type)
        {
            return type is not null && AllTypes.Contains(type);
        }
    }
}
