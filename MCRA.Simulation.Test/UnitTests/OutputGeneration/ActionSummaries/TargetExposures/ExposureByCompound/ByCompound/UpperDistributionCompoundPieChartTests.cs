using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var allRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var routes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random);
                var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
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

                var section = new UpperDistributionCompoundSection();
                section.Summarize(
                    aggregateIndividualExposures,
                    null,
                    substances,
                    rpfs,
                    memberships,
                    kineticConversionFactors,
                    97.5,
                    25,
                    75,
                    2.5,
                    97.5,
                    externalExposuresUnit,
                    targetUnit
                );
                if (aggregateIndividualExposures.Any(r => r.IsPositiveTargetExposure(targetUnit.Target)) && section.NumberOfIntakes > 0) {
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
            var allRoutes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            for (int numIndividuals = 0; numIndividuals < 100; numIndividuals++) {
                var routes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
                var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random);
                var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
                var rpfs = substances.ToDictionary(r => r, r => 1d);
                var memberships = substances.ToDictionary(r => r, r => 1d);
                var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
                var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                    substances,
                    kineticConversionFactors,
                    targetUnit
                );
                var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
                var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
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

                var section = new UpperDistributionCompoundSection();
                section.Summarize(
                    null,
                    aggregateIndividualDayExposures,
                    substances,
                    rpfs,
                    memberships,
                    kineticConversionFactors,
                    97.5,
                    25,
                    75,
                    2.5,
                    97.5,
                    externalExposuresUnit,
                    targetUnit
                );
                if (aggregateIndividualDayExposures.Any(r => r.IsPositiveTargetExposure(targetUnit.Target)) && section.NumberOfIntakes > 0) {
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
