using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var substances = FakeSubstancesGenerator.Create(4);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, routes, .1);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    routes,
                    kineticModelCalculators,
                    externalExposuresUnit,
                    targetUnit,
                    random
                );

            var section = new UpperDistributionRouteCompoundSection();
            section.Summarize(
                aggregateIndividualExposures,
                null,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                25,
                75,
                95,
                2.5,
                97.5,
                targetUnit,
                externalExposuresUnit
            );
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
            var routes = FakeSubstancesGenerator.Create(4);
            var exposureRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var rpfs = routes.ToDictionary(r => r, r => 1d);
            var memberships = routes.ToDictionary(r => r, r => 1d);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(routes, exposureRoutes, .1);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                routes,
                kineticConversionFactors,
                targetUnit
            );
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateIndividualDayExposures = FakeAggregateIndividualDayExposuresGenerator.Create(
                individualDays,
                routes,
                exposureRoutes,
                kineticModelCalculators,
                externalExposuresUnit,
                targetUnit,
                random
            );

            var section = new UpperDistributionRouteCompoundSection();
            section.Summarize(
                null,
                aggregateIndividualDayExposures,
                routes,
                rpfs,
                memberships,
                kineticConversionFactors,
                25,
                75,
                95,
                2.5,
                97.5,
                targetUnit,
                externalExposuresUnit
            );
            var sum = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new UpperDistributionRouteCompoundPieChartCreator(section, false);
            RenderChart(chart, $"TestCreate2");
            AssertIsValidView(section);
        }
    }
}
