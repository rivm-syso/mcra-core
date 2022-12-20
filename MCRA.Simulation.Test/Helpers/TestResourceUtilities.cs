using MCRA.Utils.DataFileReading;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.Data.Raw.Copying;
using System;
using System.IO;
using System.IO.Compression;

namespace MCRA.Simulation.Test.Helpers {
    public static class TestResourceUtilities {

        public static string TestOutputPath {
            get {
                var outputPath = Path.GetFullPath(Properties.Settings.Default.TestOutputPath);
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                return outputPath;
            }
        }

        public static string ConcatWithOutputPath(string filename) {
            return Path.Combine(TestOutputPath, filename);
        }

        public static string CreateTestOutputPath(string id) {
            var outputPath = Path.Combine(TestOutputPath, id);
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
                System.Threading.Thread.Sleep(100);
            }
            Directory.CreateDirectory(outputPath);
            return outputPath;
        }

        public static string GetOrCreateTestOutputPath(string id) {
            var outputPath = Path.Combine(TestOutputPath, id);
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
            return outputPath;
        }

        public static void CopyRawDataTablesToFolder(string sourceFileName, string outputPath) {
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
                System.Threading.Thread.Sleep(100);
            }
            using (var writer = new CsvFileDataSourceWriter(new DirectoryInfo(outputPath))) {
                var bulkCopier = new RawDataSourceBulkCopier(writer);
                var tgs = bulkCopier.CopyFromDataFile(sourceFileName);
            }
        }

        public static ICompiledDataManager CompiledDataManagerFromFolder(string folder, string pathZipFile) {
            if (!Directory.Exists(folder)) {
                throw new Exception($"Data folder {folder} not found!");
            }

            if (File.Exists(pathZipFile)) {
                File.Delete(pathZipFile);
            }

            ZipFile.CreateFromDirectory(folder, pathZipFile, CompressionLevel.Optimal, false);

            return new CompiledDataManager(
                new SimpleRawDataProvider(() => new ZippedCsvRawDataManager(pathZipFile))
            );
        }
    }
}
