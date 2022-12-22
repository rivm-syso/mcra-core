using System.IO.Compression;
using System.Text;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.General.TableDefinitions;

namespace MCRA.Data.Management.DataTemplateGeneration {

    /// <summary>
    /// Generator class for creating template zipped-csv datasets for tables
    /// of specific table groups.
    /// </summary>
    public class CsvDatasetTemplateGenerator : IDatasetTemplateGenerator {

        private readonly string _csvTargetFolder;
        private readonly string _csvTargetFileName;

        /// <summary>
        /// Creates a new <see cref="ExcelDatasetTemplateGenerator"/> instance.
        /// </summary>
        /// <param name="targetFileName"></param>
        public CsvDatasetTemplateGenerator(string targetFileName) {
            _csvTargetFileName = targetFileName;
            _csvTargetFolder = Path.Combine(
                Path.GetDirectoryName(targetFileName),
                $".{Path.GetFileNameWithoutExtension(targetFileName)}-tmp"
            );
        }

        /// <summary>
        /// Method to generate the template for the specified data source.
        /// </summary>
        /// <param name="sourceTableGroup">Source table group of the tables to create</param>
        /// <param name="dataFormatId">Optional data format id for a subset of the table group</param>
        public void Create(SourceTableGroup sourceTableGroup, string dataFormatId = null) {
            Directory.CreateDirectory(_csvTargetFolder);
            createTables(sourceTableGroup, dataFormatId);
            createReadMe(sourceTableGroup);
            if (File.Exists(_csvTargetFileName)) {
                File.Delete(_csvTargetFileName);
            }
            ZipFile.CreateFromDirectory(_csvTargetFolder, _csvTargetFileName);
            Directory.Delete(_csvTargetFolder, true);
        }

        private void createTables(SourceTableGroup sourceTableGroup, string dataFormatId) {
            var tableIds = McraTableDefinitions.Instance.GetTableGroupRawTables(sourceTableGroup, dataFormatId);
            foreach (var tableId in tableIds) {
                var table = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                var fileName = Path.Combine(_csvTargetFolder, $"{table.Id}.csv");
                var headers = table.ColumnDefinitions
                    .Select(r => $"\"{r.Id}\"")
                    .ToList();
                File.WriteAllText(fileName, string.Join(",", headers), Encoding.UTF8);
            }
        }

        private void createReadMe(SourceTableGroup sourceTableGroup) {
            var fileName = Path.Combine(_csvTargetFolder, $"README.md");

            var tableGroup = McraTableDefinitions.Instance.DataGroupDefinitions[sourceTableGroup];
            var module = McraModuleDefinitions.Instance.ModuleDefinitionsByTableGroup[sourceTableGroup];
            var moduleClass = McraModuleDefinitions.Instance.GetActionClass(module.ActionType).ToString();
            var readmeText = getReadmeText();
            readmeText = readmeText.Replace("[TableGroupId]", tableGroup.Id.ToLower());
            readmeText = readmeText.Replace("[TableGroup]", tableGroup.Name.ToLower());
            readmeText = readmeText.Replace("[ModuleId]", module.Id.ToLower());
            readmeText = readmeText.Replace("[ModuleClassId]", moduleClass.ToLower());

            File.WriteAllText(fileName, readmeText, Encoding.UTF8);
        }

        private static string getReadmeText() {
            string readmeText;
            var assembly = typeof(CsvDatasetTemplateGenerator).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Resources.TextTemplates.CsvDataSourceTemplate_ReadMe.md";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    readmeText = reader.ReadToEnd();
                }
            }
            return readmeText;
        }
    }
}
