using FileForge.Constants;
using FileForge.Exceptions;
using System.Collections.Immutable;

namespace FileForge.Setup
{
    public class PathMappings
    {
        private readonly Dictionary<string, PathMap> pathDictionary = new();
        private readonly LinkedList<TemplateConfig> templateConfigs = new();

        public PathMappings(string templateDirectory, TemplateConfig templateConfig)
        {
            AddDefaultMap(templateDirectory, false);
            MapPaths(templateDirectory, templateConfig);
        }

        public IEnumerable<PathMap> Paths => pathDictionary.Select(e => e.Value);

        public TemplateConfig[] TemplateConfigs => templateConfigs.ToArray();

        public PathMap? Get(string path)
        {
            return pathDictionary.GetValueOrDefault(path);
        }

        private void MapPaths(string currentDirectory, TemplateConfig? templateConfig = null)
        {
            templateConfig ??= GetTemplateConfig(currentDirectory);
            if (templateConfig is not null)
            {
                AddTemplateConfig(templateConfig);
                MapTemplateConfig(currentDirectory);
                MapByTemplateConfig(currentDirectory, templateConfig);
            }

            var childFiles = Directory
                .EnumerateFiles(currentDirectory)
                .Where(file => !pathDictionary.ContainsKey(file));

            var unmappedFolders = Directory
                .EnumerateDirectories(currentDirectory)
                .Where(folder => !pathDictionary.ContainsKey(folder));

            var mappedFolders = Directory
                .EnumerateDirectories(currentDirectory)
                .Where(folder => pathDictionary.ContainsKey(folder) && pathDictionary[folder].Action != PathActions.Ignore);

            foreach (var file in childFiles)
                AddDefaultMap(file, true);

            foreach (var folder in unmappedFolders)
            {
                AddDefaultMap(folder, false);
                MapPaths(folder);
            }

            foreach (var folder in mappedFolders)
                MapPaths(folder);
        }

        private void MapByTemplateConfig(string currentDirectory, TemplateConfig templateConfig)
        {
            foreach (var pathConfig in templateConfig.Paths)
            {
                if (string.IsNullOrWhiteSpace(pathConfig.Pattern))
                    throw new InvalidPatternException(pathConfig.Pattern);

                var files = Directory
                    .EnumerateFiles(currentDirectory, "*", SearchOption.AllDirectories)
                    .Where(file => pathConfig.Regex.IsMatch(file));

                MapPaths(pathConfig, files, true);

                var folders = Directory
                    .EnumerateDirectories(currentDirectory, "*", SearchOption.AllDirectories)
                    .Where(folder => pathConfig.Regex.IsMatch(folder));

                MapPaths(pathConfig, folders, false);
            }
        }

        private void MapPaths(TemplateConfig.PathConfig pathConfig, IEnumerable<string> directories, bool isFile)
        {
            foreach (var directory in directories)
            {
                var config = new PathMap
                {
                    Path = directory,
                    Action = pathConfig.Action ?? PathActions.Default,
                    Condition = pathConfig.Condition,
                    FileExists = pathConfig.FileExists ?? FileExistsActions.Default,
                    FolderExists = pathConfig.FolderExists ?? FolderExistsActions.Default,
                    IsFile = isFile
                };

                if (!pathDictionary.TryAdd(directory, config))
                    throw new DuplicatePathException(directory);
            }
        }

        private void AddDefaultMap(string directory, bool isFile)
        {
            var config = new PathMap
            {
                Path = directory,
                Action = PathActions.Default,
                FileExists = FileExistsActions.Default,
                FolderExists = FolderExistsActions.Default,
                IsFile = isFile
            };
            pathDictionary.Add(config.Path, config);
        }

        private void AddTemplateConfig(TemplateConfig templateConfig)
        {
            templateConfigs.AddLast(templateConfig);
        }

        private void MapTemplateConfig(string directory)
        {
            var config = new PathMap
            {
                Path = Path.Combine(directory, TemplateConfig.FileName),
                Action = PathActions.Ignore,
                FileExists = FileExistsActions.Default,
                IsFile = true
            };
            pathDictionary.Add(config.Path, config);
        }

        private TemplateConfig? GetTemplateConfig(string directory)
        {
            var filePath = Path.Combine(directory, TemplateConfig.FileName);
            if (pathDictionary.ContainsKey(filePath))
                return null;

            return TemplateConfig.ReadTemplateConfig(filePath);
        }

        public class PathMap
        {
            public string Path { get; set; } = null!;
            public bool IsFile { get; set; }
            public string Action { get; set; } = null!;
            public string FileExists { get; set; } = null!;
            public string FolderExists { get; set; } = null!;
            public string? Condition { get; set; }
        }
    }
}
