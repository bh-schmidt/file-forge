using FileForge.Commands;
using FileForge.Setup;
using ImprovedConsole.CommandRunners;

namespace FileForge
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var commandBuilder = new FileForgeCommandBuilder();
            var runner = new CommandRunner(commandBuilder);
            runner.Run(args);
        }
    }
}