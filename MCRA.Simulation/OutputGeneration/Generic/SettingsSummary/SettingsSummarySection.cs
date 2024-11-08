using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.OutputGeneration {
    public class SettingsSummarySection : SummarySection {

        public List<InputParameterRecord> SummaryRecords = [];
        public List<DataSourceSummaryRecord> DataSourceSummaryRecords;

        public SettingsSummarySection() {
        }

        public SettingsSummarySection(Guid guid) {
            _sectionId = guid;
        }

        public void SummarizeDataSource(ActionDataSummaryRecord record) {
            if (DataSourceSummaryRecords == null) {
                DataSourceSummaryRecords = [];
            }

            //get version as string, append file name of version in brackets
            //if it's different from the data source name
            string versionName = record.Version.ToString();
            if(record.DataSourceName?.ToLower() != record.VersionName?.ToLower()) {
                versionName = $"{record.Version} ({record.VersionName})";
            }

            DataSourceSummaryRecords.Add(new DataSourceSummaryRecord() {
                IdDataSourceVersion = record.IdDataSourceVersion,
                DataType = record.SourceTableGroup.GetDisplayName(),
                Name = record.DataSourceName,
                Path = record.DataSourcePath,
                Checksum = record.Checksum,
                Version = versionName,
                VersionDate = record.VersionDate?.ToString(),
            });
        }

        public void SummarizeSetting<T>(string name, T value) {
            SummaryRecords.Add(new InputParameterRecord() {
                Option = name,
                Value = value.PrintValue(),
            });
        }
    }
}
