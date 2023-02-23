using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators {
    public abstract class CalculatorTestsBase {

        private static string _sectionOutputPath = Path.Combine(TestUtilities.TestOutputPath, "Calculators");

        /// <summary>
        /// Creates the summary section tests output folder that is used when rendering views
        /// to the test output folder.
        /// </summary>
        /// <param name="testCtx"></param>
        [AssemblyInitialize]
        public static void MyTestInitialize(TestContext testCtx) {
            if (!Directory.Exists(_sectionOutputPath)) {
                Directory.CreateDirectory(_sectionOutputPath);
            }
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
