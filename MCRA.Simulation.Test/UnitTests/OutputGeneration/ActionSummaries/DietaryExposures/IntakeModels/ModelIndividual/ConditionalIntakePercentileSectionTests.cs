using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ModelIndividual
    /// </summary>
    [TestClass]
    public class ConditionalIntakePercentileSectionTests : SectionTestBase {
        /// <summary>
        /// Test ConditionalIntakePercentileSection view
        /// </summary>
        [TestMethod]
        public void ConditionalIntakePercentileSection_Test1() {
            var section = new ConditionalIntakePercentileSection() {
                ConditionalIntakePercentileSections = [
                    new CovariatesCollectionIntakePercentileSection() {
                        CovariatesCollection = new CovariatesCollection(),
                        IntakePercentileSection = new IntakePercentileSection(),
                    },
                ],
            };
            AssertIsValidView(section);
        }
    }
}