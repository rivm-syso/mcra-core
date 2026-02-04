using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.UnitDefinitions.Defaults;
using MCRA.Simulation.Calculators.ConcentrationModelBuilder;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Utils.Statistics;


namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Calculators {

    [TestClass]
    public class ConcentrationModelBuilderTests {

        [TestMethod]
        public void ConcentrationModelBuilderTests_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var concentrationDistributions = new List<DustConcentrationDistribution>() { new() {
                Mean = -.1,
                CvVariability = 0.05,
                DistributionType = DustConcentrationDistributionType.LogNormal,
                Substance = new Compound() { Code = "C" }
            } };

            var concentrationModelsBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                concentrationDistributions,
                NonDetectsHandlingMethod.ReplaceByZero,
                0,
                SystemUnits.DefaultDustConcentrationUnit
            );

            var name = concentrationModels.FirstOrDefault().Value.GetType().Name;
            Assert.AreEqual((new CMSummaryStatistics()).GetType().Name, name);
        }
    }
}