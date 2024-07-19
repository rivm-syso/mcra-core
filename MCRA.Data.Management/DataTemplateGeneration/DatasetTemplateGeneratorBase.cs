using System.Text.RegularExpressions;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.General.TableDefinitions;
using GitVersion = ThisAssembly.Git.BaseVersion;

namespace MCRA.Data.Management.DataTemplateGeneration {

    /// <summary>
    /// Extension method to split strings of camel case format (e.g. CamelCaseVariableName)
    /// to lowercase dashed format (used in documentation hyperlinks (camel-case-variable-name)
    /// </summary>
    public static partial class StringSplitExtensions {
        public static string SplitCamelCase(this object s) {
            return string.Join("-", CamelCaseSplitter().Split(s.ToString())).ToLower();
        }
        [GeneratedRegex(@"(?<!^)(?=[A-Z](?![A-Z]|$))")]
        private static partial Regex CamelCaseSplitter();
    }

    /// <summary>
    /// Generator base class for creating a data set template, with datasets for tables
    /// of specific table groups.
    /// </summary>
    /// <param name="targetFileName"></param>
    public class DatasetTemplateGeneratorBase(string targetFileName) : IDatasetTemplateGenerator {

        protected readonly string _targetFileName = targetFileName;

        /// <summary>
        /// Method to generate the template for the specified data source.
        /// </summary>
        /// <param name="sourceTableGroup">Source table group of the tables to create</param>
        /// <param name="dataFormatId">Optional data format id for a subset of the table group</param>
        public virtual void Create(SourceTableGroup sourceTableGroup, string dataFormatId = null) {
        }

        protected static string getReadmeText(SourceTableGroup sourceTableGroup, string templateResourceName) {
            string readmeText;
            var assembly = typeof(DatasetTemplateGeneratorBase).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Resources.TextTemplates.{templateResourceName}";
            using (var stream = assembly.GetManifestResourceStream(resourceName)) {
                using (var reader = new StreamReader(stream)) {
                    readmeText = reader.ReadToEnd();
                }
            }

            //replace tags in the text
            var tableGroup = McraTableDefinitions.Instance.DataGroupDefinitions[sourceTableGroup];
            var module = McraModuleDefinitions.Instance.ModuleDefinitionsByTableGroup[sourceTableGroup];
            var moduleClass = McraModuleDefinitions.Instance.GetActionClass(module.ActionType).SplitCamelCase();
            //split based on capitalization of name
            readmeText = readmeText
                .Replace("[TableGroupId]", tableGroup.Id.ToLower())
                .Replace("[TableGroup]", tableGroup.Name.ToLower())
                .Replace("[ModuleId]", module.Id.SplitCamelCase())
                .Replace("[ModuleClassId]", moduleClass)
                .Replace("[McraVersion]", $"{GitVersion.Major}.{GitVersion.Minor}.{GitVersion.Patch}");

            return readmeText;
        }
    }
}
