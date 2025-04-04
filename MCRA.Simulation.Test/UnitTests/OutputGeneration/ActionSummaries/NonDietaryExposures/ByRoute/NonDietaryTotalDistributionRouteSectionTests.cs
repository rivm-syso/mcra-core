﻿using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.NonDietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, NonDietaryExposures, ByRoute
    /// </summary>
    [TestClass]
    public class NonDietaryTotalDistributionRouteSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize nondietary acute, create chart, test NonDietaryTotalDistributionRouteSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryTotalDistributionRouteSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = FakeIndividualsGenerator.CreateSimulated(numIndividuals, 2, random);
                var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = FakeNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

                var section = new NonDietaryTotalDistributionRouteSection();
                section.Summarize(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Acute, 25, 75, 2.5, 97.5, false);
                Assert.AreEqual(nonDietaryIntakes.Any() ? nonDietaryExposureRoutes.Count : 0, section.Records.Count);
                section.SummarizeUncertainty(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Chronic, false);

                var chart = new NonDietaryTotalDistributionRoutePieChartCreator(section, false);
                RenderChart(chart, $"TestAcute");
                AssertIsValidView(section);
            }
        }
        /// <summary>
        ///  Summarizes nondietary chronic, create chart, test NonDietaryTotalDistributionRouteSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryTotalDistributionRouteSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = FakeIndividualsGenerator.CreateSimulated(numIndividuals, 2, random);
                var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = FakeNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

                var section = new NonDietaryTotalDistributionRouteSection();
                section.Summarize(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Chronic, 25, 75, 2.5, 97.5, false);
                Assert.AreEqual(nonDietaryIntakes.Any() ? nonDietaryExposureRoutes.Count : 0, section.Records.Count);
                section.Summarize(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Acute, 25, 75, 2.5, 97.5, false);
                section.SummarizeUncertainty(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Chronic, false);

                var chart = new NonDietaryTotalDistributionRoutePieChartCreator(section, false);
                RenderChart(chart, $"TestChronic");
                AssertIsValidView(section);
            }
        }
    }
}
