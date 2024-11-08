using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class TdsPotentialReductionFactorsSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test TdsPotentialReductionFactorsSection view
        /// </summary>
        [TestMethod]
        public void TdsPotentialReductionFactorsSection_TestSummarize() {
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var concentrationDistributions = new Dictionary<(Food, Compound), ConcentrationDistribution>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    concentrationDistributions[(food, substance)] = new ConcentrationDistribution() {
                        Compound = substance,
                        Food = food,
                        CV = .4,
                        Limit = 1.99,
                        Mean = 1.3,
                        Percentage = 95,
                        Percentile = 2.2
                    };
                }
            }
            var section = new TdsPotentialReductionFactorsSection();
            section.Summarize(concentrationDistributions, foods.Select(c => c.Code).ToList(), ConcentrationUnit.mgPerKg);
            Assert.AreEqual(9, section.Records.Count);
            AssertIsValidView(section);
        }
    }
}
