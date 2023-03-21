using FileForge.Constants;
using FileForge.Exceptions;
using FileForge.Maps;
using System.Collections.Immutable;

namespace FileForge.Setup
{
    public class PathMappings
    {
        private readonly string templateDirectory;
        private readonly TemplateConfig templateConfig;

        private readonly Dictionary<string, FolderMap> tempFolders = new();
        private readonly Dictionary<string, FileMap> tempFiles = new();

        public PathMappings(string templateDirectory, TemplateConfig templateConfig)
        {
            this.templateDirectory = Path.GetFullPath(templateDirectory);
            this.templateConfig = templateConfig;

            RootFolder = new FolderMap
            {
                Path = templateDirectory,
                Action = PathActions.Default,
                FolderExists = FolderExistsActions.Default,
                TemplateConfig = templateConfig,
            };
        }

        public FolderMap RootFolder { get; }

        public void Map()
        {
            MapPaths(RootFolder, templateDirectory);
        }

        private void MapPaths(FolderMap folderMap, string currentDirectory)
        {
            folderMap.TemplateConfig ??= GetTemplateConfig(currentDirectory);
            if (folderMap.TemplateConfig is not null)
            {
                MapTemplateConfig(folderMap, currentDirectory);
                MapFolder(currentDirectory, templateConfig); //???
            }

            var filePaths = Directory
                .EnumerateFiles(currentDirectory);

            foreach (var filePath in filePaths)
            {
                var file = tempFiles.GetValueOrDefault(filePath);
                if (file is null)
                {
                    AddDefaultFile(folderMap, filePath);
                    continue;
                }

                file.Parent = folderMap;
                folderMap.Files.Add(filePath, file);
            }

            var folderPaths = Directory
                .EnumerateDirectories(currentDirectory)
                .Where(dir => !folderMap.Folders.ContainsKey(dir) || folderMap.Folders[dir].Action != PathActions.Ignore);

            foreach (var folderPath in folderPaths)
            {
                var folder = tempFolders.GetValueOrDefault(folderPath);
                if(folder is null)
                {
                    AddDefaultFolder(folderMap, folderPath);
                    continue;
                }

                folder.Parent = folderMap;
                folderMap.Folders.Add(folderPath, folder);
            }
        }

        private void MapFolder(string currentDirectory, TemplateConfig templateConfig)
        {
            foreach (var pathConfig in templateConfig.Paths)
            {
                if (string.IsNullOrWhiteSpace(pathConfig.Pattern))
                    throw new InvalidPatternException(pathConfig.Pattern);

                var files = Directory
                    .EnumerateFiles(currentDirectory, "*", SearchOption.AllDirectories)
                    .Where(file => pathConfig.GetRegex(currentDirectory).IsMatch(file));

                foreach (var file in files)
                    AddTempFile(pathConfig, file);

                var folders = Directory
                    .EnumerateDirectories(currentDirectory, "*", SearchOption.AllDirectories)
                    .Where(folder => pathConfig.GetRegex(currentDirectory).IsMatch(folder));

                foreach (var folder in folders)
                    AddTempFolder(pathConfig, folder);
            }
        }

        private void AddTempFile(TemplateConfig.PathConfig pathConfig, string directory)
        {
            var path = Path.GetFullPath(directory);

            var config = new FileMap
            {
                Path = path,
                Action = pathConfig.Action ?? PathActions.Default,
                Condition = pathConfig.Condition,
            };

            if (config.Action != PathActions.Ignore)
                config.FileExists = pathConfig.FileExists ?? FileExistsActions.Default;

            if (!tempFiles.TryAdd(path, config))
                throw new DuplicatePathException(directory);
        }


        private void AddTempFolder(TemplateConfig.PathConfig pathConfig, string directory)
        {
            var path = Path.GetFullPath(directory);

            var config = new FolderMap
            {
                Path = path,
                Action = pathConfig.Action ?? PathActions.Default,
                Condition = pathConfig.Condition,
            };

            if (config.Action != PathActions.Ignore)
                config.FolderExists = pathConfig.FolderExists ?? FolderExistsActions.Default;

            if (!tempFolders.TryAdd(path, config))
                throw new DuplicatePathException(directory);
        }

        private void AddDefaultFolder(FolderMap previousFolder, string directory)
        {
            string path = Path.GetFullPath(directory);
            if (previousFolder.Folders.ContainsKey(path))
                return;

            var config = new FolderMap
            {
                Path = path,
                Action = PathActions.Default,
                FolderExists = FolderExistsActions.Default,
                Parent = previousFolder
            };

            previousFolder.Folders.Add(path, config);
        }

        private void AddDefaultFile(FolderMap folder, string directory)
        {
            string path = Path.GetFullPath(directory);
            if (folder.Files.ContainsKey(path))
                return;

            var config = new FileMap
            {
                Path = path,
                Action = PathActions.Default,
                FileExists = FileExistsActions.Default,
                Parent = folder
            };

            folder.Files.Add(path, config);
        }

        private void MapTemplateConfig(FolderMap folder, string directory)
        {
            string path = Path.GetFullPath(Path.Combine(directory, TemplateConfig.FileName));
            if (folder.Files.ContainsKey(path))
                return;

            var config = new FileMap
            {
                Path = path,
                Action = PathActions.Ignore,
                Parent = folder
            };

            folder.Files.Add(path, config);
        }

        private TemplateConfig? GetTemplateConfig(string directory)
        {
            var filePath = Path.GetFullPath(Path.Combine(directory, TemplateConfig.FileName));
            if (tempFiles.ContainsKey(filePath))
                return null;

            return TemplateConfig.ReadTemplateConfig(filePath);
        }
    }
}
