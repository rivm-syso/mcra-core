using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, Route
    /// </summary>
    [TestClass]
    public class TotalDistributionAggregateRouteSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize aggregate exposure routes chronic, create chart, test TotalDistributionAggregateRouteSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionAggregateRouteSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral };
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, exposureRoutes, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                absorptionFactors
            );
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = MockAggregateIndividualIntakeGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                kineticModelCalculators,
                externalExposuresUnit,
                random
            );

            var section = new TotalDistributionAggregateRouteSection();
            section.Summarize(aggregateIndividualExposures, null, exposureRoutes, rpfs, memberships, absorptionFactors, 25, 75, 2.5, 97.5, false);
            var sum = section.DistributionRouteTotalRecords.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new TotalDistributionAggregateRoutePieChartCreator(section, false);
            RenderChart(chart, $"TestCreate1");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize aggregate exposure routes acute, create chart, test TotalDistributionAggregateRouteSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionAggregateRouteSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral };
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                absorptionFactors
            );
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateIndividualDayExposures = MockAggregateIndividualDayIntakeGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                targetExposuresCalculator,
                ExposureUnitTriple.FromExposureUnit(ExposureUnit.mgPerKgBWPerDay),
                random
            );

            var section = new TotalDistributionAggregateRouteSection();
            section.Summarize(null, aggregateIndividualDayExposures, exposureRoutes, rpfs, memberships, absorptionFactors, 25, 75, 2.5, 97.5, false);
            var sum = section.DistributionRouteTotalRecords.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new TotalDistributionAggregateRoutePieChartCreator(section, false);
            RenderChart(chart, $"TestCreate2");
            AssertIsValidView(section);
        }
    }
}
