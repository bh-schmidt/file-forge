using ImprovedConsole.CommandRunners.Commands;

namespace FileForge.Commands
{
    public class FileForgeCommandBuilder : CommandBuilder
    {
        public FileForgeCommandBuilder()
        {
            AddDefaultCommand<InjectTemplate>();
        }
    }
}
