using FileForge.Exceptions;
using FileForge.Helpers;

namespace FileForge.Constants
{
    public class ParameterType
    {
        public static readonly ParameterType Text = "text";
        public static readonly ParameterType Long = "long";
        public static readonly ParameterType Decimal = "decimal";
        public static readonly ParameterType SingleSelect = "single-select";
        public static readonly ParameterType MultiSelect = "multi-select";
        public static readonly ParameterType TextOption = "text-option";
        public static readonly ParameterType Default = Text;

        private static readonly HashSet<string> allTypes = Enumeration
            .GetAll<ParameterType>()
            .Select(e => e.Value)
            .Distinct()
            .ToHashSet();

        private ParameterType(string value)
        {
            if (allTypes is not null && !allTypes.Contains(value))
                throw new InvalidFieldException("parameter type", value);

            Value = value;
        }

        public string Value { get; set; }

        public static implicit operator ParameterType(string value) => new ParameterType(value);
        public static bool operator ==(ParameterType var1, ParameterType var2) => var1.Value == var2.Value;
        public static bool operator !=(ParameterType var1, ParameterType var2) => var1.Value != var2.Value;
    }
}
