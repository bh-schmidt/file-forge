namespace FileForge.Constants
{
    public class FolderExistsAction
    {
        private readonly string value;

        public static readonly FolderExistsAction None = "none";
        public static readonly FolderExistsAction Clear = "clear";
        public static readonly FolderExistsAction Default = None;

        private static readonly FolderExistsAction[] allActions = new[] { None, Clear };

        public FolderExistsAction(string value)
        {
            if (allActions is not null && !allActions.Any(e => e.value == value))
                throw new Exception(); // to do

            this.value = value;
        }

        public static implicit operator FolderExistsAction(string value) => new FolderExistsAction(value);
        public static bool operator ==(FolderExistsAction var1, FolderExistsAction var2) => var1.value == var2.value;
        public static bool operator !=(FolderExistsAction var1, FolderExistsAction var2) => var1.value != var2.value;
    }
}
