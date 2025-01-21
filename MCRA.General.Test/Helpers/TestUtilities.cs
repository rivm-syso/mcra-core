namespace MCRA.General.Test.Helpers {
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
    }
}
