namespace FileForge.Constants
{
    public class VariableType
    {
        private readonly string value;

        public static readonly VariableType Text = "text";
        public static readonly VariableType Long = "long";
        public static readonly VariableType Decimal = "decimal";
        public static readonly VariableType SingleSelect = "single-select";
        public static readonly VariableType MultiSelect = "multi-select";
        public static readonly VariableType TextOption = "text-option";
        public static readonly VariableType Default = Text;
        private static readonly VariableType[] allTypes = new[] { Text, Long, Decimal, SingleSelect, MultiSelect, TextOption };

        public VariableType(string value)
        {
            if (allTypes is not null && !allTypes.Any(e => e.value == value))
                throw new Exception(); // to do

            this.value = value;
        }

        public static implicit operator VariableType(string value) => new VariableType(value);
        public static bool operator ==(VariableType var1, VariableType var2) => var1.value == var2.value;
        public static bool operator !=(VariableType var1, VariableType var2) => var1.value != var2.value;
    }
}
