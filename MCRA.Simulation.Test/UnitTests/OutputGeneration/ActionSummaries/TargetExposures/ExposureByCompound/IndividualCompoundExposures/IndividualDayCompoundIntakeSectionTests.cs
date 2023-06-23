using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries,TargetExposures, ExposureByCompound, IndividualCompoundExposures
    /// </summary>
    [TestClass]
    public class IndividualDayCompoundIntakeSectionTests : SectionTestBase {
        /// <summary>
        /// Test IndividualDayCompoundIntakeSection view
        /// </summary>
        [TestMethod]
        public void IndividualDayCompoundIntakeSection_TestValidView() {
            var section = new IndividualDayCompoundIntakeSection() {
                IndividualCompoundIntakeRecords = new List<IndividualDayCompoundIntakeRecord>() {
                    new IndividualDayCompoundIntakeRecord() {
                        Bodyweight = 75,
                        CumulativeExposure = 1.234,
                        ExposureAmount = 2.468,
                        SimulatedIndividualDayId = "12345",
                        RelativeCompartmentWeight = 0.023,
                        SamplingWeight = 1,
                        SubstanceCode = "C",
                        DietarySurveyDayCode = "1",
                        DietarySurveyIndividualCode ="12345"
                    }
                },
            };
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test IndividualDayCompoundIntakeSection summarize by substance
        /// </summary>
        [TestMethod]
        public void IndividualDayCompoundIntakeSection_TestSummarizeBySubstance() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new IndividualDayCompoundIntakeSection();
            var allRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, false, random);
            var substances = MockSubstancesGenerator.Create(5);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateExposures = MockAggregateIndividualDayIntakeGenerator.Create(
                simulatedIndividualDays: individualDays,
                substances: substances,
                exposureRoutes: exposureRoutes,
                targetExposuresCalculator: targetExposuresCalculator,
                targetUnit: TargetUnit.FromDoseUnit(DoseUnit.ugPerKgBWPerDay, BiologicalMatrix.Urine),
                random: random);
            section.Summarize(aggregateExposures, substances, rpfs, memberships);
            var positives = aggregateExposures.SelectMany(r => r.TargetExposuresBySubstance).Count(r => r.Value.SubstanceAmount > 0);
            Assert.AreEqual(positives, section.IndividualCompoundIntakeRecords.Count);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test IndividualDayCompoundIntakeSection summarize by substance
        /// </summary>
        [TestMethod]
        public void IndividualDayCompoundIntakeSection_TestSummarizeTotal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var section = new IndividualDayCompoundIntakeSection();
            var allRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };
            var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, false, random);
            var substances = MockSubstancesGenerator.Create(2);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var aggregateExposures = MockAggregateIndividualDayIntakeGenerator.Create(
                simulatedIndividualDays: individualDays,
                substances: substances,
                exposureRoutes: exposureRoutes,
                targetExposuresCalculator: targetExposuresCalculator,
                targetUnit: TargetUnit.FromDoseUnit(DoseUnit.ugPerKgBWPerDay, BiologicalMatrixConverter.FromString("Urine")),
                random: random);
            section.Summarize(aggregateExposures, substances, rpfs, memberships, substances.First(), true);
            var positives = aggregateExposures.Count(r => r.TargetExposuresBySubstance.Any(e => e.Value.SubstanceAmount > 0));
            Assert.AreEqual(positives, section.IndividualCompoundIntakeRecords.Count);
            AssertIsValidView(section);
        }
    }
}