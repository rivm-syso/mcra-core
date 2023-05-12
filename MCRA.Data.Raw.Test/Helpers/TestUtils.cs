namespace MCRA.Data.Raw.Test.Helpers {
    public static class TestUtils {

        public static string OutputResourcesPath {
            get {
                var outputPath = Path.GetFullPath(Properties.Settings.Default.TestOutputPath);
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                return outputPath;
            }
        }

        public static string ConcatWithOutputPath(string filename) {
            return Path.Combine(OutputResourcesPath, filename);
        }

        public static string CreateTestOutputPath(string id) {
            var outputPath = Path.Combine(OutputResourcesPath, id);
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
                System.Threading.Thread.Sleep(100);
            }
            Directory.CreateDirectory(outputPath);
            return outputPath;
        }

        public static string GetResource(string path) {
            return Path.Combine("Resources", path);
        }
    }
}
