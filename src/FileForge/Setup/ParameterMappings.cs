using FileForge.Constants;
using FileForge.Exceptions;
using FileForge.Maps;
using System.Diagnostics.CodeAnalysis;

namespace FileForge.Setup
{
    public class ParameterMappings
    {
        private HashSet<ParameterType> optionsParams = new() { ParameterType.TextOption, ParameterType.SingleSelect, ParameterType.MultiSelect };
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

            folderMap.Parameters.Add(parameter.Name!, new ParameterMap
            {
                Name = parameter.Name!,
                Description = parameter.Description!,
                Type = parameter.Type!,
                Dependencies = parameter.Dependencies,
                Condition = parameter.Condition,
                Required = parameter.Required,
                Options = parameter.Options
            });
        }

        private void Validate(FolderMap folder,  TemplateConfig.ParameterConfig parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Name))
                throw new InvalidFieldException("parameter name", parameter.Name);

            if (folder.Parameters.ContainsKey(parameter.Name))
                throw new DuplicateValueException("parameter name", parameter.Name);

            if (string.IsNullOrWhiteSpace(parameter.Description))
                throw new InvalidFieldException("parameter description", parameter.Description);

            if (parameter.Type is null)
                throw new InvalidFieldException("parameter type", parameter.Type);

            if (optionsParams.Contains( parameter.Type))
            {
                if (parameter.Options is null || !parameter.Options.Any())
                    throw new InvalidFieldException("parameter options", parameter.Options);

                if (parameter.Options.Any(option => option is null || !option.Any()))
                    throw new InvalidFieldException("parameter options", parameter.Options);
            }

            if (parameter.Dependencies is not null && parameter.Dependencies.Any())
            {
                if (parameter.Dependencies.Any(dependency => dependency is null || !dependency.Any()))
                    throw new InvalidFieldException("parameter dependencies", parameter.Dependencies);

                var invalidDependencies = parameter.Dependencies
                    .Where(dependency => !folder.ParameterExists(dependency));

                if (invalidDependencies.Any())
                    throw new InvalidFieldException("parameter dependencies", invalidDependencies);
            }
        }
    }
}
