using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    [TestClass]
    public class UpperDistributionAggregateRouteSectionTests : ChartCreatorTestBase {

        [TestMethod]
        public void UpperDistributionAggregateRouteSection_TestChronic() {
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
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(kineticModelCalculators);
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator.Create(
               individualDays,
               substances,
               paths,
               kineticModelCalculators,
               externalExposuresUnit,
               targetUnit,
               random
            );

            var section = new ContributionByRouteUpperSection();
            section.Summarize(
                aggregateIndividualExposures,
                null,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                95,
                2.5,
                97.5,
                targetUnit,
                externalExposuresUnit
            );
            var sum = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new ContributionByRouteUpperPieChartCreator(section, false);
            RenderChart(chart, $"TestChronic");
            AssertIsValidView(section);
        }

        [TestMethod]
        public void UpperDistributionAggregateRouteSection_TestAcute() {
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

            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(kineticModelCalculators);
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);
            var aggregateIndividualDayExposures = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    routes,
                    kineticModelCalculators,
                    externalExposuresUnit,
                    targetUnit,
                    random
                );

            var section = new ContributionByRouteUpperSection();
            section.Summarize(
                null,
                aggregateIndividualDayExposures,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                95,
                2.5,
                97.5,
                targetUnit,
                externalExposuresUnit
            );
            var sum = section.Records.Sum(c => c.ContributionPercentage);
            Assert.AreEqual(98D, sum, 3D);

            var chart = new ContributionByRouteUpperPieChartCreator(section, false);
            RenderChart(chart, $"TestAcute");
            AssertIsValidView(section);
        }
    }
}
