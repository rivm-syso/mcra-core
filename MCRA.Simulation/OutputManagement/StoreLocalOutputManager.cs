using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputManagement {

    /// <summary>
    /// Manager for creating and storing outputs that uses a
    /// local folder for storing the output data.
    /// </summary>
    public class StoreLocalOutputManager : IOutputManager {

        public bool WriteTocCsv { get; set; } = true;
        public bool WriteReport { get; set; } = false;
        public bool WriteCsvFiles { get; set; } = true;
        public bool WriteChartFiles { get; set; } = true;

        private readonly string _outputPath;

        /// <summary>
        /// Creates a new <see cref="InMemoryOutputManager"/> instance.
        /// </summary>
        /// <param name="outputPath">The folder in which the output should be stored.</param>
        public StoreLocalOutputManager(
            string outputPath
        ) {
            _outputPath = outputPath;
        }

        public IOutput CreateOutput(int taskId) {
            var output = new OutputData() {
                idTask = taskId,
                StartExecution = DateTime.Now,
                BuildDate = Simulation.RetrieveLinkerTimestamp(),
                BuildVersion = Simulation.RetrieveVersion(),
                DateCreated = DateTime.Now,
            };

            GetOutputTempFolder(output);

            return output;
        }

        public IRawDataWriter CreateRawDataWriter(IOutput output) {
            var outputDataFolder = Path.Combine(_outputPath, "OutputData");
            return new CsvRawDataWriter(outputDataFolder);
        }

        public ISectionManager CreateSectionManager(IOutput output) {
            var sectionDataFolder = Path.Combine(_outputPath, "Temp");
            return new StoreLocalSectionManager(sectionDataFolder);
        }

        public void SaveOutput(
            IOutput output,
            ISectionManager sectionManager
        ) {
            var outputFolder = new DirectoryInfo(_outputPath);

            // Build the XML recursively from the section header
            var toc = SummaryToc.FromCompressedXml(output.SectionHeaderData, sectionManager);

            // Save created CSV files
            if (WriteCsvFiles) {
                var csvFiles = new Dictionary<string, string>();
                toc.SaveTablesAsCsv(outputFolder, sectionManager, csvFiles);
                var csvIndexFileName = Path.Combine(outputFolder.FullName, "_CsvFileIndex.txt");
                using (var sw = new StreamWriter(csvIndexFileName)) {
                    foreach (var kvp in csvFiles) {
                        sw.WriteLine($"{kvp.Key}\t{kvp.Value}");
                    }
                }
            }

            // Save created chart files
            if (WriteChartFiles) {
                var chartFiles = new Dictionary<string, string>();
                toc.SaveChartFiles(outputFolder, sectionManager, chartFiles);
                var chartIndexFileName = Path.Combine(outputFolder.FullName, "_ChartFileIndex.txt");
                using (var sw = new StreamWriter(chartIndexFileName)) {
                    foreach (var kvp in chartFiles) {
                        sw.WriteLine($"{kvp.Key}\t{kvp.Value}");
                    }
                }
            }

            if (WriteTocCsv) {
                toc.WriteHeadersToFiles(outputFolder.FullName, "txt");
            }

            if (WriteReport) {
                var reportPath = Path.Combine(outputFolder.FullName, "Report");
                if (!Directory.Exists(reportPath)) {
                    Directory.CreateDirectory(reportPath);
                }
                var reportFileName = Path.Combine(reportPath, "_Report.html");
                var reportBuilder = new ReportBuilder(toc);
                var html = reportBuilder.RenderReport(null, true, reportPath);
                File.WriteAllText(reportFileName, html);
            }
        }

        public string GetOutputTempFolder(IOutput output) {
            var tempFolder = Path.Combine(_outputPath, "Temp");
            if (!Directory.Exists(tempFolder)) {
                Directory.CreateDirectory(tempFolder);
            }
            return tempFolder;
        }
    }
}
