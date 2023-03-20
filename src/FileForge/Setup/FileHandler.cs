using FileForge.Constants;
using ImprovedConsole.Forms;
using ImprovedConsole.Forms.Fields.OptionSelectors;

namespace FileForge.Setup
{
    public class FileHandler
    {
        private readonly string templateDirectory;
        private readonly string targetDirectory;
        private readonly VariableHandler variableHandler;
        private readonly PathMappings.PathMap? pathMapping;

        public FileHandler(
            string templateDirectory,
            string targetDirectory,
            VariableHandler variableHandler,
            PathMappings.PathMap? pathMapping)
        {
            this.templateDirectory = templateDirectory;
            this.targetDirectory = targetDirectory;
            this.variableHandler = variableHandler;
            this.pathMapping = pathMapping;
        }

        public void Create()
        {
            if (pathMapping is null || !pathMapping.IsFile || pathMapping.Action == PathActions.Ignore)
                return;

            string content = GetContent(pathMapping, variableHandler);

            var relativePath = Path.GetRelativePath(templateDirectory, pathMapping.Path);
            var absolutePath = Path.GetFullPath(Path.Combine(targetDirectory, relativePath));

            if (!File.Exists(absolutePath))
            {
                File.WriteAllText(absolutePath, content);
                return;
            }

            bool replaceAnswer = false;

            if (pathMapping.FileExists == FileExistsActions.Ask)
            {
                var form = new Form();
                form.Add()
                    .OptionSelector($"The file {relativePath} already exists. Do you want to replace?", new[] { "y", "n" }, new OptionSelectorsOptions { Required = true })
                    .OnConfirm(v => replaceAnswer = v == "y");
                form.Run();
            }

            if (pathMapping.FileExists == FileExistsActions.Replace || replaceAnswer)
            {
                File.Delete(absolutePath);
                File.WriteAllText(absolutePath, content);
            }
        }

        private static string GetContent(PathMappings.PathMap pathMapping, VariableHandler variableHandler)
        {
            // to do: inject variables
            return File.ReadAllText(pathMapping.Path);
        }
    }
}
