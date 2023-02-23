using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries.DoseResponseModels
    /// </summary>
    [TestClass]
    public class DoseResponseModelsSubstanceOverviewSectionTests : SectionTestBase {

        /// <summary>
        /// Test DoseResponseModelsSubstanceOverviewSection view
        /// </summary>
        [TestMethod]
        public void DoseResponseModelsSubstanceOverviewSection_Test1() {
            var section = new DoseResponseModelsSubstanceOverviewSection();
            section.SummaryRecords = new List<DoseResponseModelSubstanceSummaryRecord>() {
                new DoseResponseModelSubstanceSummaryRecord() {
                    CodeExperiment = "Exp",
                    BenchmarkDose = 0.12,
                    BenchmarkDoseLower = 0.09,
                    BenchmarkDoseUpper = 0.18,
                    CodeCompound = "C",
                    CodeResponse="Resp",
                    Converged = true,
                    CovariateLevel = "",
                    IsTested = true,
                }
            };
            AssertIsValidView(section);
        }
    }
}