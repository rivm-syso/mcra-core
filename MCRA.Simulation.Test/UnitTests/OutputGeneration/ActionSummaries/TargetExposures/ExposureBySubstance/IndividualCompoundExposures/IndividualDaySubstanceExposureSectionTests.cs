using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    [TestClass]
    public class IndividualDaySubstanceExposureSectionTests : SectionTestBase {

        [TestMethod]
        public void IndividualDaySubstanceExposureSection_TestValidView() {
            var section = new IndividualDaySubstanceExposureSection() {
                Records = [
                    new IndividualDaySubstanceExposureRecord() {
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
        public void IndividualDaySubstanceExposureSection_TestSummarizeBySubstance() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, false, random);
            var substances = FakeSubstancesGenerator.Create(1);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var kineticModelCalculators = FakeKineticModelsGenerator
                .CreateAbsorptionFactorKineticModelCalculators(substances, kineticConversionFactors, targetUnit);
            var kineticConversionCalculator = new KineticConversionFactorsCalculator(kineticModelCalculators);
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculator);
            var exposureTripleUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateExposures = FakeAggregateIndividualDayExposuresGenerator.Create(
                simulatedIndividualDays: individualDays,
                substances: substances,
                routes: routes,
                kineticModelCalculators: kineticModelCalculators,
                exposureTripleUnit,
                targetUnit,
                random: random
            );

            var section = new IndividualDaySubstanceExposureSection();
            section.Summarize(aggregateExposures, substances, rpfs, memberships, kineticConversionFactors, targetUnit, exposureTripleUnit);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test IndividualDayCompoundIntakeSection summarize by substance
        /// </summary>
        [TestMethod]
        public void IndividualDaySubstanceExposureSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral, ExposureRoute.Inhalation };
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(10, 2, false, random);
            var substances = FakeSubstancesGenerator.Create(2);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticConversionFactors = FakeKineticModelsGenerator
                .CreateAbsorptionFactors(substances, routes, 1);
            var kineticModelCalculators = FakeKineticModelsGenerator
                .CreateAbsorptionFactorKineticModelCalculators(substances, kineticConversionFactors, targetUnit);
            var kineticConversionCalculator = new KineticConversionFactorsCalculator(kineticModelCalculators);
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculator);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var aggregateExposures = FakeAggregateIndividualDayExposuresGenerator
                .Create(
                    simulatedIndividualDays: individualDays,
                    substances: substances,
                    routes: routes,
                    kineticModelCalculators: kineticModelCalculators,
                    externalExposuresUnit: externalExposuresUnit,
                    targetUnit,
                    random: random
                );

            var section = new IndividualDaySubstanceExposureSection();
            section.Summarize(
                aggregateExposures,
                substances,
                rpfs,
                memberships,
                kineticConversionFactors,
                targetUnit,
                externalExposuresUnit,
                substances.First(),
                true
            );
            AssertIsValidView(section);
        }
    }
}