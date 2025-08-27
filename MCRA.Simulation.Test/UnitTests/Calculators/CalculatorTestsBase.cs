using MCRA.Simulation.Test.Helpers;

namespace MCRA.Simulation.Test.UnitTests.Calculators {
    [TestClass]
    public abstract class CalculatorTestsBase {

        private static string _sectionOutputPath =
            Path.Combine(TestUtilities.TestOutputPath, "Calculators");

        /// <summary>
        /// Creates the summary section tests output folder that is used when rendering views
        /// to the test output folder.
        /// </summary>
        protected CalculatorTestsBase() {
            Directory.CreateDirectory(_sectionOutputPath);
        }

        /// <summary>
        /// Creeates a test output folder for with the specified name as a
        /// subfolder of the calculators tests.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetCalculatorTestOutputFolder(string name) {
            var outputPath = Path.Combine(_sectionOutputPath, GetType().Name, name);
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
            }
            return outputPath;
        }
    }
}
