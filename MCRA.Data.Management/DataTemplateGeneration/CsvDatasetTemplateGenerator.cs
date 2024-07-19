using System.IO.Compression;
using System.Text;
using MCRA.General;
using MCRA.General.TableDefinitions;

namespace MCRA.Data.Management.DataTemplateGeneration {

    /// <summary>
    /// Generator class for creating template zipped-csv datasets for tables
    /// of specific table groups.
    /// </summary>
    public class CsvDatasetTemplateGenerator : DatasetTemplateGeneratorBase {

        private readonly string _csvTargetFolder;

        /// <summary>
        /// Creates a new <see cref="ExcelDatasetTemplateGenerator"/> instance.
        /// </summary>
        /// <param name="targetFileName"></param>
        public CsvDatasetTemplateGenerator(string targetFileName) : base(targetFileName) {
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
        public override void Create(SourceTableGroup sourceTableGroup, string dataFormatId = null) {
            Directory.CreateDirectory(_csvTargetFolder);
            createTables(sourceTableGroup, dataFormatId);
            createReadMe(sourceTableGroup);
            if (File.Exists(_targetFileName)) {
                File.Delete(_targetFileName);
            }
            ZipFile.CreateFromDirectory(_csvTargetFolder, _targetFileName);
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

            var readmeText = getReadmeText(sourceTableGroup, "CsvDataSourceTemplate_ReadMe.md");

            File.WriteAllText(fileName, readmeText, Encoding.UTF8);
        }
    }
}
