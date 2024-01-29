using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, Route
    /// </summary>
    [TestClass]
    public class UpperDistributionAggregateRouteSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize aggregate exposure routes chronic, test UpperDistributionAggregateRouteSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionAggregateRouteSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Dermal, ExposurePathType.Oral };
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                absorptionFactors
            );
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateIndividualExposures = MockAggregateIndividualIntakeGenerator.Create(
               individualDays,
               substances,
               exposureRoutes,
               kineticModelCalculators,
               externalExposuresUnit,
               random
            );

            var section = new UpperDistributionAggregateRouteSection();
            section.Summarize(
                aggregateIndividualExposures,
                null,
                substances,
                rpfs,
                memberships,
                absorptionFactors,
                exposureRoutes,
                25,
                75,
                95,
                ExposureType.Chronic,
                2.5,
                97.5,
                false
            );
            var sum = section.DistributionRouteUpperRecords.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new UpperDistributionAggregateRoutePieChartCreator(section, false);
            RenderChart(chart, $"TestCreate1");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize aggregate exposure routes acute, test UpperDistributionAggregateRouteSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionAggregateRouteSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(4);
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Dermal, ExposurePathType.Oral };
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                absorptionFactors
            );
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
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

            var section = new UpperDistributionAggregateRouteSection();
            section.Summarize(
                null,
                aggregateIndividualDayExposures,
                substances,
                rpfs,
                memberships,
                absorptionFactors,
                exposureRoutes,
                25,
                75,
                95,
                ExposureType.Acute,
                2.5,
                97.5,
                false
            );
            var sum = section.DistributionRouteUpperRecords.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new UpperDistributionAggregateRoutePieChartCreator(section, false);
            RenderChart(chart, $"TestCreate2");
            AssertIsValidView(section);
        }
    }
}
