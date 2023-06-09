using FileForge.Exceptions;
using FileForge.Setup;
using System.Diagnostics.CodeAnalysis;

namespace FileForge
{
    public class Forge
    {
        public static void ForgeTemplate(
            string templateDirectory,
            string targetDirectory)
        {
            ValidateDirectories(targetDirectory, templateDirectory);
            var templateConfig = GetTemplateConfig(templateDirectory);

            var targetFolder = Path.GetFullPath(targetDirectory);

            var pathMappings = new PathMappings(templateDirectory, templateConfig);
            pathMappings.Map();

            var parameterMappings = new ParameterMappings(pathMappings.RootFolder);
            parameterMappings.Map();

            var parameterHandler = new ParameterHandler(pathMappings.RootFolder);
            parameterHandler.Ask();

            var folderHandler = new FolderHandler(templateDirectory, targetFolder, pathMappings.RootFolder);
            folderHandler.Create();
        }

        private static void ValidateDirectories([NotNull] string? targetDirectory, [NotNull] string? templateDirectory)
        {
            if (string.IsNullOrEmpty(targetDirectory))
                throw new InvalidTargetDirectoryException(targetDirectory);

            if (string.IsNullOrEmpty(templateDirectory))
                throw new InvalidTemplateDirectoryException(templateDirectory);
        }

        private static TemplateConfig GetTemplateConfig(string templateDirectory)
        {
            var relativePath = Path.Combine(templateDirectory, TemplateConfig.FileName);
            var templateConfigPath = Path.GetFullPath(relativePath);

            if (!File.Exists(templateConfigPath))
                throw new TemplateNotFoundException(templateDirectory);

            var templateConfig = TemplateConfig.ReadTemplateConfig(templateConfigPath);
            if (templateConfig is null)
                throw new InvalidTemplateFileException();

            return templateConfig;
        }
    }
}
