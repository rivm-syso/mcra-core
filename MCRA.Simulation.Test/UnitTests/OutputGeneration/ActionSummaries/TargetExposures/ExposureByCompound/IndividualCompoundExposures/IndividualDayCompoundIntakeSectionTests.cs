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
                IndividualCompoundIntakeRecords = [
                    new IndividualDayCompoundIntakeRecord() {
                        Bodyweight = 75,
                        CumulativeExposure = 1.234,
                        Exposure = 2.468,
                        SimulatedIndividualDayId = "12345",
                        SamplingWeight = 1,
                        SubstanceCode = "C",
                        DietarySurveyDayCode = "1",
                        DietarySurveyIndividualCode ="12345"
                    }
                ],
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
            var allRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, false, random);
            var substances = MockSubstancesGenerator.Create(5);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, kineticConversionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var exposureTripleUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateExposures = FakeAggregateIndividualDayExposuresGenerator.Create(
                simulatedIndividualDays: individualDays,
                substances: substances,
                exposureRoutes: exposureRoutes,
                kineticModelCalculators: kineticModelCalculators,
                exposureTripleUnit,
                targetUnit,
                random: random
            );
            section.Summarize(aggregateExposures, substances, rpfs, memberships, kineticConversionFactors, targetUnit, exposureTripleUnit);
            var positives = aggregateExposures
                .SelectMany(r => r.InternalTargetExposures[targetUnit.Target])
                .Count(r => r.Value.Exposure > 0);
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
            var allRoutes = new[] { ExposurePathType.Dermal, ExposurePathType.Oral, ExposurePathType.Inhalation };
            var exposureRoutes = allRoutes.Where(r => random.NextDouble() > .5).ToList();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, false, random);
            var substances = MockSubstancesGenerator.Create(2);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, kineticConversionFactors);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var exposureTripleUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateExposures = FakeAggregateIndividualDayExposuresGenerator.Create(
                simulatedIndividualDays: individualDays,
                substances: substances,
                exposureRoutes: exposureRoutes,
                kineticModelCalculators: kineticModelCalculators,
                exposureTripleUnit,
                targetUnit,
                random: random);
            section.Summarize(aggregateExposures, substances, rpfs, memberships, kineticConversionFactors, targetUnit, exposureTripleUnit, substances.First(), true);
            var positives = aggregateExposures.Count(r => r.IsPositiveTargetExposure(targetUnit.Target));
            Assert.AreEqual(positives, section.IndividualCompoundIntakeRecords.Count);
            AssertIsValidView(section);
        }
    }
}