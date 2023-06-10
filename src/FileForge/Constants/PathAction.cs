using FileForge.Exceptions;
using FileForge.Helpers;

namespace FileForge.Constants
{
    public class PathAction
    {
        public static readonly PathAction Inject = "inject";
        public static readonly PathAction Copy = "copy";
        public static readonly PathAction Ignore = "ignore";
        public static readonly PathAction Default = Inject;

        private static readonly HashSet<string> allActions = Enumeration
            .GetAll<PathAction>()
            .Select(e => e.Value)
            .Distinct()
            .ToHashSet();

        public PathAction(string value)
        {
            if (allActions is not null && !allActions.Contains(value))
                throw new InvalidFieldException("path action", value);

            Value = value;
        }

        public string Value { get; set; }

        public static implicit operator PathAction(string value) => new PathAction(value);
        public static implicit operator string(PathAction path) => path.Value;
        public static bool operator ==(PathAction action1, PathAction action2) => action1.Value == action2.Value;
        public static bool operator !=(PathAction action1, PathAction action2) => action1.Value != action2.Value;
    }
}
