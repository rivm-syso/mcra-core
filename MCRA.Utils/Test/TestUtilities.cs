
namespace MCRA.Utils.Test {
    public static class TestUtilities {

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
    }
}
