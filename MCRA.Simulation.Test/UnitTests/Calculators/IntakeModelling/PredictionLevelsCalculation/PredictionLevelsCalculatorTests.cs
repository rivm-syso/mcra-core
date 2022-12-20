using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.IntakeModelling.PredictionLevelsCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {

    /// <summary>
    /// Covariate groups calculator tests.
    /// </summary>
    [TestClass]
    public class PredictionLevelsCalculatorTests {

        /// <summary>
        /// Test compute calculation of prediction levels from a number of desired
        /// intervals from exposures.
        /// </summary>
        [TestMethod]
        public void PredictionLevelsCalculator_TestIntervalsOnly() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 4, null);
            Assert.AreEqual(4, predictionLevels.Count);
        }

        /// <summary>
        /// Test compute calculation of prediction levels from a number of desired
        /// intervals from exposures combined with user specified levels.
        /// </summary>
        [TestMethod]
        public void PredictionLevelsCalculator_TestIntervalsAndUserSpecified() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var substances = MockSubstancesGenerator.Create(3);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, substances, 0.5, true, random);
            var predictionLevels = PredictionLevelsCalculator.ComputePredictionLevels(exposures, 4, new [] { 200D });
            Assert.AreEqual(5, predictionLevels.Count);
        }

        /// <summary>
        /// Test compute calculation of prediction levels from user specified levels only.
        /// </summary>
        [TestMethod]
        public void PredictionLevelsCalculator_TestUserSpecifiedOnly() {
            var predictionLevels = PredictionLevelsCalculator
                .ComputePredictionLevels((ICollection<IIndividualDay>)null, 4, new[] { 3D });
            Assert.AreEqual(1, predictionLevels.Count);
            Assert.AreEqual(3D, predictionLevels.First());
        }
    }
}
