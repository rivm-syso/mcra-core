using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRouteCompound
    /// </summary>
    [TestClass]
    public class UpperDistributionRouteCompoundSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize aggregate exposure routes chronic, create chart, test UpperDistributionRouteCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionRouteCompound_TestChronic() {
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
            var targetUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = MockAggregateIndividualIntakeGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                kineticModelCalculators,
                targetUnit,
                random
            );

            var section = new UpperDistributionRouteCompoundSection();
            section.Summarize(aggregateIndividualExposures, null, substances, rpfs, memberships, absorptionFactors, ExposureType.Chronic, 25, 75, 95, 2.5, 97.5, false);
            var sum = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new UpperDistributionRouteCompoundPieChartCreator(section, false);
            RenderChart(chart, $"TestCreate1");
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize aggregate exposure routes acute, create chart, test UpperDistributionRouteCompoundSection view
        /// </summary>
        [TestMethod]
        public void UpperDistributionRouteCompound_TestAcute() {
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
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateIndividualDayExposures = MockAggregateIndividualDayIntakeGenerator.Create(
                individualDays,
                substances,
                exposureRoutes,
                targetExposuresCalculator,
                new TargetUnit(ExposureUnit.mgPerKgBWPerDay),
                random
            );

            var section = new UpperDistributionRouteCompoundSection();
            section.Summarize(null, aggregateIndividualDayExposures, substances, rpfs, memberships, absorptionFactors, ExposureType.Acute, 25, 75, 95, 2.5, 97.5, false);
            var sum = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new UpperDistributionRouteCompoundPieChartCreator(section, false);
            RenderChart(chart, $"TestCreate2");
            AssertIsValidView(section);
        }
    }
}
