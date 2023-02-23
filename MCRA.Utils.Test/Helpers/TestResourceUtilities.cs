namespace MCRA.Utils.Test.Helpers {
    public static class TestResourceUtilities {

        public static string TestOutputsPath {
            get {
                var outputPath = Path.GetFullPath(Properties.Settings.Default.TestOutputPath);
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                return outputPath;
            }
        }

        public static string ConcatWithOutputPath(string filename) {
            return Path.Combine(TestOutputsPath, filename);
        }
    }
}
