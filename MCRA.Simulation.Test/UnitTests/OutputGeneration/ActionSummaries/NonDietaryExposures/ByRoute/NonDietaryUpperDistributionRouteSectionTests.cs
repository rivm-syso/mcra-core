using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.NonDietaryExposures {
    /// <summary>
    /// OutputGeneration, NonDietaryExposures, ByRoute
    /// </summary>
    [TestClass]
    public class NonDietaryUpperDistributionRouteSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize nondietary acute, create chart, test NonDietaryUpperDistributionRouteSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryUpperDistributionRouteSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new [] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals += 10) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = FakeIndividualsGenerator.Create(numIndividuals, 1, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = MockNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

                var section = new NonDietaryUpperDistributionRouteSection();
                section.Summarize(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Acute, 99, 25, 75, 2.5, 97.5, false);
                section.SummarizeUncertainty(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Acute, 99, false);

                Assert.AreEqual(nonDietaryIntakes.Any() ? nonDietaryExposureRoutes.Count : 0, section.Records.Count);

                var chart = new NonDietaryUpperDistributionRoutePieChartCreator(section, false);
                var plotModel = chart.Create();
                Assert.IsNotNull(plotModel);
                AssertIsValidView(section);
            }
        }
        /// <summary>
        /// Summarize nondietary acute, create chart, test NonDietaryUpperDistributionRouteSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryUpperDistributionRouteSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals += 10) {
                var nonDietaryExposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individuals = FakeIndividualsGenerator.Create(numIndividuals, 2, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1D);
                var nonDietaryIntakes = MockNonDietaryIndividualDayIntakeGenerator.Generate(individuals, substances, nonDietaryExposureRoutes, 0, random);

                var section = new NonDietaryUpperDistributionRouteSection();
                section.Summarize(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Chronic, 99, 25, 75, 2.5, 97.5, false);
                Assert.AreEqual(nonDietaryIntakes.Any() ? nonDietaryExposureRoutes.Count : 0, section.Records.Count);
                section.SummarizeUncertainty(nonDietaryIntakes, rpfs, memberships, nonDietaryExposureRoutes, ExposureType.Chronic, 99,false);

                var chart = new NonDietaryUpperDistributionRoutePieChartCreator(section, false);
                var plotModel = chart.Create();
                Assert.IsNotNull(plotModel);
                AssertIsValidView(section);
            }
        }
    }
}
