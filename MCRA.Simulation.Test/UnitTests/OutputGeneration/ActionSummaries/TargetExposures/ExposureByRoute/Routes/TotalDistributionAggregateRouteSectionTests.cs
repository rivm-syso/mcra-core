using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var substances = FakeSubstancesGenerator.Create(4);
            var routes = new [] { ExposureRoute.Dermal, ExposureRoute.Oral };
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
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator.Create(
                individualDays,
                substances,
                routes,
                kineticModelCalculators,
                externalExposuresUnit,
                targetUnit,
                random
            );

            var section = new ExposuresByRouteSection();
            section.Summarize(
                aggregateIndividualExposures,
                null,
                routes,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                25,
                75,
                targetUnit,
                externalExposuresUnit,
                false
            );
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize aggregate exposure routes acute, create chart, test TotalDistributionAggregateRouteSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionAggregateRouteSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateIndividualDayExposures = FakeAggregateIndividualDayExposuresGenerator.Create(
                individualDays,
                substances,
                routes,
                kineticModelCalculators,
                externalExposuresUnit,
                targetUnit,
                random
            );

            var section = new ExposuresByRouteSection();
            section.Summarize(
                null,
                aggregateIndividualDayExposures,
                routes,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                25,
                75,
                targetUnit,
                externalExposuresUnit,
                false
            );
            AssertIsValidView(section);
        }
    }
}
