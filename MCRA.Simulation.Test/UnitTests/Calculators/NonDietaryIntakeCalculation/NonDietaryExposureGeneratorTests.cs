﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.mgPerKgBWPerDay, 1, true);

            var calculator = new NonDietaryMatchedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
            var result = calculator.GenerateAcute(
                individualDays.Cast<IIndividualDay>().ToList(),
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
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.mgPerKgBWPerDay, 1, true);

            var calculator = new NonDietaryMatchedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
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
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random);

            var calculator = new NonDietaryUnmatchedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
            var result = calculator.GenerateAcute(
                individualDays.Cast<IIndividualDay>().ToList(),
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
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random);

            var calculator = new NonDietaryUnmatchedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
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
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random);

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
            var result = calculator.GenerateAcute(
                individualDays.Cast<IIndividualDay>().ToList(),
                nonDietarySurveys.Keys,
                ExternalExposureUnit.mgPerKgBWPerDay,
                substances,
                routes,
                123456);
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
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random);

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
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
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var memberships = substances.ToDictionary(c => c, c => 1d);
            var kineticConversionFactors = new Dictionary<(ExposureRoute, Compound), double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    kineticConversionFactors[(route, substance)] = 1d;
                }
            }

            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.mgPerDay);

            var calculator = new NonDietaryUnmatchedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
            var result = calculator.GenerateChronic(
                individualDays.Cast<IIndividualDay>().ToList(),
                nonDietarySurveys.Keys,
                ExternalExposureUnit.ugPerKgBWPerDay,
                substances,
                routes,
                123456);
            Assert.AreEqual(result.ExternalIndividualDayExposures.Count, individualDays.Count);
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, correlated, chronic, 2 surveys.
        /// Two surveys with 3 exposure sets each are generated.
        /// Checks whether the 6 simulated nondietary exposure sets contains only one exposure set from a survey (and not combines several)
        /// Checks whether the units are OK.
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(3, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.mgPerDay, 2);
            var nonDietaryExposures = nonDietarySurveys.SelectMany(c => c.Value.SelectMany(r => r.NonDietaryExposures)).ToList();
            var dermal = nonDietaryExposures.Select(c => c.Dermal).ToList();
            var oral = nonDietaryExposures.Select(c => c.Oral).ToList();

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator();
            calculator.Initialize(nonDietarySurveys);
            var result = calculator.GenerateChronic(
                individualDays.Cast<IIndividualDay>().ToList(),
                nonDietarySurveys.Keys,
                ExternalExposureUnit.ugPerKgBWPerDay,
                substances,
                routes,
                123456);

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

            foreach (var item in dermalRandom) {
                Assert.IsTrue(dermal.Contains(item));
            }
            foreach (var item in oralRandom) {
                Assert.IsTrue(oral.Contains(item));
            }
        }

        /// <summary>
        /// Generate nondietary exposure, unmatched, correlated, chronic, 2 surveys.
        /// Two surveys with 3 exposure sets each are generated.
        /// Checks whether the mean of the data equals the mean of the 10000 simulated nondietary data with 6 exposure sets each
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(3, 2, random, useSamplingWeights: false);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.mgPerDay, 2);
            var nonDietaryExposures = nonDietarySurveys.SelectMany(c => c.Value.SelectMany(r => r.NonDietaryExposures)).ToList();
            var dermal = nonDietaryExposures.Select(c => c.Dermal).ToList();
            var oral = nonDietaryExposures.Select(c => c.Oral).ToList();
            var sum = dermal.Average() + oral.Average();

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator();
            var sumRandom = 0d;
            var nIter = 100;
            for (int i = 0; i < nIter; i++) {
                calculator.Initialize(nonDietarySurveys);
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
            var individuals = FakeIndividualsGenerator.Create(3, 2, random, useSamplingWeights: false);
            var substances = FakeSubstancesGenerator.Create(1);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.mgPerDay, 2);
            var nonDietaryExposures = nonDietarySurveys.SelectMany(c => c.Value.SelectMany(r => r.NonDietaryExposures)).ToList();
            var dermal = nonDietaryExposures.Select(c => c.Dermal).ToList();
            var oral = nonDietaryExposures.Select(c => c.Oral).ToList();
            var sum = dermal.Average() + oral.Average();

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator();
            var sumRandom = 0d;
            var individualsSimulation = FakeIndividualsGenerator.Create(10000, 2, random, useSamplingWeights: false);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individualsSimulation);
            calculator.Initialize(nonDietarySurveys);
            var result = calculator.GenerateChronic(
                simulatedIndividualDays.Cast<IIndividualDay>().ToList(),
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

            sumRandom = dermalRandom.Average() + oralRandom.Average();
            Assert.AreEqual(sum, sumRandom, 1e-2);
        }


        /// <summary>
        /// Generate nondietary exposure, unmatched, correlated, chronic, 2 surveys.
        /// Two surveys with 50 exposure sets each are generated.
        /// Checks whether the mean of the data equals the mean of 100000 simulated nondietary sampling sets,
        /// Includes proportion of zeros. Note that the mean of the data should be weighted with the proportion of zero's in each survey
        /// </summary>
        [TestMethod]
        public void NonDietaryExposureGenerator_TestUnmatchedCorrelatedChronic6() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRoute>() { ExposureRoute.Dermal, ExposureRoute.Oral };
            var individuals = FakeIndividualsGenerator.Create(50, 2, random, useSamplingWeights: false);
            var substances = FakeSubstancesGenerator.Create(1);
            bool proportionZeros = true;
            var nSurveys = 2;
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, routes, random, ExternalExposureUnit.mgPerDay, nSurveys, false, proportionZeros);
            var proportion = nonDietarySurveys.Select(c => c.Key.ProportionZeros).ToList();

            var nonDietaryExposures = nonDietarySurveys.SelectMany(c => c.Value.SelectMany(r => r.NonDietaryExposures)).ToList();
            var dermal = nonDietaryExposures.Select(c => c.Dermal).ToList();
            var oral = nonDietaryExposures.Select(c => c.Oral).ToList();
            var ii = 0;
            var dermalSum = 0d;
            var oralSum = 0d;
            for (int i = 0; i < nSurveys; i++) {
                dermalSum += dermal.Skip(ii * nSurveys).Take(nSurveys).Average() * (100 - proportion[i]) / 100;
                oralSum += oral.Skip(ii * nSurveys).Take(nSurveys).Average() * (100 - proportion[i]) / 100;
                ii++;
            }
            var meanData = (dermalSum + oralSum) / nSurveys;

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator();
            var individualsSimulation = FakeIndividualsGenerator.Create(100000, 2, random, useSamplingWeights: false);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individualsSimulation);
            calculator.Initialize(nonDietarySurveys);
            var result = calculator.GenerateChronic(
                simulatedIndividualDays.Cast<IIndividualDay>().ToList(),
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
                .ToList();
            var oralRandom = result.ExternalIndividualDayExposures
                .SelectMany(c => c.ExposuresPerPath)
                .Where(c => c.Key.Route == ExposureRoute.Oral)
                .SelectMany(c => c.Value)
                .Select(c => c.Amount)
                .ToList();
            var meanSimulated = (dermalRandom.Sum() + oralRandom.Sum()) / result.ExternalIndividualDayExposures.Count;
            Assert.AreEqual(meanData, meanSimulated, 1e-1);
            var inputSection = new NonDietaryExposuresSummarySection();
            inputSection.Summarize(nonDietarySurveys, substances, 25, 75);
        }
    }
}
