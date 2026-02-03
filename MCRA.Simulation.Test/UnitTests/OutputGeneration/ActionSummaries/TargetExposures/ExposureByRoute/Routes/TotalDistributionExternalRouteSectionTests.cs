using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, Route
    /// </summary>
    [TestClass]
    public class TotalDistributionExternalRouteSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize aggregate exposure routes chronic, create chart, test TotalDistributionAggregateRouteSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionExternalRouteSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal };
            var paths = FakeExposurePathGenerator.Create(routes);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var kineticConversionFactors = FakeAbsorptionFactorsGenerator.CreateAbsorptionFactors(substances, routes, .1);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);
            var section = new ExposureByRouteSection();
            section.Summarize(
                individualExposures,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                25,
                75,
                targetUnit,
                false,
                false
            );
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize aggregate exposure routes acute, create chart, test TotalDistributionAggregateRouteSection view
        /// </summary>
        [TestMethod]
        public void TotalDistributionExternalRouteSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, true, random);
            var kineticConversionFactors = FakeAbsorptionFactorsGenerator.CreateAbsorptionFactors(substances, .1);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);
            var section = new ExposureByRouteSection();
            section.Summarize(
                individualExposures,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                25,
                75,
                targetUnit,
                false, 
                false
            );
            AssertIsValidView(section);
        }
    }
}
