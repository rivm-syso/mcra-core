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
        public bool WriteDataFiles { get; set; } = true;
        public bool WriteAsSpreadsheet { get; set; } = false;
        public bool WriteChartFiles { get; set; } = true;

        private string _outputPath;

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
                DateCreated = DateTime.Now
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
            var metadataPath = Path.Combine(outputFolder.FullName, "Metadata");
            Directory.CreateDirectory(metadataPath);

            // Build the XML recursively from the section header
            var toc = SummaryToc.FromCompressedXml(output.SectionHeaderData, sectionManager);

            var csvFileIndex = new Dictionary<Guid, (string FileName, string TitlePath)>();
            // Save created CSV files
            var dataFolder = new DirectoryInfo(Path.Combine(_outputPath, "Data"));
            if (WriteDataFiles || WriteReport) {
                toc.SaveTablesAsCsv(dataFolder, sectionManager, csvFileIndex);
                var csvIndexFileName = Path.Combine(outputFolder.FullName, "Metadata", "CsvFileIndex.txt");
                using (var sw = new StreamWriter(csvIndexFileName)) {
                    foreach (var (fileName, titlePath) in csvFileIndex.Values) {
                        sw.WriteLine($"{fileName}\t{titlePath}");
                    }
                }
            }

            var svgFileIndex = new Dictionary<Guid, (string FileName, string TitlePath)>();
            // Save created chart files
            if (WriteChartFiles || WriteReport) {
                var chartsFolder = new DirectoryInfo(Path.Combine(_outputPath, "Img"));
                toc.SaveChartFiles(chartsFolder, sectionManager, svgFileIndex);
                var chartIndexFileName = Path.Combine(outputFolder.FullName, "Metadata", "ChartFileIndex.txt");
                using (var sw = new StreamWriter(chartIndexFileName)) {
                    foreach (var (fileName, titlePath) in svgFileIndex.Values) {
                        sw.WriteLine($"{fileName}\t{titlePath}");
                    }
                }
            }

            if (WriteTocCsv) {
                toc.WriteHeadersToFiles(Path.Combine(outputFolder.FullName, "Metadata"), "txt");
            }

            if (WriteReport) {
                var reportFileName = Path.Combine(outputFolder.FullName, "Report.html");
                var reportBuilder = new ReportBuilder(toc);
                var html = reportBuilder.RenderDisplayReport(null, true, outputFolder.FullName, csvIndex: csvFileIndex, svgIndex: svgFileIndex);
                File.WriteAllText(reportFileName, html);
            }

            if (WriteAsSpreadsheet && (WriteReport || WriteDataFiles)) {
                //Use data folder to write spreadsheet files using the toc and the file index
                foreach (var dh in toc.DataHeaders) {
                    if (csvFileIndex.TryGetValue(dh.SectionId, out var idx)) {
                        var xlsFile = Path.Combine(
                            dataFolder.FullName,
                            $"{Path.GetFileNameWithoutExtension(idx.FileName)}.xlsx"
                        );
                        var csvFile = Path.Combine(dataFolder.FullName, idx.FileName);
                        dh.SaveSpreadsheetFile(sectionManager, xlsFile, csvFile);
                        //remove the CSV output file in the output folder
                        if (File.Exists(csvFile)) {
                            File.Delete(csvFile);
                        }
                    }
                }
            }
        }

        public string GetOutputTempFolder(IOutput output) {
            var tempFolder = Path.Combine(_outputPath, "Temp");
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }

        public void SetOutputPath(string outputPath) {
            if (Directory.Exists(outputPath)) {
                _outputPath = outputPath;
            } else {
                throw new DirectoryNotFoundException("Path 'outputPath' was not found.");
            }
        }
    }
}
