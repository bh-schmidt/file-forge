namespace FileForge.Constants
{
    public class FileExistsAction
    {
        private readonly string value;

        public static readonly FileExistsAction Ask = "ask";
        public static readonly FileExistsAction Replace = "replace";
        public static readonly FileExistsAction Ignore = "ignore";
        public static readonly FileExistsAction Default = Ask;

        private static readonly FileExistsAction[] allActions = new[] { Ask, Replace, Ignore };

        public FileExistsAction(string value)
        {
            if (allActions is not null && !allActions.Any(e => e.value == value))
                throw new Exception(); // to do

            this.value = value;
        }

        public static implicit operator FileExistsAction(string value) => new FileExistsAction(value);
        public static bool operator ==(FileExistsAction var1, FileExistsAction var2) => var1.value == var2.value;
        public static bool operator !=(FileExistsAction var1, FileExistsAction var2) => var1.value != var2.value;
    }
}
