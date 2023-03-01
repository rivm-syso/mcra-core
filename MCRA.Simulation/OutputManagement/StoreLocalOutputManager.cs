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

            var csvFileIndex = new Dictionary<Guid, (string FileName, string TitlePath)>();
            // Save created CSV files
            if (WriteCsvFiles || WriteReport) {
                toc.SaveTablesAsCsv(outputFolder, sectionManager, csvFileIndex);
                var csvIndexFileName = Path.Combine(outputFolder.FullName, "_CsvFileIndex.txt");
                using (var sw = new StreamWriter(csvIndexFileName)) {
                    foreach (var (fileName, titlePath) in csvFileIndex.Values) {
                        sw.WriteLine($"{fileName}\t{titlePath}");
                    }
                }
            }

            var svgFileIndex = new Dictionary<Guid, (string FileName, string TitlePath)>();
            // Save created chart files
            if (WriteChartFiles || WriteReport) {
                toc.SaveChartFiles(outputFolder, sectionManager, svgFileIndex);
                var chartIndexFileName = Path.Combine(outputFolder.FullName, "_ChartFileIndex.txt");
                using (var sw = new StreamWriter(chartIndexFileName)) {
                    foreach (var (fileName, titlePath) in svgFileIndex.Values) {
                        sw.WriteLine($"{fileName}\t{titlePath}");
                    }
                }
            }

            if (WriteTocCsv) {
                toc.WriteHeadersToFiles(outputFolder.FullName, "txt");
            }

            if (WriteReport) {
                var reportFileName = Path.Combine(outputFolder.FullName, "_Report.html");
                var reportBuilder = new ReportBuilder(toc);
                var html = reportBuilder.RenderDisplayReport(null, true, outputFolder.FullName, false, csvFileIndex, svgFileIndex);
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
