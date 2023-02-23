using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.UnitVariabilityCalculation {
    /// <summary>
    /// UnitVariabilityCalculation calculator
    /// </summary>
    [TestClass]
    public class BernoulliDistributionModelTests {

        /// <summary>
        /// Test Bernoulli distribution model. Generates a model using and draws
        /// a number of times from this model.
        /// </summary>
        [TestMethod]
        public void BernoulliDistributionModelTest1() {
            var vf = 5;
            var unitsCompositeSample = 24;
            var food = new Food() {
                Name = "Apple",
            };
            var foodProperty = new FoodProperty() {
                UnitWeight = 100,
            };
            food.Properties = foodProperty;
            var uvFactor = new UnitVariabilityFactor() {
                Factor = vf,
                UnitsInCompositeSample = unitsCompositeSample,
            };

            // Create unit variability model
            var uvCalculator = new BernoulliDistributionModel(food, uvFactor, EstimatesNature.Realistic);
            uvCalculator.CalculateParameters();

            // Draw n times
            var values = new List<IntakePortion>();
            var random = new McraRandomGenerator();
            var intakePortion = new IntakePortion() {
                Amount = 100,
                Concentration = 100,
            };
            for (int i = 0; i < 10000; ++i) {
                values.AddRange(uvCalculator.DrawFromDistribution(food, intakePortion, random));
            }
            var concentrations = values.Select(r => (double)r.Concentration).ToList();

            // Derive variability factor from drawn concentrations
            var mean = concentrations.Average();
            var p97p5 = concentrations.Percentile(97.5);
            var vf_computed = p97p5 / mean;

            // Another way of deriving the unit variability factor
            var svf = concentrations.Select(c => c / mean).ToList();
            var svfp97p5 = concentrations.Select(c => c / mean).ToList().Percentile(97.5);
        }
    }
}
