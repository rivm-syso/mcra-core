namespace MCRA.Data.Compiled.Test.Helpers {
    public static class TestUtilities {

        private static string _testOutputsPath = "../../../TestOutput";

        public static string OutputResourcesPath {
            get {
                var outputPath = Path.GetFullPath(_testOutputsPath);
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                return outputPath;
            }
        }

        public static string ConcatWithOutputPath(string filename) {
            return Path.Combine(OutputResourcesPath, filename);
        }
    }
}
