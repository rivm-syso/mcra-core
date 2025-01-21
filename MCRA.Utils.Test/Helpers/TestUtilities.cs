namespace MCRA.Utils.Test.Helpers {
    public static class TestUtilities {

        private static string _testOutputsPath = "../../../TestOutput";

        public static string TestOutputsPath {
            get {
                var outputPath = Path.GetFullPath(_testOutputsPath);
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
