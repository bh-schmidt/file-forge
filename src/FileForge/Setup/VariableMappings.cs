using FileForge.Constants;
using FileForge.Maps;

namespace FileForge.Setup
{
    public class VariableMappings
    {
        private FolderMap rootFolder;

        public VariableMappings(FolderMap rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        public void Map()
        {
            Map(rootFolder);
        }

        public void Map(FolderMap folderMap)
        {
            if (folderMap.TemplateConfig is not null)
                foreach (var variable in folderMap.TemplateConfig.Variables)
                    Add(folderMap, variable);

            foreach (var (_, folder) in folderMap.Folders)
                Map(folder);
        }

        public void Add(FolderMap folderMap, TemplateConfig.VariableConfig variable)
        {
            Validate(folderMap, variable);

            folderMap.Variables.Add(variable.Name, new VariableMap
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

        private void Validate(FolderMap folder, TemplateConfig.VariableConfig variable)
        {
            // validate the variableMap instead of templateconfig

            if (string.IsNullOrWhiteSpace(variable.Name))
                return; // to do

            if (folder.Variables.ContainsKey(variable.Name))
                return; // to do

            if (string.IsNullOrWhiteSpace(variable.Description))
                return; // to do

            if (variable.Type == VariableType.TextOption || variable.Type == VariableType.SingleSelect || variable.Type == VariableType.MultiSelect)
            {
                if (variable.Options is null || !variable.Options.Any())
                    return; // to do

                if (variable.Options.Any(option => option is null || !option.Any()))
                    return; // to do
            }

            if (variable.Dependencies is not null)
            {
                if (!variable.Dependencies.Any())
                    return; // to do

                if (variable.Dependencies.Any(dependency => dependency is null || !dependency.Any()))
                    return; // to do

                var invalidDependencies = variable.Dependencies
                    .Where(dependency => !folder.VariableExists(dependency));

                if (invalidDependencies.Any())
                    return; // to do
            }
        }
    }
}
