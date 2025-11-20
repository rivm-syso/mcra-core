using MCRA.General;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.NonDietaryIntakeCalculation {

    /// <summary>
    /// NonDietaryIntakeCalculation calculator
    /// </summary>
    [TestClass]
    public class NonDietaryExposureGeneratorTests {

        /// <summary>
        /// Generate nondietary exposure, matched, uncorrelated, acute
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestMatchedAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(100, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(4);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator
                .Create(individuals, substances, routes, random, ExternalExposureUnit.mgPerKgBWPerDay);

            var calculator = new NonDietaryMatchedExposureGenerator(nonDietarySurveys);
            var result = calculator.GenerateAcute(
                [.. individualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.mgPerGBWPerDay,
                substances,
                routes,
                123456);
            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, matched, uncorrelated, chronic
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestMatchedChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(100, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(4);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator
                .Create(individuals, substances, routes, random, ExternalExposureUnit.mgPerKgBWPerDay);

            var calculator = new NonDietaryMatchedExposureGenerator(nonDietarySurveys);
            var result = calculator.GenerateChronic(
                [.. individualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.mgPerKgBWPerDay,
                substances,
                routes,
                123456);
            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, uncorrelated, acute
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(100, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(4);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.CreateUnmatched(substances, routes, random);

            var calculator = new NonDietaryUnmatchedExposureGenerator(nonDietarySurveys);
            var result = calculator.GenerateAcute(
                [.. individualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.ugPerKgBWPerDay,
                substances,
                routes,
                123456);

            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, uncorrelated, chronic
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(100, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(4);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.CreateUnmatched(substances, routes, random);

            var calculator = new NonDietaryUnmatchedExposureGenerator(nonDietarySurveys);
            var result = calculator.GenerateChronic(
                individualDays.Cast<IIndividualDay>().ToList(),
                nonDietarySurveys.Keys,
                ExternalExposureUnit.mgPerKgBWPerDay,
                substances,
                routes,
                123456);
            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, correlated, acute
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(100, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(4);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.CreateUnmatched(substances, routes, random);

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator(
                nonDietarySurveys,
                false,
                false
            );
            var result = calculator.GenerateAcute(
                [.. individualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.mgPerKgBWPerDay,
                substances,
                routes,
                123456
            );

            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, correlated, chronic
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(3, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.CreateUnmatched(substances, routes, random);

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator(
                nonDietarySurveys,
                false,
                false
            );
            var result = calculator.GenerateChronic(
                individualDays.Cast<IIndividualDay>().ToList(),
                nonDietarySurveys.Keys,
                ExternalExposureUnit.mgPerKgBWPerDay,
                substances,
                routes,
                123456);
            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, uncorrelated, acute, checks exposure per person/day and per kg/day
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal };
            var individuals = FakeIndividualsGenerator.Create(1, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator
                .CreateUnmatched(substances, routes, random);
            var calculator = new NonDietaryUnmatchedExposureGenerator(nonDietarySurveys);
            var result = calculator.GenerateChronic(
                [.. individualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.ugPerKgBWPerDay,
                substances,
                routes,
                seed);
            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, correlated, chronic, 2 surveys.
        /// Two surveys with 3 exposure sets each are generated.
        /// Checks whether the 6 simulated nondietary exposure sets contains only one exposure
        /// set from a survey (and not combines several).
        /// Checks whether the units are OK.
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator
                .CreateUnmatched(
                    substances,
                    routes,
                    random,
                    exposureUnit: ExternalExposureUnit.mgPerDay,
                    numSurveys: 2,
                    numExposures: 2,
                    numUncertaintySets: 0
                );

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator(
                nonDietarySurveys,
                false,
                false
            );
            var result = calculator.GenerateChronic(
                [.. individualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.ugPerKgBWPerDay,
                substances,
                routes,
                seed);

            // Compute expected exposures per route from input
            var expected = new Dictionary<ExposureRoute, List<double>>();
            var nonDietaryExposures = nonDietarySurveys
                .SelectMany(c => c.Value.SelectMany(r => r.NonDietaryExposures))
                .ToList();
            expected[ExposureRoute.Dermal] = nonDietaryExposures.Select(c => c.Dermal).ToList();
            expected[ExposureRoute.Oral] = nonDietaryExposures.Select(c => c.Oral).ToList();

            // Get generated exposures per route
            foreach (var route in routes) {
                var drawn = result.ExternalIndividualDayExposures
                    .SelectMany(c => c.ExposuresPerPath
                        .Where(c => c.Key.Route == route)
                        .SelectMany(c => c.Value)
                    )
                    .Select(c => c.Amount)
                    .ToList();
                foreach (var item in drawn) {
                    Assert.Contains(item, expected[route]);
                }
            }
        }

        /// <summary>
        /// Generate non-dietary exposure, unmatched, correlated, chronic, 2 surveys.
        /// Two surveys with 3 exposure sets each are generated.
        /// Checks whether the mean of the data equals the mean of the 10000 simulated
        /// non-dietary data with 6 exposure sets each.
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(3, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator
                .CreateUnmatched(substances, routes, random, ExternalExposureUnit.mgPerDay);
            var nonDietaryExposures = nonDietarySurveys.SelectMany(c => c.Value.SelectMany(r => r.NonDietaryExposures)).ToList();
            var dermal = nonDietaryExposures.Select(c => c.Dermal).ToList();
            var oral = nonDietaryExposures.Select(c => c.Oral).ToList();
            var sum = dermal.Average() + oral.Average();

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator(
                nonDietarySurveys,
                false,
                false
            );
            var sumRandom = 0d;
            var nIter = 100;
            for (int i = 0; i < nIter; i++) {
                var result = calculator.GenerateChronic(
                    individualDays.Cast<IIndividualDay>().ToList(),
                    nonDietarySurveys.Keys,
                    ExternalExposureUnit.ugPerKgBWPerDay,
                    substances,
                    routes,
                    random.Next());

                var dermalRandom = result.ExternalIndividualDayExposures
                    .SelectMany(c => c.ExposuresPerPath)
                    .Where(c => c.Key.Route == ExposureRoute.Dermal)
                    .SelectMany(c => c.Value)
                    .Select(c => c.Amount)
                    .ToHashSet();
                var oralRandom = result.ExternalIndividualDayExposures
                    .SelectMany(c => c.ExposuresPerPath)
                    .Where(c => c.Key.Route == ExposureRoute.Oral)
                    .SelectMany(c => c.Value)
                    .Select(c => c.Amount)
                    .ToHashSet();

                sumRandom += dermalRandom.Average() + oralRandom.Average();
            }
            var simulatedSum = sumRandom / nIter;
            Assert.AreEqual(sum, simulatedSum, 1e-1);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, correlated, chronic, 2 surveys.
        /// Two surveys with 3 exposure sets each are generated.
        /// Checks whether the mean of the data equals the mean of 10000 simulated nondietary sampling sets
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic4() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var substances = FakeSubstancesGenerator.Create(1);
            var individuals = FakeIndividualsGenerator.Create(3, 2, random, useSamplingWeights: false);
            var individualsSimulation = FakeIndividualsGenerator.Create(10000, 2, random, useSamplingWeights: false);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individualsSimulation);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator
                .CreateUnmatched(
                    substances,
                    routes,
                    random,
                    ExternalExposureUnit.mgPerDay,
                    numSurveys: 2
                );

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator(
                nonDietarySurveys,
                false,
                false
            );

            var result = calculator.GenerateChronic(
                [.. simulatedIndividualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.ugPerKgBWPerDay,
                substances,
                routes,
                random.Next()
            );

            // Compute expected
            var nonDietaryExposures = nonDietarySurveys
                .SelectMany(c => c.Value.SelectMany(r => r.NonDietaryExposures)).ToList();
            var dermal = nonDietaryExposures.Select(c => c.Dermal).ToList();
            var oral = nonDietaryExposures.Select(c => c.Oral).ToList();
            var sumExpected = dermal.Average() + oral.Average();

            // Simulated exposures
            var simulated = routes
                .Sum(r => result.ExternalIndividualDayExposures
                    .SelectMany(c => c.ExposuresPerPath)
                    .Where(c => c.Key.Route == r)
                    .SelectMany(c => c.Value)
                    .Select(c => c.Amount)
                    .Average()
                );
            Assert.AreEqual(sumExpected, simulated, 1e-2);
        }

        /// <summary>
        /// Generate non-dietary exposure, unmatched, correlated, chronic, 2 surveys.
        /// Two surveys with 50 exposure sets each are generated.
        /// Checks whether the mean of the data equals the mean of 100000 simulated nondietary sampling sets,
        /// Includes proportion of zeros. Note that the mean of the data should be weighted with the proportion of zero's in each survey
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic6() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(100000, 2, random);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var nSurveys = 2;
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator
                .CreateUnmatched(
                    substances,
                    routes,
                    random,
                    ExternalExposureUnit.mgPerDay,
                    numSurveys: nSurveys,
                    numExposures: 50,
                    correlated: true,
                    hasZeros: true
                );

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator(
                nonDietarySurveys,
                false,
                false
            );
            var result = calculator.GenerateChronic(
                [.. simulatedIndividualDays.Cast<IIndividualDay>()],
                nonDietarySurveys.Keys,
                ExternalExposureUnit.ugPerKgBWPerDay,
                substances,
                routes,
                random.Next()
            );

            // Compute expected
            var dermalSum = nonDietarySurveys
                .SelectMany(r => r.Value)
                .GroupBy(r => r.IndividualCode)
                .Average(r => r.Sum(c => c.NonDietaryExposures.Sum(nde => nde.Dermal)));
            var oralSum = nonDietarySurveys
                .SelectMany(r => r.Value)
                .GroupBy(r => r.IndividualCode)
                .Average(r => r.Sum(c => c.NonDietaryExposures.Sum(nde => nde.Oral)));
            var meanData = dermalSum + oralSum;

            // Calculate simulated
            var dermalRandom = result.ExternalIndividualDayExposures
                .SelectMany(c => c.ExposuresPerPath)
                .Where(c => c.Key.Route == ExposureRoute.Dermal)
                .SelectMany(c => c.Value)
                .Select(c => c.Amount)
                .ToList();
            var oralRandom = result.ExternalIndividualDayExposures
                .SelectMany(c => c.ExposuresPerPath)
                .Where(c => c.Key.Route == ExposureRoute.Oral)
                .SelectMany(c => c.Value)
                .Select(c => c.Amount)
                .ToList();
            var meanSimulated = (dermalRandom.Sum() + oralRandom.Sum()) / result.ExternalIndividualDayExposures.Count;

            // Assert
            Assert.AreEqual(meanData, meanSimulated, 1e-1);
        }
    }
}
