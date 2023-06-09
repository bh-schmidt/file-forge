namespace FileForge.Helpers
{
    public static class Enumeration
    {
        internal static IEnumerable<T> GetAll<T>() =>
            typeof(T)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(e => e.GetValue(null))
                .Cast<T>();
    }
}
