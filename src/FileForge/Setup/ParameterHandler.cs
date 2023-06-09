using FileForge.Constants;
using FileForge.Exceptions;
using FileForge.Helpers;
using FileForge.Maps;
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
    public class ParameterHandler
    {
        private readonly Dictionary<string, IField> fields = new Dictionary<string, IField>();
        private readonly Form form = new();
        private readonly FolderMap rootFolder;

        public ParameterHandler(FolderMap rootFolder)
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
            foreach (var (_, parameter) in folderMap.Parameters)
                AddFields(folderMap, parameter);

            foreach (var (_, folder) in folderMap.Folders)
                AddFields(folder);
        }

        private void AddFields(FolderMap folderMap, ParameterMap parameter)
        {
            if (fields.ContainsKey(parameter.Name))
                throw new DuplicateValueException("parameter name", parameter.Name);

            var formItem = CreateFormItem(folderMap, parameter);

            AddFields(parameter, formItem);

            fields.Add(parameter.Name, formItem.Field!);
        }

        private void AddFields(ParameterMap parameter, FormItem formItem)
        {
            if (parameter.Type == ParameterType.Text)
            {
                AddText(formItem, parameter);
                return;
            }

            if (parameter.Type == ParameterType.Long)
            {
                AddLong(formItem, parameter);
                return;
            }

            if (parameter.Type == ParameterType.Decimal)
            {
                AddDecimal(formItem, parameter);
                return;
            }

            if (parameter.Type == ParameterType.SingleSelect)
            {
                AddSingleSelect(formItem, parameter);
                return;
            }

            if (parameter.Type == ParameterType.MultiSelect)
            {
                AddMultiSelect(formItem, parameter);
                return;
            }

            if (parameter.Type == ParameterType.TextOption)
            {
                AddTextOption(formItem, parameter);
                return;
            }
        }

        private FormItem CreateFormItem(FolderMap folderMap, ParameterMap parameter)
        {
            var options = new FormItemOptions();

            if (parameter.Dependencies is not null && parameter.Dependencies.Any())
            {
                var invalidDependencies = parameter.Dependencies
                    .Where(e => !fields.ContainsKey(e))
                    .Distinct();

                if (invalidDependencies.Any())
                    throw new InvalidFieldException(nameof(TemplateConfig.ParameterConfig.Dependencies), invalidDependencies);

                var dependencies = parameter.Dependencies
                    .Distinct()
                    .Select(parameterName => fields.GetValueOrDefault(parameterName)!)
                    .ToArray();

                options.Dependencies = new FormItemDependencies(dependencies);
            }

            if (!string.IsNullOrWhiteSpace(parameter.Condition))
            {
                options.Condition = () =>
                {
                    var parameters = folderMap.GetAnsweredParameters();
                    return ExpressionEvaluation.Evaluate(parameter.Condition, parameters);
                };
            }

            return form.Add(options);
        }

        private void AddText(FormItem formItem, ParameterMap parameter)
        {
            TextFieldOptions options = new TextFieldOptions();

            if (!string.IsNullOrWhiteSpace(parameter.Required))
                options.Required = parameter.Required == "true";

            formItem
                .TextField(parameter.Description, options)
                .OnConfirm(value =>
                {
                    parameter.Value = value;
                })
                .OnReset(() =>
                {
                    parameter.Value = null;
                });
        }

        private void AddLong(FormItem formItem, ParameterMap parameter)
        {
            LongFieldOptions options = new LongFieldOptions();

            if (!string.IsNullOrWhiteSpace(parameter.Required))
                options.Required = parameter.Required == "true";

            formItem
                .LongField(parameter.Description, options)
                .OnConfirm(value =>
                {
                    parameter.Value = value;
                })
                .OnReset(() =>
                {
                    parameter.Value = null;
                });
        }

        private void AddDecimal(FormItem formItem, ParameterMap parameter)
        {
            DecimalFieldOptions options = new DecimalFieldOptions();

            if (!string.IsNullOrWhiteSpace(parameter.Required))
                options.Required = parameter.Required == "true";

            formItem
                .DecimalField(parameter.Description)
                .OnConfirm(value =>
                {
                    parameter.Value = value;
                })
                .OnReset(() =>
                {
                    parameter.Value = null;
                });
        }

        private void AddMultiSelect(FormItem formItem, ParameterMap parameter)
        {
            MultiSelectOptions options = new MultiSelectOptions();

            if (!string.IsNullOrWhiteSpace(parameter.Required))
                options.Required = parameter.Required == "true";

            formItem
                .MultiSelect(parameter.Description, () => parameter.Options!)
                .OnConfirm(options =>
                {
                    parameter.Value = options.Select(e => e.Value);
                })
                .OnReset(() =>
                {
                    parameter.Value = null;
                });
        }

        private void AddSingleSelect(FormItem formItem, ParameterMap parameter)
        {
            SingleSelectOptions options = new SingleSelectOptions();

            if (!string.IsNullOrWhiteSpace(parameter.Required))
                options.Required = parameter.Required == "true";

            formItem
                .SingleSelect(parameter.Description, () => parameter.Options!)
                .OnConfirm(option =>
                {
                    parameter.Value = option?.Value;
                })
                .OnReset(() =>
                {
                    parameter.Value = null;
                });
        }

        private void AddTextOption(FormItem formItem, ParameterMap parameter)
        {
            TextOptionOptions options = new TextOptionOptions();

            if (!string.IsNullOrWhiteSpace(parameter.Required))
                options.Required = parameter.Required == "true";

            formItem
                .TextOption(parameter.Description, () => parameter.Options!)
                .OnConfirm(value =>
                {
                    parameter.Value = value;
                })
                .OnReset(() =>
                {
                    parameter.Value = null;
                });
        }
    }
}
