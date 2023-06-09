using FileForge.Cli.Commands;
using ImprovedConsole.CommandRunners;

var commandBuilder = new FileForgeCommandBuilder();
var runner = new CommandRunner(commandBuilder);
runner.Run(args);