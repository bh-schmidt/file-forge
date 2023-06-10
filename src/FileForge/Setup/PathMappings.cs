using FileForge.Constants;
using FileForge.Helpers;
using FileForge.Maps;

namespace FileForge.Setup
{
    public class PathMappings
    {
        private readonly string templateDirectory;
        private readonly TemplateConfig templateConfig;

        private readonly Dictionary<string, FolderMap> mappedFolders = new();
        private readonly Dictionary<string, FileMap> mappedFiles = new();

        public PathMappings(string templateDirectory, TemplateConfig templateConfig)
        {
            this.templateDirectory = Path.GetFullPath(templateDirectory);
            this.templateConfig = templateConfig;

            RootFolder = new FolderMap
            {
                Path = templateDirectory,
                Action = PathAction.Default,
                FolderExists = FolderExistsAction.Default,
                TemplateConfig = templateConfig,
            };
        }

        public FolderMap RootFolder { get; }

        public void Map()
        {
            MapPaths(RootFolder, templateDirectory);
        }

        private void MapPaths(FolderMap rootFolder, string currentDirectory)
        {
            if(!PathHelper.IsSubPath(templateDirectory, currentDirectory))
                return; // to do: throw invalid path

            rootFolder.TemplateConfig ??= TemplateConfig.ReadTemplateConfigByDirectory(currentDirectory);

            var filePaths = Directory
                .EnumerateFiles(currentDirectory);

            foreach (var filePath in filePaths)
            {
                var pathConfig = rootFolder.GetPathConfig(filePath);

                var fileConfig = new FileMap
                {
                    Path = filePath,
                    Action = pathConfig?.Action ?? PathAction.Default,
                    Condition = pathConfig?.Condition,
                    Parent = rootFolder
                };

                if (fileConfig.Action != PathAction.Ignore)
                    fileConfig.FileExists = pathConfig?.FileExists ?? FileExistsAction.Default;

                rootFolder.Files.Add(filePath, fileConfig);
            }

            var folderPaths = Directory
                .EnumerateDirectories(currentDirectory)
                .Where(dir => !rootFolder.Folders.ContainsKey(dir) || rootFolder.Folders[dir].Action != PathAction.Ignore);

            foreach (var folderPath in folderPaths)
            {
                var pathConfig = rootFolder.GetPathConfig(folderPath);

                var folderMap = new FolderMap
                {
                    Path = folderPath,
                    Action = pathConfig?.Action ?? PathAction.Default,
                    Condition = pathConfig?.Condition,
                    Parent = rootFolder
                };

                if (folderMap.Action != PathAction.Ignore)
                    folderMap.FolderExists = pathConfig?.FolderExists ?? FolderExistsAction.Default;

                rootFolder.Folders.Add(folderPath, folderMap);

                MapPaths(folderMap, folderPath);
            }
        }
    }
}
