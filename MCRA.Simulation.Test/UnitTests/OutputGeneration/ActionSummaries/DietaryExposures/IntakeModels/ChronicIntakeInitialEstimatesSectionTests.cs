using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels
    /// </summary>
    [TestClass]
    public class ChronicIntakeInitialEstimatesTests  {

        /// <summary>
        /// Test ChronicIntakeInitialEstimatesSection model LNN
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionSection_Test5() {
            var lnnModel = new LNNModel(
                new FrequencyModelCalculationSettings(new FrequencyModelSettingsDto()),
                new AmountModelCalculationSettings(new AmountModelSettingsDto())
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
