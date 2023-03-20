using FileForge.Constants;
using FileForge.Exceptions;
using System.Collections.Immutable;

namespace FileForge.Setup
{
    public class PathMappings
    {
        private readonly Dictionary<string, PathMap> pathDictionary = new();
        private readonly LinkedList<TemplateConfig> templateConfigs = new();
        private readonly string templateDirectory;
        private readonly TemplateConfig templateConfig;

        public PathMappings(string templateDirectory, TemplateConfig templateConfig)
        {
            this.templateDirectory = Path.GetFullPath(templateDirectory);
            this.templateConfig = templateConfig;
        }

        public IEnumerable<PathMap> Paths => pathDictionary.Select(e => e.Value);

        public TemplateConfig[] TemplateConfigs => templateConfigs.ToArray();

        public PathMap? Get(string path)
        {
            return pathDictionary.GetValueOrDefault(path);
        }

        public void Map()
        {
            MapPaths(templateDirectory, templateConfig);
        }

        private void MapPaths(string currentDirectory, TemplateConfig? templateConfig = null)
        {
            AddDefaultMap(currentDirectory, false);

            templateConfig ??= GetTemplateConfig(currentDirectory);
            if (templateConfig is not null)
            {
                AddTemplateConfig(templateConfig);
                MapTemplateConfig(currentDirectory);
                MapByTemplateConfig(currentDirectory, templateConfig);
            }

            var files = Directory
                .EnumerateFiles(currentDirectory);

            foreach (var file in files)
                AddDefaultMap(file, true);

            var folders = Directory
                .EnumerateDirectories(currentDirectory)
                .Where(folder => !pathDictionary.ContainsKey(folder) || pathDictionary[folder].Action != PathActions.Ignore);

            foreach (var folder in folders)
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
                    .Where(file => pathConfig.GetRegex(currentDirectory).IsMatch(file));

                MapPaths(pathConfig, files, true);

                var folders = Directory
                    .EnumerateDirectories(currentDirectory, "*", SearchOption.AllDirectories)
                    .Where(folder => pathConfig.GetRegex(currentDirectory).IsMatch(folder));

                MapPaths(pathConfig, folders, false);
            }
        }

        private void MapPaths(TemplateConfig.PathConfig pathConfig, IEnumerable<string> directories, bool isFile)
        {
            foreach (var directory in directories)
            {
                var path = Path.GetFullPath(directory);

                var config = new PathMap
                {
                    Path = path,
                    Action = pathConfig.Action ?? PathActions.Default,
                    Condition = pathConfig.Condition,
                    IsFile = isFile
                };

                if (config.Action != PathActions.Ignore)
                {
                    config.FileExists = pathConfig.FileExists ?? FileExistsActions.Default;
                    config.FolderExists = pathConfig.FolderExists ?? FolderExistsActions.Default;
                }

                if (!pathDictionary.TryAdd(path, config))
                    throw new DuplicatePathException(directory);
            }
        }

        private void AddDefaultMap(string directory, bool isFile)
        {
            string path = Path.GetFullPath(directory);
            if (pathDictionary.ContainsKey(path))
                return;

            var config = new PathMap
            {
                Path = path,
                Action = PathActions.Default,
                FileExists = FileExistsActions.Default,
                FolderExists = FolderExistsActions.Default,
                IsFile = isFile
            };
            pathDictionary.Add(path, config);
        }

        private void AddTemplateConfig(TemplateConfig templateConfig)
        {
            templateConfigs.AddLast(templateConfig);
        }

        private void MapTemplateConfig(string directory)
        {
            string path = Path.GetFullPath(Path.Combine(directory, TemplateConfig.FileName));
            if (pathDictionary.ContainsKey(path))
                return;

            var config = new PathMap
            {
                Path = path,
                Action = PathActions.Ignore,
                IsFile = true
            };
            pathDictionary.Add(path, config);
        }

        private TemplateConfig? GetTemplateConfig(string directory)
        {
            var filePath = Path.GetFullPath(Path.Combine(directory, TemplateConfig.FileName));
            if (pathDictionary.ContainsKey(filePath))
                return null;

            return TemplateConfig.ReadTemplateConfig(filePath);
        }

        public class PathMap
        {
            public string Path { get; set; } = null!;
            public bool IsFile { get; set; }
            public string Action { get; set; } = null!;
            public string? FileExists { get; set; } = null!;
            public string? FolderExists { get; set; } = null!;
            public string? Condition { get; set; }
        }
    }
}
