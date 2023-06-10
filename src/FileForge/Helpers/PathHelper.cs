namespace FileForge.Helpers
{
    internal static class PathHelper
    {
        public static bool IsSubPath(string basePath, string subPath)
        {
            string relativePath = Path.GetRelativePath(basePath, subPath);
            return !relativePath.StartsWith("..") && !Path.IsPathRooted(relativePath);
        }
    }
}
