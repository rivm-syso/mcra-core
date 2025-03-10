﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.NonDietaryExposures;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the NonDietaryExposures action
    /// </summary>
    [TestClass]
    public class NonDietaryExposuresActionCalculatorTests : ActionCalculatorTestsBase {
        /// <summary>
        /// Runs the NonDietaryExposures action: load data, summarize action result, load data uncertain
        /// </summary>
        [TestMethod]
        public void NonDietaryExposuresActionCalculator_TestSets() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var substances = FakeSubstancesGenerator.Create(3);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Inhalation, ExposureRoute.Oral };
            var nondietaryExposureSets = FakeNonDietaryExposureSetsGenerator.MockNonDietaryExposureSets(individuals, substances, routes, random, ExternalExposureUnit.mgPerKgBWPerDay);

            var compiledData = new CompiledData() {
                NonDietaryExposureSets = nondietaryExposureSets,
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var data = new ActionData() {
                ActiveSubstances = substances
            };
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new NonDietaryExposuresActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.IsNotNull(data.NonDietaryExposureSets);
            Assert.IsNotNull(data.NonDietaryExposures);
            Assert.IsNotNull(data.NonDietaryExposureRoutes);
            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = [UncertaintySource.NonDietaryExposures]
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.NonDietaryExposures] = random
            };

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }
    }
}
