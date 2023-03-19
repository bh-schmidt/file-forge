using FileForge.Constants;
using System.Text;
using System.Text.RegularExpressions;

namespace FileForge.Setup
{
    public class FolderHandler
    {
        private readonly string templateDirectory;
        private readonly string targetDirectory;
        private readonly VariableHandler variableHandler;
        private readonly PathMappings pathMappings;

        public FolderHandler(
            string templateDirectory,
            string targetDirectory,
            VariableHandler variableHandler,
            PathMappings pathMappings)
        {
            this.templateDirectory = templateDirectory;
            this.targetDirectory = targetDirectory;
            this.variableHandler = variableHandler;
            this.pathMappings = pathMappings;
        }

        public void Create()
        {
            var rootFolder = pathMappings.Paths
                .OrderByDescending(e => e.Path.Length)
                .FirstOrDefault();

            if (rootFolder is null)
                return;

            HandleFolder(rootFolder);
        }

        private void HandleFolder(PathMappings.PathMap? folder)
        {
            if (folder is null || folder.Action == PathActions.Ignore)
                return;

            DirectoryInfo directoryInfo = new DirectoryInfo(folder.Path);

            EnsureFolder(folder, directoryInfo);

            var fileInfos = directoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                var file = pathMappings.Get(fileInfo.FullName);
                var fileHandler = new FileHandler(templateDirectory, targetDirectory, variableHandler, file);
                fileHandler.Create();
            }

            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories)
            {
                var internalFolder = pathMappings.Get(directory.FullName);
                HandleFolder(internalFolder);
            }
        }

        private void EnsureFolder(PathMappings.PathMap folder, DirectoryInfo directoryInfo)
        {
            var relativePath = Path.GetRelativePath(templateDirectory, directoryInfo.FullName);
            if (folder.Action == PathActions.Inject && folder.Action.Contains('$'))
                relativePath = PathVariableInjector.InjectVariables(relativePath, variableHandler);

            var absolutePath = Path.GetFullPath(Path.Combine(targetDirectory, relativePath));

            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
                return;
            }

            if (folder.FolderExists == FolderExistsActions.Clear)
            {
                Directory.Delete(absolutePath);
                Directory.CreateDirectory(absolutePath);
            }
        }
    }
}
