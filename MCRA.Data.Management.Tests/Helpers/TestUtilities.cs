namespace MCRA.Data.Management.Test.Helpers {
    public static class TestUtilities {

        private static string _testOutputsPath = "../../../TestOutput";

        public static string TestOutputPath {
            get {
                var outputPath = Path.GetFullPath(_testOutputsPath);
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                return outputPath;
            }
        }

        public static string CreateTestOutputPath(string id) {
            var outputPath = Path.Combine(TestOutputPath, id);
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
                Thread.Sleep(100);
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
