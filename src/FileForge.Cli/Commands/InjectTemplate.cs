using FileForge.Exceptions;
using FileForge.Setup;
using ImprovedConsole.CommandRunners.Arguments;
using ImprovedConsole.CommandRunners.Commands;

namespace FileForge.Cli.Commands
{
    public class InjectTemplate : Command
    {
        public InjectTemplate() : base("Inject a template into the provided folder")
        {
            AddParameter("targetDirectory", "The directory that will be created at the provided directory");
            AddOption("--template", "The directory of the template");
            SetHandler(arguments => Handle(arguments));
        }

        private static void Handle(CommandArguments arguments)
        {
            var targetDirectory = arguments.Parameters["targetDirectory"]?.Value;
            var templateDirectory = arguments.Options["--template"]?.Value;
            var currentDirectory = Directory.GetCurrentDirectory();

            if (string.IsNullOrEmpty(targetDirectory))
                throw new InvalidTargetFolderException(targetDirectory);

            if (targetDirectory.Any(e => e == Path.DirectorySeparatorChar || e == Path.AltDirectorySeparatorChar || Path.GetInvalidPathChars().Contains(e)))
                throw new InvalidTargetFolderException(targetDirectory);

            if (string.IsNullOrEmpty(templateDirectory))
                throw new InvalidTemplateDirectoryException(templateDirectory);

            var filePath = templateDirectory.EndsWith(TemplateConfig.FileName) ?
                templateDirectory :
                Path.Combine(templateDirectory, TemplateConfig.FileName);

            filePath = Path.GetFullPath(filePath);

            if (!File.Exists(filePath))
                throw new TemplateNotFoundException(templateDirectory);

            var templateConfig = TemplateConfig.ReadTemplateConfig(filePath);
            if (templateConfig is null)
                throw new InvalidTemplateFileException();

            var targetFolder = Path.IsPathRooted(templateDirectory) ?
                Path.GetFullPath(templateDirectory) :
                Path.GetFullPath(Path.Combine(currentDirectory, targetDirectory));

            var pathMappings = new PathMappings(templateDirectory, templateConfig);
            pathMappings.Map();

            var parameterMappings = new ParameterMappings(pathMappings.RootFolder);
            parameterMappings.Map();

            var parameterHandler = new ParameterHandler(pathMappings.RootFolder);
            parameterHandler.Ask();

            var folderHandler = new FolderHandler(templateDirectory, targetFolder, pathMappings.RootFolder);
            folderHandler.Create();
        }
    }
}
