using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ModelIndividual
    /// </summary>
    [TestClass]
    public class ConditionalIntakePercentageSectionTests : SectionTestBase {
        /// <summary>
        /// Test ConditionalIntakePercentageSection view
        /// </summary>
        [TestMethod]
        public void ConditionalIntakePercentageSection_Test1() {
            var section = new ConditionalIntakePercentageSection() {
                ConditionalIntakePercentageSections = [
                    new CovariatesCollectionIntakePercentageSection(){
                        CovariatesCollection = new CovariatesCollection(),
                        IntakePercentageSection = new IntakePercentageSection(),
                    },
                ],
            };
            AssertIsValidView(section);
        }
    }
}