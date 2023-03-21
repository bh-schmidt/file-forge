using FileForge.Constants;
using FileForge.Exceptions;
using FileForge.Maps;
using ImprovedConsole.Forms;
using ImprovedConsole.Forms.Fields;
using System.Collections.Immutable;

namespace FileForge.Setup
{
    public class VariableHandler
    {
        private readonly Dictionary<string, IField> fields = new Dictionary<string, IField>();
        private readonly Dictionary<string, object?> variables = new Dictionary<string, object?>();
        private readonly Form form = new Form();
        private readonly FolderMap rootFolder;

        public object? Get(string name) => variables.GetValueOrDefault(name);

        public VariableHandler(FolderMap rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        public Dictionary<string, object?> Variables => variables;

        public void Ask()
        {
            AddFields(rootFolder);

            form.Run();
        }

        public void AddFields(FolderMap folderMap)
        {
            foreach (var (_, variable) in folderMap.Variables)
                AddFields(variable);

            foreach (var (_, folder) in folderMap.Folders)
                AddFields(folder);
        }

        private void AddFields(VariableMap variable)
        {
            var formItem = CreateFormItem(variable);

            switch (variable.Type)
            {
                case VariableTypes.Text:
                    AddText(formItem, variable);
                    break;
                case VariableTypes.SingleSelect:
                    AddSingleSelect(formItem, variable);
                    break;
                case VariableTypes.MultiSelect:
                    AddMultiSelect(formItem, variable);
                    break;
                case VariableTypes.TextOption:
                    AddOptionSelector(formItem, variable);
                    break;
                default:
                    break;
            }

            fields.Add(variable.Name, formItem.Field!);
        }

        private FormItem CreateFormItem(VariableMap variable)
        {
            var options = new FormItemOptions();

            if (variable.Dependencies is not null && variable.Dependencies.Any())
            {
                var invalidDependencies = variable.Dependencies
                    .Where(e => !fields.ContainsKey(e))
                    .Distinct();

                if (invalidDependencies.Any())
                    throw new InvalidDependencyException(variable.Name, invalidDependencies);

                var dependencies = variable.Dependencies
                    .Distinct()
                    .Select(variableName => fields.GetValueOrDefault(variableName)!)
                    .ToArray();

                options.Dependencies = new FormItemDependencies(dependencies);
            }

            return form.Add(options);
        }

        private void AddText(FormItem formItem, VariableMap variable)
        {
            formItem
                .TextField(variable.Description)
                .OnConfirm(value =>
                {
                    variables.Remove(variable.Name);
                    variables.Add(variable.Name, value);
                })
                .OnReset(() =>
                {
                    variables.Remove(variable.Name);
                });
        }

        private void AddMultiSelect(FormItem formItem, VariableMap variable)
        {
            formItem
                .MultiSelect(variable.Description, () => variable.Options!)
                .OnConfirm(options =>
                {
                    variables.Remove(variable.Name);
                    variables.Add(variable.Name, options.Select(e => e.Value));
                })
                .OnReset(() =>
                {
                    variables.Remove(variable.Name);
                });
        }

        private void AddSingleSelect(FormItem formItem, VariableMap variable)
        {
            formItem
                .SingleSelect(variable.Description, () => variable.Options!)
                .OnConfirm(option =>
                {
                    variables.Remove(variable.Name);
                    variables.Add(variable.Name, option?.Value);
                })
                .OnReset(() =>
                {
                    variables.Remove(variable.Name);
                });
        }

        private void AddOptionSelector(FormItem formItem, VariableMap variable)
        {
            formItem
                .OptionSelector(variable.Description, () => variable.Options!)
                .OnConfirm(value =>
                {
                    variables.Remove(variable.Name);
                    variables.Add(variable.Name, value);
                })
                .OnReset(() =>
                {
                    variables.Remove(variable.Name);
                });
        }
    }
}
