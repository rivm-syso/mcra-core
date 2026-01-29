using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, ByCompound
    /// </summary>
    [TestClass]
    public class ContributionBySubstanceTotalPieChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Summarize chronic aggregate, create chart and test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void ContributionBySubstanceTotalPieChartCreator_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            for (int numIndividuals = 1; numIndividuals < 100; numIndividuals++) {
                var paths = FakeExposurePathGenerator.Create([.. routes]);
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
                var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);


                var section = new ContributionBySubstanceTotalSection();
                section.Summarize(
                    individualExposures,
                    substances,
                    rpfs,
                    memberships,
                    kineticConversionFactors,
                    2.5,
                    97.5,
                    false
                );
        
                Assert.HasCount(substances.Count, section.Records);
                var chart = new ContributionBySubstanceTotalPieChartCreator(section, false);
                RenderChart(chart, $"TestCreate1{numIndividuals}");
                AssertIsValidView(section);
            }
        }
    }
}
