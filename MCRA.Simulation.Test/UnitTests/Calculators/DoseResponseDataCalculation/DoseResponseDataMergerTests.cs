using MCRA.Simulation.Calculators.DoseResponseDataCalculation;
using MCRA.Simulation.Test.Helpers;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DoseResponseModelCalculation {

    /// <summary>
    /// DoseResponseModelCalculation calculator
    /// </summary>
    [TestClass]
    public class DoseResponseDataMergerTests {

        /// <summary>
        /// Data merger test 1
        /// </summary>
        [TestMethod]
        public void DoseResponseDataMerger_Test1() {
            var experimentsIds = new List<string>() { "foetalExp", "foetalExp2" };
            testMerge(experimentsIds, "DoseResponseDataMerger_Test1");
        }

        /// <summary>
        /// Data merger test 2
        /// </summary>
        [TestMethod]
        public void DoseResponseDataMerger_Test2() {
            var experimentsIds = new List<string>() { "Mixture", "Mixture2" };
            testMerge(experimentsIds, "DoseResponseDataMerger_Test2");
        }

        /// <summary>
        /// Data merger test 3
        /// </summary>
        [TestMethod]
        public void DoseResponseDataMerger_Test3() {
            var experimentsIds = new List<string>() { "Mixture", "Mixture3" };
            testMerge(experimentsIds, "DoseResponseDataMerger_Test3");
        }

        /// <summary>
        /// Data merger test 4
        /// </summary>
        [TestMethod]
        public void DoseResponseDataMerger_Test4() {
            var experimentsIds = new List<string>() { "Mixture", "CAdose-IFNgml" };
            testMerge(experimentsIds, "DoseResponseDataMerger_Test4");
        }

        private static void testMerge(List<string> experimentsIds, string outputId) {
            var outputPath = TestUtilities.CreateTestOutputPath(outputId);
            var dataFolder = Path.Combine("Resources", "DoseResponseData");
            var targetFileName = Path.Combine(outputPath, "DoseResponseData.zip");
            var dataManager = TestUtilities.CompiledDataManagerFromFolder(dataFolder, targetFileName);

            var allExperiments = dataManager.GetAllDoseResponseExperiments();

            var foetalExperiments = experimentsIds.Select(r => allExperiments[r]).ToList();
            var experimentsMerger = new DoseResponseDataMerger();
            var groupedExperiments = foetalExperiments
                .SelectMany(r => r.Responses, (e, r) => new { r, e })
                .GroupBy(r => r.r);
            var response = groupedExperiments.First().Key;
            var experiments = groupedExperiments.First().Select(r => r.e).ToList();
            var mergedExperiment = experimentsMerger.Merge(experiments, response);

            var dataTable = mergedExperiment.CreateAllResponsesDataTable();
            dataTable.ToCsv(Path.Combine(outputPath, "merged.csv"));

            var isMixture = mergedExperiment.ExperimentalUnits.Select(c => c.Doses.Count(d => d.Value > 0)).Any(r => r > 1);
            var processDataTable = mergedExperiment.toDataTable(response, isMixture);
            processDataTable.ToCsv(Path.Combine(outputPath, $"{response.Code}covariate.csv"));
        }
    }
}
