using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.NonDietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, NonDietaryExposures, ByRouteCompound
    /// </summary>
    [TestClass]
    public class NonDietaryUpperDistributionRouteCompoundSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize nondietary acute, create chart, test NonDietaryUpperDistributionRouteCompoundSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryUpperDistributionRouteCompoundSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals += 10) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = MockIndividualsGenerator.Create(numIndividuals, 2, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = MockNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

                var section = new NonDietaryUpperDistributionRouteCompoundSection();
                section.Summarize(substances, nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Acute, 25, 75, 90, 2.5, 97.5, false);
                section.SummarizeUncertainty(substances, nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Acute, 90, false);

                var expectedRecods = nonDietaryIntakes.Any() ? nonDietaryExposureRoutes.Count * substances.Count : 0;
                Assert.AreEqual(expectedRecods, section.Records.Count);

                var chart = new NonDietaryUpperDistributionRouteCompoundPieChartCreator(section, false);
                RenderChart(chart,$"TestAcute");
                AssertIsValidView(section);
            }
        }
        /// <summary>
        /// Summarize nondietary chronic, create chart, test NonDietaryUpperDistributionRouteCompoundSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryUpperDistributionRouteSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals += 10) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = MockIndividualsGenerator.Create(numIndividuals, 2, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = MockNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

                var section = new NonDietaryUpperDistributionRouteCompoundSection();
                section.Summarize(substances, nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Chronic, 25, 75, 90, 2.5, 97.5, false);
                section.SummarizeUncertainty(substances, nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Chronic, 90, false);

                var expectedRecods = nonDietaryIntakes.Any() ? nonDietaryExposureRoutes.Count * substances.Count : 0;
                Assert.AreEqual(expectedRecods, section.Records.Count);

                var chart = new NonDietaryUpperDistributionRouteCompoundPieChartCreator(section, false);
                RenderChart(chart, $"TestChronic");
                AssertIsValidView(section);
            }
        }
    }
}
