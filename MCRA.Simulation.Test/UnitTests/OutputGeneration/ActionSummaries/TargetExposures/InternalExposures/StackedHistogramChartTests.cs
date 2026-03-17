using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByCompound, ByCompound
    /// </summary>
    [TestClass]
    public class StackedHistogramChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Summarize chronic aggregate, create chart and test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void StackedHistogramChartTests_TestRoutes() {
            var seed = 1;
            var numIndividuals = 100;
            var random = new McraRandomGenerator(seed);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var paths = FakeExposurePathGenerator.Create([.. routes]);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random);
            var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = FakeAbsorptionFactorsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var externalExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    paths,
                    kineticModelCalculators,
                    externalExposureUnit,
                    targetUnit,
                    random
                );

            var section = new InternalDistributionTotalSection();
      
            section.SummarizeCategorizedBins(
                aggregateIndividualExposures,
                rpfs,
                memberships,
                routes,
                kineticConversionFactors,
                externalExposureUnit,
                targetUnit
            );
            var chart = new InternalChronicStackedHistogramChartCreator(section, targetUnit.Target.GetDisplayName());
  
            RenderChart(chart, $"StackedHistogramRoutesTest");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize chronic aggregate, create chart and test TotalDistributionCompoundSection view
        /// </summary>
        [TestMethod]
        public void StackedHistogramChartTests_TestStratified() {
            var seed = 1;
            var numIndividuals = 100;
            var random = new McraRandomGenerator(seed);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var paths = FakeExposurePathGenerator.Create([.. routes]);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random, properties);
            var substances = FakeSubstancesGenerator.Create(random.Next(1, 4));
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = FakeAbsorptionFactorsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = FakeKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(
                substances,
                kineticConversionFactors,
                targetUnit
            );
            var externalExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateIndividualExposures = FakeAggregateIndividualExposuresGenerator
                .Create(
                    individualDays,
                    substances,
                    paths,
                    kineticModelCalculators,
                    externalExposureUnit,
                    targetUnit,
                    random
                );

            var section = new InternalDistributionTotalSection();

            PopulationStratifier stratifier = new GenderStratifier();
            section.SummarizeStratifiedBinsGraph(
                aggregateIndividualExposures,
                rpfs,
                memberships,
                stratifier,
                targetUnit
            );
            var chart = new StratifiedStackedHistogramChartCreator(section, targetUnit.Target.GetDisplayName());

            RenderChart(chart, $"StackedHistogramStratifierTest");
        }
    }
}
