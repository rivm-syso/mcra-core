using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.OutputManagement {
    public interface ISectionManager {

        string GetTempDataFolder();

        void SaveSection(SummarySection section);
        SummarySection LoadSection(string sectionId, Type sectionType);

        void WriteCsvDataToFile(Guid sectionId, string filename);
        void WriteChartDataToFile(Guid sectionId, string filename);
        void WriteXmlDataToFile(Guid sectionId, string filename);

        void SaveSummarySectionHtml(SummarySection section, string sectionHtml);
        string GetSectionHtml(Guid sectionId);

        void SaveXmlDataSectionData(XmlDataSummarySection dataSection);
        void SaveCsvDataSectionData(CsvDataSummarySection dataSection);
        void SaveChartSectionChart(ChartSummarySection chartSection);
    }
}