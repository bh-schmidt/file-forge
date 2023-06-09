using ImprovedConsole.CommandRunners.Commands;

namespace FileForge.Cli.Commands
{
    public class FileForgeCommandBuilder : CommandBuilder
    {
        public FileForgeCommandBuilder()
        {
            AddDefaultCommand<InjectTemplate>();
        }
    }
}
