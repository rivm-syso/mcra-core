using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels
    /// </summary>
    [TestClass]
    public class ChronicIntakeInitialEstimatesTests {

        /// <summary>
        /// Test ChronicIntakeInitialEstimatesSection model LNN
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionSection_Test5() {
            var lnnModel = new LNNModel(
                new IntakeModelCalculationSettings(),
                new IntakeModelCalculationSettings()
            ) {
                FrequencyInitials = new FrequencyModelSummary(),
                AmountInitials = new NormalAmountsModelSummary(),
                FallBackModel = IntakeModelType.LNN,
                FrequencyAmountModelSummary = new FrequencyAmountModelSummary(),
            };
            var section = new ChronicIntakeInitialEstimatesSection();
            section.SummarizeModels(new SectionHeader(), lnnModel);
        }
    }
}
