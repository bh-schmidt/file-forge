using FileForge.Constants;
using FileForge.Maps;
using ImprovedConsole.Forms;
using ImprovedConsole.Forms.Fields.OptionSelectors;
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
            if (file is null || file.Action == PathActions.Ignore)
                return;

            string content = GetContent();

            var relativePath = Path.GetRelativePath(templateDirectory, file.Path);
            relativePath = PathVariableInjector.InjectVariables(relativePath, file.Parent!);
            var absolutePath = Path.GetFullPath(Path.Combine(targetDirectory, relativePath));

            if (!File.Exists(absolutePath))
            {
                File.WriteAllText(absolutePath, content);
                return;
            }

            bool replaceAnswer = false;

            if (file.FileExists == FileExistsActions.Ask)
            {
                var form = new Form(new FormOptions
                {
                    ShowConfirmationForms = false
                });

                form.Add()
                    .OptionSelector($"The file {relativePath} already exists. Do you want to replace?", new[] { "y", "n" }, new OptionSelectorsOptions { Required = true })
                    .OnConfirm(v => replaceAnswer = v == "y");

                form.Run();
            }

            if (file.FileExists == FileExistsActions.Replace || replaceAnswer)
            {
                File.Delete(absolutePath);
                File.WriteAllText(absolutePath, content);
            }
        }

        private string GetContent()
        {
            var text = File.ReadAllText(file!.Path);

            if (file.Action == PathActions.Copy)
                return text;

            var script = new ScriptObject();
            var variables = file.GetVariables();
            foreach (var variable in variables)
                script.Add(variable.Name, variable.Answer);

            var context = new TemplateContext();
            context.PushGlobal(script);

            var template = Template.Parse(text);
            return template.Render(context);
        }
    }
}
