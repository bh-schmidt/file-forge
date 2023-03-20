using FileForge.Constants;

namespace FileForge.Setup
{
    public class VariableMappings
    {
        private readonly IEnumerable<TemplateConfig> templates;
        private Dictionary<string, Variable> variables = new();

        public VariableMappings(IEnumerable<TemplateConfig> templates)
        {
            this.templates = templates;
        }

        public void Map()
        {
            foreach (var template in templates)
                Add(template.Variables);
        }

        public IEnumerable<Variable> Variables => variables.Select(e => e.Value);

        public void Add(IEnumerable<TemplateConfig.VariableConfig> variables)
        {
            foreach (var variable in variables)
                Add(variable);
        }

        public void Add(TemplateConfig.VariableConfig variable)
        {
            Validate(variable);

            variables.Add(variable.Name, new Variable
            {
                Name = variable.Name,
                Description = variable.Description,
                Type = variable.Type,
                Dependencies = variable.Dependencies,
                Condition = variable.Condition,
                Required = variable.Required,
                Options = variable.Options
            });
        }

        private void Validate(TemplateConfig.VariableConfig variable)
        {
            if (string.IsNullOrWhiteSpace(variable.Name))
                return;

            if (variables.ContainsKey(variable.Name))
                return;

            if (string.IsNullOrWhiteSpace(variable.Description))
                return;

            if (!VariableTypes.IsValid(variable.Type))
                return;

            if (variable.Type is VariableTypes.TextOption or VariableTypes.SingleSelect or VariableTypes.MultiSelect)
            {
                if (variable.Options is null || !variable.Options.Any())
                    return;

                if (variable.Options.Any(option => option is null || !option.Any()))
                    return;
            }

            if (variable.Dependencies is not null)
            {
                if (!variable.Dependencies.Any())
                    return;

                if (variable.Dependencies.Any(dependency => dependency is null || !dependency.Any()))
                    return;

                var invalidDependencies = variable.Dependencies
                    .Where(dependency => !variables.ContainsKey(dependency));

                if (invalidDependencies.Any())
                    return;
            }
        }

        public class Variable
        {
            public string Name { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string Type { get; set; } = null!;
            public string[]? Dependencies { get; set; }
            public string? Condition { get; set; }
            public string? Required { get; set; }
            public string[]? Options { get; set; }
        }
    }
}
