using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    [TestClass]
    public class UpperDistributionExternalRouteSectionTests : ChartCreatorTestBase {

        [TestMethod]
        public void UpperDistributionExternalRouteSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);
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
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(kineticModelCalculators);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);
            var section = new ContributionByRouteUpperSection();
            section.Summarize(
                individualExposures,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                95,
                2.5,
                97.5,
                false
            );
            var sum = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new ContributionByRouteUpperPieChartCreator(section, false);
            RenderChart(chart, $"TestChronic");
            AssertIsValidView(section);
        }

        [TestMethod]
        public void UpperDistributionExternalouteSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(4);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);
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

            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(kineticModelCalculators);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);
            var section = new ContributionByRouteUpperSection();
            section.Summarize(
                individualExposures,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                95,
                2.5,
                97.5,
                false
            );
            var sum = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new ContributionByRouteUpperPieChartCreator(section, false);
            RenderChart(chart, $"TestAcute");
            AssertIsValidView(section);
        }
    }
}
