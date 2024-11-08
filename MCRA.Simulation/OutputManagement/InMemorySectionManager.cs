using MCRA.Utils.Xml;
using MCRA.Simulation.OutputGeneration;
using System.Text;

namespace MCRA.Simulation.OutputManagement {

    /// <summary>
    /// In-memory output section manager. All output sections and section data
    /// is stored in-memory.
    /// </summary>
    public class InMemorySectionManager : ISectionManager {

        private readonly Dictionary<string, SummarySection> _sectionLookup = [];

        private readonly Dictionary<string, byte[]> _sectionData = [];

        public InMemorySectionManager() {
        }

        public void SaveSection(SummarySection section) {
            _sectionLookup[section.SectionId] = section;
        }

        public SummarySection LoadSection(string sectionId, Type sectionType) {
            _sectionLookup.TryGetValue(sectionId, out var section);
            return section;
        }

        public void SaveSummarySectionHtml(SummarySection section, string sectionHtml) {
            var bytes = XmlSerialization.CompressString(sectionHtml);
            _sectionData[section.SectionId] = bytes;
        }

        public void SaveCsvDataSectionData(CsvDataSummarySection dataSection) {
            var bytes = File.ReadAllBytes(dataSection.CsvFileName);
            _sectionLookup[dataSection.SectionId] = dataSection;
            _sectionData[dataSection.SectionId] = bytes;
        }

        public void SaveChartSectionChart(ChartSummarySection chartSection) {
            var bytes = File.ReadAllBytes(chartSection.TempFileName);
            _sectionLookup[chartSection.SectionId] = chartSection;
            _sectionData[chartSection.SectionId] = bytes;
        }

        public void SaveXmlDataSectionData(XmlDataSummarySection dataSection) {
            var bytes = Encoding.UTF8.GetBytes(dataSection.Xml);
            _sectionLookup[dataSection.SectionId] = dataSection;
            _sectionData[dataSection.SectionId] = bytes;
        }

        public string GetSectionHtml(Guid sectionId) {
            if (_sectionData.TryGetValue(sectionId.ToString(), out var data)) {
                return XmlSerialization.UncompressBytes(data);
            }
            return null;
        }

        public void WriteCsvDataToFile(Guid sectionId, string filename) => writeDataToFile(sectionId, filename);

        public void WriteChartDataToFile(Guid sectionId, string filename) => writeDataToFile(sectionId, filename);

        public void WriteXmlDataToFile(Guid sectionId, string filename) => writeDataToFile(sectionId, filename);


        private void writeDataToFile(Guid sectionId, string filename) {
            _sectionData.TryGetValue(sectionId.ToString(), out var sectionData);
            if (sectionData != null) {
                File.WriteAllBytes(filename, sectionData);
            }
        }

        public string GetTempDataFolder() {
            return null;
        }
    }
}
