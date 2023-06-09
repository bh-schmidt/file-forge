using FileForge.Constants;
using FileForge.Maps;

namespace FileForge.Setup
{
    public class ParameterMappings
    {
        private FolderMap rootFolder;

        public ParameterMappings(FolderMap rootFolder)
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
                foreach (var parameter in folderMap.TemplateConfig.Parameters)
                    Add(folderMap, parameter);

            foreach (var (_, folder) in folderMap.Folders)
                Map(folder);
        }

        public void Add(FolderMap folderMap, TemplateConfig.ParameterConfig parameter)
        {
            Validate(folderMap, parameter);

            folderMap.Parameters.Add(parameter.Name, new ParameterMap
            {
                Name = parameter.Name,
                Description = parameter.Description,
                Type = parameter.Type,
                Dependencies = parameter.Dependencies,
                Condition = parameter.Condition,
                Required = parameter.Required,
                Options = parameter.Options
            });
        }

        private void Validate(FolderMap folder, TemplateConfig.ParameterConfig parameter)
        {
            // to do: validate the parameterMap instead of templateconfig

            if (string.IsNullOrWhiteSpace(parameter.Name))
                return; // to do

            if (folder.Parameters.ContainsKey(parameter.Name))
                return; // to do

            if (string.IsNullOrWhiteSpace(parameter.Description))
                return; // to do

            if (parameter.Type is null)
            {
                return; // to do
            }

            if (parameter.Type == ParameterType.TextOption || parameter.Type == ParameterType.SingleSelect || parameter.Type == ParameterType.MultiSelect)
            {
                if (parameter.Options is null || !parameter.Options.Any())
                    return; // to do

                if (parameter.Options.Any(option => option is null || !option.Any()))
                    return; // to do
            }

            if (parameter.Dependencies is not null)
            {
                if (!parameter.Dependencies.Any())
                    return; // to do

                if (parameter.Dependencies.Any(dependency => dependency is null || !dependency.Any()))
                    return; // to do

                var invalidDependencies = parameter.Dependencies
                    .Where(dependency => !folder.ParameterExists(dependency));

                if (invalidDependencies.Any())
                    return; // to do
            }
        }
    }
}
