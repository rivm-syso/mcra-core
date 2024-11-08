using MCRA.Utils.Csv;
using MCRA.Data.Raw;
using MCRA.General;
using System.IO.Compression;

namespace MCRA.Data.Management.RawDataWriters {
    public class CsvRawDataWriter : IRawDataWriter {

        protected readonly Dictionary<SourceTableGroup, IRawTableGroupData> _rawData;

        protected readonly string _csvFolder;

        public CsvRawDataWriter(string outputPath) {
            _csvFolder = outputPath;
            _rawData = [];
        }

        public IRawTableGroupData Get(SourceTableGroup tableGroup) {
            if (_rawData.TryGetValue(tableGroup, out var result)) {
                return result;
            }
            return null;
        }

        public void Set<T>(T rawTableGroupData) where T : IRawTableGroupData {
            _rawData[rawTableGroupData.SourceTableGroup] = rawTableGroupData;
        }

        public virtual void Store() {
            foreach (var rawTableGroupData in _rawData.Values) {
                if (rawTableGroupData?.DataTables?.Any(r => r.Value.RecordsUntyped?.Count > 0) ?? false) {
                    if (!Directory.Exists(_csvFolder)) {
                        Directory.CreateDirectory(_csvFolder);
                        Thread.Sleep(100);
                    }
                    foreach (var table in rawTableGroupData.DataTables.Values) {
                        if (table.RecordsUntyped?.Count > 0) {
                            var filename = $"{table.RawDataSourceTableID}.csv";
                            var outputPath = Path.Combine(_csvFolder, filename);
                            var t = table.GetType().GetGenericArguments().Single();
                            var csvWriter = new CsvWriter();
                            csvWriter.WriteToCsvFile(table.RecordsUntyped, outputPath, true, null, t.GetProperties().ToList());
                        }
                    }
                }
            }
        }

        public static void ExportZippedCsv(IRawTableGroupData rawTableGroupData, string targetFileName) {
            if (File.Exists(targetFileName)) {
                File.Delete(targetFileName);
            }
            var wd = $"McraZippedCsv-{Guid.NewGuid():D}";
            var tempCsvFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), wd));
            var writer = new CsvRawDataWriter(tempCsvFolder.FullName);
            writer.Set(rawTableGroupData);
            ZipFile.CreateFromDirectory(tempCsvFolder.FullName, targetFileName, CompressionLevel.Optimal, false);
            writer.Store();
        }
    }
}
