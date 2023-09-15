using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, ByCompound
    /// </summary>
    [TestClass]
    public class UpperDistributionCompoundPieChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize chronic aggregate, create chart and test UpperDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionCompoundPieChart_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
                var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
                var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
                var aggregateIndividualExposures = MockAggregateIndividualIntakeGenerator.Create(
                    individualDays,
                    substances,
                    exposureRoutes,
                    kineticModelCalculators,
                    externalExposuresUnit,
                    random
                );

                var section = new UpperDistributionCompoundSection();
                section.Summarize(aggregateIndividualExposures, null, substances, rpfs, memberships, ExposureType.Chronic, 97.5, 25, 75, 2.5, 97.5, false);
                if (aggregateIndividualExposures.Any(r => r.TotalConcentrationAtTarget(rpfs, memberships, false) > 0D) && section.NRecords > 0) {
                    Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
                }
                Assert.AreEqual(substances.Count, section.Records.Count);

                var chart = new UpperDistributionCompoundPieChartCreator(section, false);
                RenderChart(chart, $"TestCreate1");
                AssertIsValidView(section);
            }
        }

        /// <summary>
        /// Summarize acute aggregate, create chart and test UpperDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionCompoundPieChart_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
                var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
                var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
                var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
                var aggregateIndividualDayExposures = MockAggregateIndividualDayIntakeGenerator
                    .Create(
                        individualDays,
                        substances,
                        exposureRoutes,
                        targetExposuresCalculator,
                        externalExposuresUnit,
                        random
                    );

                var section = new UpperDistributionCompoundSection();
                section.Summarize(null, aggregateIndividualDayExposures, substances, rpfs, memberships, ExposureType.Acute, 97.5, 25, 75, 2.5, 97.5, false);
                if (aggregateIndividualDayExposures.Any(r => r.TotalConcentrationAtTarget(rpfs, memberships, false) > 0D) && section.NRecords > 0) {
                    Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
                }
                Assert.AreEqual(substances.Count, section.Records.Count);

                var chart = new UpperDistributionCompoundPieChartCreator(section, false);
                RenderChart(chart, $"TestCreate2");
                AssertIsValidView(section);
            }
        }
    }
}
