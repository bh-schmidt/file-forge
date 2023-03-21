using FileForge.Constants;
using FileForge.Exceptions;
using FileForge.Maps;
using Flee.PublicTypes;
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
                AddFields(folderMap, variable);

            foreach (var (_, folder) in folderMap.Folders)
                AddFields(folder);
        }

        private void AddFields(FolderMap folderMap, VariableMap variable)
        {
            if (fields.ContainsKey(variable.Name))
                return; // to do

            var formItem = CreateFormItem(folderMap, variable);

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

        private FormItem CreateFormItem(FolderMap folderMap, VariableMap variable)
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

            if (!string.IsNullOrWhiteSpace(variable.Condition))
            {
                options.Condition = () =>
                {
                    var context = new ExpressionContext();

                    var variables = folderMap
                        .GetVariables()
                        .Where(e => e.Answer is not null);

                    foreach (var variable in variables)
                        context.Variables.Add(variable.Name, variable.Answer!);

                    var expression = context.CompileGeneric<bool>(variable.Condition);
                    return expression.Evaluate();
                };
            }

            return form.Add(options);
        }

        private void AddText(FormItem formItem, VariableMap variable)
        {
            formItem
                .TextField(variable.Description)
                .OnConfirm(value =>
                {
                    variable.Answer = value;
                })
                .OnReset(() =>
                {
                    variable.Answer = null;
                });
        }

        private void AddMultiSelect(FormItem formItem, VariableMap variable)
        {
            formItem
                .MultiSelect(variable.Description, () => variable.Options!)
                .OnConfirm(options =>
                {
                    variable.Answer = options.Select(e => e.Value);
                })
                .OnReset(() =>
                {
                    variable.Answer = null;
                });
        }

        private void AddSingleSelect(FormItem formItem, VariableMap variable)
        {
            formItem
                .SingleSelect(variable.Description, () => variable.Options!)
                .OnConfirm(option =>
                {
                    variable.Answer = option?.Value;
                })
                .OnReset(() =>
                {
                    variable.Answer = null;
                });
        }

        private void AddOptionSelector(FormItem formItem, VariableMap variable)
        {
            formItem
                .OptionSelector(variable.Description, () => variable.Options!)
                .OnConfirm(value =>
                {
                    variable.Answer = value;
                })
                .OnReset(() =>
                {
                    variable.Answer = null;
                });
        }
    }
}
