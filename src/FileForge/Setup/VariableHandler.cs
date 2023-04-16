using FileForge.Constants;
using FileForge.Exceptions;
using FileForge.Helpers;
using FileForge.Maps;
using Flee.PublicTypes;
using ImprovedConsole.Forms;
using ImprovedConsole.Forms.Fields;
using ImprovedConsole.Forms.Fields.DecimalFields;
using ImprovedConsole.Forms.Fields.LongFields;
using ImprovedConsole.Forms.Fields.MultiSelects;
using ImprovedConsole.Forms.Fields.SingleSelects;
using ImprovedConsole.Forms.Fields.TextFields;
using ImprovedConsole.Forms.Fields.TextOptions;
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

            AddFields(variable, formItem);

            fields.Add(variable.Name, formItem.Field!);
        }

        private void AddFields(VariableMap variable, FormItem formItem)
        {
            if (variable.Type == VariableType.Text)
            {
                AddText(formItem, variable);
                return;
            }

            if (variable.Type == VariableType.Long)
            {
                AddLong(formItem, variable);
                return;
            }

            if (variable.Type == VariableType.Decimal)
            {
                AddDecimal(formItem, variable);
                return;
            }

            if (variable.Type == VariableType.SingleSelect)
            {
                AddSingleSelect(formItem, variable);
                return;
            }

            if (variable.Type == VariableType.MultiSelect)
            {
                AddMultiSelect(formItem, variable);
                return;
            }

            if (variable.Type == VariableType.TextOption)
            {
                AddTextOption(formItem, variable);
                return;
            }
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
                    var variables = folderMap.GetAnsweredVariables();
                    return ExpressionEvaluation.Evaluate(variable.Condition, variables);
                };
            }

            return form.Add(options);
        }

        private void AddText(FormItem formItem, VariableMap variable)
        {
            TextFieldOptions options = new TextFieldOptions();

            if (!string.IsNullOrWhiteSpace(variable.Required))
                options.Required = variable.Required == "true";

            formItem
                .TextField(variable.Description, options)
                .OnConfirm(value =>
                {
                    variable.Answer = value;
                })
                .OnReset(() =>
                {
                    variable.Answer = null;
                });
        }

        private void AddLong(FormItem formItem, VariableMap variable)
        {
            LongFieldOptions options = new LongFieldOptions();

            if (!string.IsNullOrWhiteSpace(variable.Required))
                options.Required = variable.Required == "true";

            formItem
                .LongField(variable.Description, options)
                .OnConfirm(value =>
                {
                    variable.Answer = value;
                })
                .OnReset(() =>
                {
                    variable.Answer = null;
                });
        }

        private void AddDecimal(FormItem formItem, VariableMap variable)
        {
            DecimalFieldOptions options = new DecimalFieldOptions();

            if (!string.IsNullOrWhiteSpace(variable.Required))
                options.Required = variable.Required == "true";

            formItem
                .DecimalField(variable.Description)
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
            MultiSelectOptions options = new MultiSelectOptions();

            if (!string.IsNullOrWhiteSpace(variable.Required))
                options.Required = variable.Required == "true";

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
            SingleSelectOptions options = new SingleSelectOptions();

            if (!string.IsNullOrWhiteSpace(variable.Required))
                options.Required = variable.Required == "true";

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

        private void AddTextOption(FormItem formItem, VariableMap variable)
        {
            TextOptionOptions options = new TextOptionOptions();

            if (!string.IsNullOrWhiteSpace(variable.Required))
                options.Required = variable.Required == "true";

            formItem
                .TextOption(variable.Description, () => variable.Options!)
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
