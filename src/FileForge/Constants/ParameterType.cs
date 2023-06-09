namespace FileForge.Constants
{
    public class ParameterType
    {
        private readonly string value;

        public static readonly ParameterType Text = "text";
        public static readonly ParameterType Long = "long";
        public static readonly ParameterType Decimal = "decimal";
        public static readonly ParameterType SingleSelect = "single-select";
        public static readonly ParameterType MultiSelect = "multi-select";
        public static readonly ParameterType TextOption = "text-option";
        public static readonly ParameterType Default = Text;
        private static readonly ParameterType[] allTypes = new[] { Text, Long, Decimal, SingleSelect, MultiSelect, TextOption };

        public ParameterType(string value)
        {
            if (allTypes is not null && !allTypes.Any(e => e.value == value))
                throw new Exception(); // to do

            this.value = value;
        }

        public static implicit operator ParameterType(string value) => new ParameterType(value);
        public static bool operator ==(ParameterType var1, ParameterType var2) => var1.value == var2.value;
        public static bool operator !=(ParameterType var1, ParameterType var2) => var1.value != var2.value;
    }
}
