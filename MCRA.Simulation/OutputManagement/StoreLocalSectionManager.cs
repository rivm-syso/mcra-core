using MCRA.Utils.Xml;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.OutputManagement {
    public class StoreLocalSectionManager : ISectionManager {

        private string _dataFolder;

        public StoreLocalSectionManager(
            string dataFolder
        ) {
            _dataFolder = dataFolder;
            if (!Directory.Exists(_dataFolder)) {
                Directory.CreateDirectory(_dataFolder);
            }
        }

        public void SaveSection(SummarySection section) {
            var tempDataFileName = Path.Combine(_dataFolder, $"{section.SectionId:D}-Data.zip");
            File.WriteAllBytes(tempDataFileName, XmlSerialization.ToCompressedXml(section));
        }

        public SummarySection LoadSection(string sectionId, Type sectionType) {
            if (!string.IsNullOrEmpty(_dataFolder)) {
                var tempDataFileName = Path.Combine(_dataFolder, $"{sectionId:D}-Data.zip");
                if (File.Exists(tempDataFileName)) {
                    var bytes = File.ReadAllBytes(tempDataFileName);
                    return XmlSerialization.FromCompressedXml<SummarySection>(bytes, sectionType);
                }
            }
            return null;
        }

        public void SaveSummarySectionHtml(SummarySection section, string sectionHtml) {
            var tempDataFileName = Path.Combine(_dataFolder, $"{section.SectionId:D}.html");
            File.WriteAllText(tempDataFileName, sectionHtml);
        }

        public void SaveCsvDataSectionData(CsvDataSummarySection dataSection) {
            var bytes = File.ReadAllBytes(dataSection.CsvFileName);
            var tempDataFileName = Path.Combine(_dataFolder, $"{dataSection.SectionId:D}-CsvData.csv");
            File.WriteAllBytes(tempDataFileName, bytes);
        }

        public void SaveChartSectionChart(ChartSummarySection chartSection) {
            var bytes = File.ReadAllBytes(chartSection.TempFileName);
            var tempDataFileName = Path.Combine(_dataFolder, $"{chartSection.SectionId:D}-ChartData.svg");
            File.WriteAllBytes(tempDataFileName, bytes);
        }

        public void SaveXmlDataSectionData(XmlDataSummarySection dataSection) {
            var bytes = XmlSerialization.CompressString(dataSection.Xml);
            var tempDataFileName = Path.Combine(_dataFolder, $"{dataSection.SectionId:D}-XmlData.zip");
            File.WriteAllBytes(tempDataFileName, bytes);
        }

        public string GetSectionHtml(Guid sectionId) {
            var tempDataFileName = Path.Combine(_dataFolder, $"{sectionId:D}.html");
            if (File.Exists(tempDataFileName)) {
                return File.ReadAllText(tempDataFileName);
            }
            return null;
        }

        public void WriteCsvDataToFile(Guid sectionId, string filename) =>
            writeDataToFile(filename, $"{sectionId:D}-CsvData.csv");

        public void WriteChartDataToFile(Guid sectionId, string filename) =>
            writeDataToFile(filename, $"{sectionId:D}-ChartData.svg");

        public void WriteXmlDataToFile(Guid sectionId, string filename) =>
            writeDataToFile(filename, $"{sectionId:D}-XmlData.zip");

        private void writeDataToFile(string filename, string tempFilename) {
            if (!string.IsNullOrEmpty(_dataFolder)) {
                var tempDataFileName = Path.Combine(_dataFolder, tempFilename);
                if (File.Exists(tempDataFileName)) {
                    File.Copy(tempDataFileName, filename, true);
                }
            }
        }

        public string GetTempDataFolder() {
            return _dataFolder;
        }
    }
}
