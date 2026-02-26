using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.BodIndicatorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    [TestClass]
    public class BodIndicatorModelFactoryTests {

        [TestMethod]
        [DataRow(BodIndicatorDistributionType.Constant, typeof(BodIndicatorConstantModel))]
        [DataRow(BodIndicatorDistributionType.Triangular, typeof(BodIndicatorDistributionModel<TriangularDistribution>))]
        [DataRow(BodIndicatorDistributionType.Normal, typeof(BodIndicatorDistributionModel<NormalDistribution>))]
        [DataRow(BodIndicatorDistributionType.LogNormal, typeof(BodIndicatorDistributionModel<LogNormalDistribution>))]
        public void BodIndicatorModelFactory_TestCreate(
            BodIndicatorDistributionType distributionType,
            Type expectedType
        ) {
            var indicator = BodIndicator.DALY;
            var nominalValue = 1D;
            var lower = .5D;
            var upper = 1.5D;
            var bod = new BurdenOfDisease() {
                BodUncertaintyDistribution = distributionType,
                Value = nominalValue,
                BodUncertaintyUpper = upper,
                BodUncertaintyLower = lower,
                BodIndicator = indicator
            };
            var population = new Population() {
                Name = "Fake Population",
                Size = 1000
            };
            var model = BodIndicatorModelFactory.Create(bod, population);
            Assert.IsInstanceOfType(model, expectedType);
            Assert.AreEqual(nominalValue, model.GetBodIndicatorValue());
            Assert.AreEqual(indicator, model.BodIndicator);
        }
    }
}
