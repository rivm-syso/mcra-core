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
    public class TotalDistributionCompoundPieChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Summarize chronic aggregate, create chart and test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundPieChart_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
                var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
                var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
                var aggregateIndividualExposures = MockAggregateIndividualIntakeGenerator
                    .Create(
                        individualDays,
                        substances,
                        exposureRoutes,
                        kineticModelCalculators,
                        externalExposuresUnit,
                        random
                    );

                var section = new TotalDistributionCompoundSection();
                section.Summarize(aggregateIndividualExposures, null, rpfs, memberships, substances, 25, 75, 2.5, 97.5, false);
                if (aggregateIndividualExposures.Any(r => r.TotalConcentrationAtTarget(rpfs, memberships, false) > 0D)) {
                    Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
                }
                Assert.AreEqual(substances.Count, section.Records.Count);
                var chart = new TotalDistributionCompoundPieChartCreator(section, false);
                RenderChart(chart, $"TestCreate1{numIndividuals}");
                AssertIsValidView(section);
            }
        }
        /// <summary>
        /// Summarize acute aggregate, create chart and test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionCompoundPieChart_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var allRoutes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random);
                var substances = MockSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
                var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
                var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
                var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
                var aggregateIndividualDayExposures = MockAggregateIndividualDayIntakeGenerator
                    .Create(
                        individualDays,
                        substances,
                        exposureRoutes,
                        targetExposuresCalculator,
                        externalExposuresUnit,
                        random
                    );

                var section = new TotalDistributionCompoundSection();
                section.Summarize(null, aggregateIndividualDayExposures, rpfs, memberships, substances, 25, 75, 2.5, 97.5, false);
                if (aggregateIndividualDayExposures.Any(r => r.TotalConcentrationAtTarget(rpfs, memberships, false) > 0D)) {
                    Assert.AreEqual(100D, section.Records.Sum(c => c.ContributionPercentage), .001);
                }
                Assert.AreEqual(substances.Count, section.Records.Count);

                var chart = new TotalDistributionCompoundPieChartCreator(section, false);
                RenderChart(chart, $"TestCreate2{numIndividuals}");
                AssertIsValidView(section);
            }
        }
    }
}
