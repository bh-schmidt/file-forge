using FileForge.Constants;
using FileForge.Maps;

namespace FileForge.Setup
{
    public class FolderHandler
    {
        private readonly string templateDirectory;
        private readonly string targetDirectory;
        private readonly FolderMap rootFolder;

        public FolderHandler(
            string templateDirectory,
            string targetDirectory,
            FolderMap rootFolder)
        {
            this.templateDirectory = templateDirectory;
            this.targetDirectory = targetDirectory;
            this.rootFolder = rootFolder;
        }

        public void Create()
        {
            HandleFolder(rootFolder);
        }

        private void HandleFolder(FolderMap? folder)
        {
            if (folder is null || folder.Action == PathActions.Ignore)
                return;

            DirectoryInfo directoryInfo = new DirectoryInfo(folder.Path);

            EnsureFolder(folder, directoryInfo);

            var fileInfos = directoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                var file = folder.Files.GetValueOrDefault(fileInfo.FullName);
                var fileHandler = new FileHandler(templateDirectory, targetDirectory, file);
                fileHandler.Create();
            }

            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories)
            {
                var internalFolder = folder.Folders.GetValueOrDefault(directory.FullName);
                HandleFolder(internalFolder);
            }
        }

        private void EnsureFolder(FolderMap folder, DirectoryInfo directoryInfo)
        {
            var relativePath = Path.GetRelativePath(templateDirectory, directoryInfo.FullName);
            if (folder.Action == PathActions.Inject && relativePath.Contains('$'))
                relativePath = PathVariableInjector.InjectVariables(relativePath, folder);

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
