using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var allRoutes = new List<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = MockIndividualsGenerator.Create(numIndividuals, 2, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = MockNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

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
            var allRoutes = new List<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = MockIndividualsGenerator.Create(numIndividuals, 2, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = MockNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

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
