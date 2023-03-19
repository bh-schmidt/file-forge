using FileForge.Constants;
using FileForge.Exceptions;
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

        public object? Get(string name) => variables.GetValueOrDefault(name);

        public VariableHandler(VariableMappings variableMappings)
        {
            foreach (var variable in variableMappings.Variables)
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

            form.Run();
        }

        public Dictionary<string, object?> Variables => variables;

        private FormItem CreateFormItem(VariableMappings.Variable variable)
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

                options.DependsOn = new DependsOnFields(dependencies);
            }

            return form.Add(options);
        }

        private void AddText(FormItem formItem, VariableMappings.Variable variable)
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

        private void AddMultiSelect(FormItem formItem, VariableMappings.Variable variable)
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

        private void AddSingleSelect(FormItem formItem, VariableMappings.Variable variable)
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

        private void AddOptionSelector(FormItem formItem, VariableMappings.Variable variable)
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
