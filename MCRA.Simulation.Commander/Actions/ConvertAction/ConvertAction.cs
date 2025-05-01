using MCRA.Data.Raw.Converters;
using MCRA.Data.Raw.Copying;
using MCRA.Utils.DataFileReading;

namespace MCRA.Simulation.Commander.Actions.ConvertAction {
    public class ConvertAction : ActionBase {

        public int Execute(ConvertActionOptions options) {

            awaitDebugger(options);

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string csvTempFolder = null;

            try {

                // Fetch input file
                var inputFileName = options.InputPath;
                if (!File.Exists(inputFileName)) {
                    throw new Exception($"Specified input file {inputFileName} does not exist.");
                }

                // Determine target file
                var targetFileName = !string.IsNullOrEmpty(options.OutputFileName)
                    ? options.OutputFileName
                    : $"{Path.GetFileNameWithoutExtension(inputFileName)}-{timestamp}.zip";
                if (File.Exists(targetFileName)) {
                    if (options.OverwriteOutput) {
                        File.Delete(targetFileName);
                    } else {
                        throw new Exception($"Output file {targetFileName} already exist. Specify a different output file name or use overwrite option.");
                    }
                }

                // Determine csv Temp folder
                csvTempFolder = $"{Path.GetFileNameWithoutExtension(inputFileName)}-{timestamp}";
                if (Directory.Exists(csvTempFolder)) {
                    throw new Exception($"Csv temp folder {csvTempFolder} already exists.");
                }

                var asExcel = options.FileType.Contains('x', StringComparison.OrdinalIgnoreCase);

                // Check if there is a entity code conversions configuration and initialize data source writer factory accordingly
                var entityRecodingsFile = getRecodingsFile(options, inputFileName);

                Func<IDataSourceWriter> writerFactory;
                Func<IDataSourceWriter> concreteWriter = asExcel
                    ? () => new SpreadsheetDataSourceWriter(targetFileName)
                    : () => new ZippedCsvFileDataSourceWriter(targetFileName, csvTempFolder, options.KeepTempFiles);

                if (!string.IsNullOrEmpty(entityRecodingsFile) && File.Exists(entityRecodingsFile)) {
                    var entityCodeConversionConfiguration = EntityCodeConversionConfiguration.FromXmlFile(entityRecodingsFile);
                    writerFactory = () => new RecodingDataSourceWriter(
                        concreteWriter.Invoke(),
                        entityCodeConversionConfiguration.EntityCodeConversions
                    );
                } else {
                    writerFactory = concreteWriter.Invoke;
                }

                // Run
                var progress = createProgressReport(options.SilentMode);
                using (var dataFileWriter = writerFactory.Invoke()) {
                    var bulkCopier = new RawDataSourceBulkCopier(dataFileWriter);
                    bulkCopier.CopyFromDataFile(
                        inputFileName,
                        progressState: progress
                    );
                }

                Console.WriteLine("\nMCRA datasource conversion finished!");

            } catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine("MCRA datasource conversion failed!");
                Console.WriteLine(ex.ToString());
                return 1;
            } finally {
#if DEBUG
                if (options.InteractiveMode) {
                    Console.SetCursorPosition(1, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.WriteLine();
                    Console.WriteLine("Press enter to exit...");
                    Console.ReadKey();
                }
#endif
            }
            return 0;
        }

        private static string getRecodingsFile(ConvertActionOptions options, string inputFileName) {
            string result = null;
            if (!string.IsNullOrEmpty(options.EntityRecodingFileName)) {
                result = options.EntityRecodingFileName;
            } else if (File.Exists($"{Path.GetFileNameWithoutExtension(inputFileName)}-Recodings.xml")) {
                result = $"{Path.GetFileNameWithoutExtension(inputFileName)}-Recodings.xml";
            } else if (File.Exists(Path.Combine(Path.GetDirectoryName(inputFileName), "Recodings.xml"))) {
                result = Path.Combine(Path.GetDirectoryName(inputFileName), "Recodings.xml");
            }
            return result;
        }
    }
}
