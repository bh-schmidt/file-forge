namespace FileForge.Constants
{
    public class PathAction
    {
        private readonly string value;

        public static readonly PathAction Inject = "inject";
        public static readonly PathAction Copy = "copy";
        public static readonly PathAction Ignore = "ignore";
        public static readonly PathAction Default = Inject;

        private static readonly PathAction[] allActions = new[] { Inject, Copy, Ignore };

        public PathAction(string value)
        {
            if (allActions is not null && !allActions.Any(e => e.value == value))
                throw new Exception(); // to do

            this.value = value;
        }

        public static implicit operator PathAction(string value) => new PathAction(value);
        public static bool operator ==(PathAction action1, PathAction action2) => action1.value == action2.value;
        public static bool operator !=(PathAction action1, PathAction action2) => action1.value != action2.value;
    }
}
