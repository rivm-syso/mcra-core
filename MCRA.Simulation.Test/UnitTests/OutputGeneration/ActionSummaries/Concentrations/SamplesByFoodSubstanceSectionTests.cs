using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class SamplesByFoodSubstanceSectionTests : SectionTestBase {
        /// <summary>
        /// Sumarize and test SamplesByFoodSubstanceSection view
        /// </summary>
        [TestMethod]
        public void SamplesByFoodSubstanceSection_Test1() {
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var mu = -1.1;
            var sigma = 2;
            var useFraction = 0.25;
            var lor = 0.05;
            var sampleSize = 200;
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, mu, sigma, useFraction, lor, sampleSize);
            var sampleCompoundCollections = FakeSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section = new SamplesByFoodSubstanceSection();
            section.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);
            var number = section.NumberOfCompoundsWithConcentrations;
            Assert.HasCount(9, section.ConcentrationInputDataRecords);
            Assert.AreEqual(3, number);
            AssertIsValidView(section);
        }
    }
}
