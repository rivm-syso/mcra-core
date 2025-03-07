using ExCSS;
using MCRA.General;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EffectModelling {
    /// <summary>
    /// Tests for ExternalIndividualDayExposure calculation.
    /// </summary>
    [TestClass]
    public class ExternalIndividualDayExposureTests {

        [TestMethod]
        public void ExternalIndividualDayExposure_HasPositives_Combinations() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            var allsubstances = FakeSubstancesGenerator.Create(3);
            var substances = allsubstances.Take(2).ToList();
            var otherSubstance = allsubstances.Last();
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);

            var externalExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            foreach (var exposure in externalExposures) {
                foreach (var route in routes) {
                    foreach (var substance in substances) {
                        Assert.IsTrue(exposure.HasPositives(route, substance));
                    }
                    Assert.IsFalse(exposure.HasPositives(route, otherSubstance));
                }
                Assert.IsFalse(exposure.HasPositives(ExposureRoute.Inhalation, otherSubstance));
            }
        }

        [TestMethod]
        public void ExternalIndividualDayExposure_HasPositives_Zero_ShouldReturnFalse() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(1, 1, random);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            var substances = FakeSubstancesGenerator.Create(2);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);

            var amount = 0.0;
            var randomMock = new Mock<IRandom>();
            randomMock.Setup(r => r.NextDouble()).Returns(amount);
            var externalExposure = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, randomMock.Object)
                .First();

            Assert.IsFalse(externalExposure.HasPositives(routes.First(), substance));
        }
    }
}
