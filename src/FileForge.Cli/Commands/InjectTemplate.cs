using ImprovedConsole;
using ImprovedConsole.CommandRunners.Arguments;
using ImprovedConsole.CommandRunners.Commands;

namespace FileForge.Cli.Commands
{
    public class InjectTemplate : Command
    {
        public InjectTemplate() : base("Inject a template into the provided folder")
        {
            AddParameter("targetName", "The folder name that will be created at your current directory");
            AddOption("--template", "The directory of your template");
            SetHandler(Handle);
        }

        private static void Handle(CommandArguments arguments)
        {
            var targetDirectory = arguments.Parameters["targetName"]?.Value;
            var templateDirectory = arguments.Options["--template"]?.Value;
            var currentDirectory = Directory.GetCurrentDirectory();

            if (targetDirectory is not null && targetDirectory.Any(e => e == Path.DirectorySeparatorChar || e == Path.AltDirectorySeparatorChar || Path.GetInvalidPathChars().Contains(e)))
            {
                Logger.WriteLine("The target name contains invalid characters.");
                return;
            }

            var targetPath = targetDirectory is null ?
                currentDirectory :
                Path.Combine(currentDirectory, targetDirectory);

            Forge.ForgeTemplate(templateDirectory!, targetPath);
        }
    }
}
