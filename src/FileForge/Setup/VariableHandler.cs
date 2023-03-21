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
        private readonly Form form = new();
        private readonly FolderMap rootFolder;

        public VariableHandler(FolderMap rootFolder)
        {
            this.rootFolder = rootFolder;
        }

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
            if (fields.ContainsKey(variable.Name))
                return; // to do

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
                    variable.Result = value;
                })
                .OnReset(() =>
                {
                    variable.Result = null;
                });
        }

        private void AddMultiSelect(FormItem formItem, VariableMap variable)
        {
            formItem
                .MultiSelect(variable.Description, () => variable.Options!)
                .OnConfirm(options =>
                {
                    variable.Result = options.Select(e => e.Value);
                })
                .OnReset(() =>
                {
                    variable.Result = null;
                });
        }

        private void AddSingleSelect(FormItem formItem, VariableMap variable)
        {
            formItem
                .SingleSelect(variable.Description, () => variable.Options!)
                .OnConfirm(option =>
                {
                    variable.Result = option?.Value;
                })
                .OnReset(() =>
                {
                    variable.Result = null;
                });
        }

        private void AddOptionSelector(FormItem formItem, VariableMap variable)
        {
            formItem
                .OptionSelector(variable.Description, () => variable.Options!)
                .OnConfirm(value =>
                {
                    variable.Result = value;
                })
                .OnReset(() =>
                {
                    variable.Result = null;
                });
        }
    }
}
