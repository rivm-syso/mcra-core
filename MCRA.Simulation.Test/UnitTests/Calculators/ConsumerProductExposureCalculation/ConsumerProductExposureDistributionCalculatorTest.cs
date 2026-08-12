using MathNet.Numerics.LinearAlgebra;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Logger;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConsumerProductExposureCalculation {

    [TestClass]
    public class ConsumerProductExposureDistributionCalculatorTest : CalculatorTestsBase {


        /// <summary>
        /// Fit lognormal distribution to percentiles using package rriskDistributions from R.
        /// </summary>
        [TestMethod]
        [DataRow(4, 4.8, new double[] { 50, 95 })]
        [DataRow(3.4, 4.2, new double[] { 5, 50, 95 })]
        [DataRow(0.4, 0.6, new double[] { 90, 95, 99 })]
        public void ConsumerProductExposureDistributionCalculator_TestCompute(
            double mean,
            double upper,
            double[] percentages
        ) {
            var random = new McraRandomGenerator(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var routes = new[] { ExposureRoute.Oral };

            var distributionExpected = LogNormalDistribution.FromMeanAndUpper(mean, upper);
            var assessment = fakeAssessment(
                substances: [.. substances],
                routes: routes,
                percentages: percentages,
                distributionModel: (e, s) => (distributionExpected, 20)
            );

            var calculator = new ConsumerProductExposureDistributionCalculator();
            var result = calculator.Compute([assessment]);

            var model = result.First();
            Assert.AreEqual(distributionExpected.Mu, model.Distribution.Mu, 1e-4);
            Assert.AreEqual(distributionExpected.Sigma, model.Distribution.Sigma, 1e-4);
            Assert.AreEqual(.2, model.FractionZeros);
        }

        private ConsumerProductExposureAssessment fakeAssessment(
            Compound[] substances,
            ExposureRoute[] routes,
            ConsumerProductExposureEstimateUnit unit = ConsumerProductExposureEstimateUnit.ugPerKgBWPerDay,
            ExposureType exposureType = ExposureType.Acute,
            GenderType gender = GenderType.Undefined,
            Func<ExposureRoute, Compound, (LogNormalDistribution, double)> distributionModel = null,
            double? ageMin = null,
            double? ageMax = null,
            double[] percentages = null
        ) {
            var i = 1;
            percentages = percentages ?? [50, 75, 95];
            var exposureSets = new List<ConsumerProductExposureSet>();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    (var distribution, var pctZeros) = distributionModel != null
                        ? distributionModel(route, substance) : (LogNormalDistribution.FromMeanAndUpper(5, 7), 20);
                    var set = fakeExposureSet(
                        substance,
                        route,
                        distribution,
                        percentages,
                        pctZeros
                    );
                    exposureSets.Add(set);
                }
            }
            var result = new ConsumerProductExposureAssessment() {
                IdAssessment = $"Fake_{i}",
                Name = $"Fake CP exposure assessment {i}",
                Description = $"Fake CP exposure assessment {i}",
                ExposureType = exposureType,
                Gender = gender,
                AgeMin = ageMin,
                AgeMax = ageMax,
                ExposureSets = exposureSets
            };
            return result;
        }

        private ConsumerProductExposureSet fakeExposureSet(
            Compound substance,
            ExposureRoute route,
            LogNormalDistribution distribution,
            double[] percentages,
            double percentageZeros = 0,
            ConsumerProductExposureEstimateUnit unit = ConsumerProductExposureEstimateUnit.ugPerKgBWPerDay,
            ConsumerProductExposureEstimateType estimateType = ConsumerProductExposureEstimateType.Systemic
        ) {
            var percentiles = percentages
                .Select(r => new ConsumerProductExposurePercentile() {
                    Percentage = r,
                    Value = distribution.InvCDF(r / 100)
                })
                .ToList();
            var result = new ConsumerProductExposureSet() {
                Substance = substance,
                ExposureRoutes = [route],
                EstimateType = estimateType,
                ConsumerProductExposureEstimateUnit = unit,
                PercentageZeros = percentageZeros,
                ExposurePercentiles = percentiles
            };
            return result;
        }
    }
}
