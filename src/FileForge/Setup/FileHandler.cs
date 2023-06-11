using FileForge.Constants;
using FileForge.Maps;
using ImprovedConsole.Forms;
using ImprovedConsole.Forms.Fields.TextOptions;
using Scriban;
using Scriban.Runtime;

namespace FileForge.Setup
{
    public class FileHandler
    {
        private readonly string templateDirectory;
        private readonly string targetDirectory;
        private readonly FileMap? file;

        public FileHandler(
            string templateDirectory,
            string targetDirectory,
            FileMap? file)
        {
            this.templateDirectory = templateDirectory;
            this.targetDirectory = targetDirectory;
            this.file = file;
        }

        public void Create()
        {
            if (file is null || file.Action == PathAction.Ignore || file.Action == PathAction.Process)
                return;

            string content = GetContent();

            var relativePath = Path.GetRelativePath(templateDirectory, file.Path);
            relativePath = PathParameterInjector.InjectParameter(relativePath, file.Parent!);
            var absolutePath = Path.GetFullPath(Path.Combine(targetDirectory, relativePath));

            if (!File.Exists(absolutePath))
            {
                File.WriteAllText(absolutePath, content);
                return;
            }

            bool replaceAnswer = false;

            if (file.FileExists == FileExistsAction.Ask)
            {
                var form = new Form(new FormOptions
                {
                    ShowConfirmationForms = false
                });

                form.Add()
                    .TextOption($"The file {relativePath} already exists. Do you want to replace?", new[] { "y", "n" }, new TextOptionOptions { Required = true })
                    .OnConfirm(v => replaceAnswer = v == "y");

                form.Run();
            }

            if (file.FileExists == FileExistsAction.Replace || replaceAnswer)
            {
                File.Delete(absolutePath);
                File.WriteAllText(absolutePath, content);
            }
        }

        private string GetContent()
        {
            var text = File.ReadAllText(file!.Path);

            if (file.Action == PathAction.Copy)
                return text;

            var script = new ScriptObject();
            var parameters = file.GetParameters();
            foreach (var parameter in parameters)
                script.Add(parameter.Name, parameter.Value);

            var context = new TemplateContext();
            context.PushGlobal(script);

            var template = Template.Parse(text);
            return template.Render(context);
        }
    }
}
