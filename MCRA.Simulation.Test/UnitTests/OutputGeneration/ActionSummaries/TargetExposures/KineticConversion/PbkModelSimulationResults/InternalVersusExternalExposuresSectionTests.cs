using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, KineticModel
    /// </summary>
    [TestClass]
    public class InternalVersusExternalExposuresSectionTests : SectionTestBase {

        /// <summary>
        /// Create chart and test KineticModelSection view, chronic, absorptionfactors
        /// </summary>
        [TestMethod]
        public void InternalVersusExternalExposuresSection_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var paths = FakeExposurePathGenerator.Create(routes);
            var kineticConversionFactors = routes.ToDictionary(r => r, r => .1);
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);

            // TODO: refactor to not use pbkmodel directly here
            var modelInstance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = 1
            };
            var pbkModelCalculator = new CosmosKineticModelCalculator(modelInstance, simulationSettings);
            var pbkKineticConversionCalculator = new PbkKineticConversionCalculator(pbkModelCalculator);
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(
                new Dictionary<Compound, IKineticConversionCalculator>() {
                    { substance, pbkKineticConversionCalculator }
                }
            );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);

            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeChronic(
                    individualExposures,
                    substances,
                    routes,
                    exposureUnit,
                    [targetUnit],
                    random,
                    new ProgressState()
                );

            var absorptionFactorsPerCompound = kineticConversionFactors.ToDictionary(c => (c.Key, substance), c => c.Value);
            var section = new InternalVersusExternalExposuresSection();
            section.Summarize(
                substance,
                routes,
                targetIndividualExposures,
                absorptionFactorsPerCompound,
                [targetUnit],
                ExposureType.Chronic,
                exposureUnit,
                double.NaN,
                double.NaN
            );
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelSection view, acute, kineticConversionFactors
        /// </summary>
        [TestMethod]
        public void InternalVersusExternalExposuresSection_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var paths = FakeExposurePathGenerator.Create(routes);
            var kineticConversionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = FakeExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            // TODO: refactor to not use pbkmodel directly here
            var modelInstance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = 1
            };
            var pbkModelCalculator = new CosmosKineticModelCalculator(modelInstance, simulationSettings);
            var pbkKineticConversionCalculator = new PbkKineticConversionCalculator(pbkModelCalculator);
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(
                new Dictionary<Compound, IKineticConversionCalculator>() {
                    { substance, pbkKineticConversionCalculator }
                }
            );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);

            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeAcute(
                    individualDayExposures,
                    substances,
                    routes,
                    exposureUnit,
                    [targetUnit],
                    random,
                    new ProgressState()
                );

            var absorptionFactorsPerCompound = kineticConversionFactors
                .ToDictionary(c => (c.Key, substance), c => c.Value);

            var section = new InternalVersusExternalExposuresSection();
            section.Summarize(
                substance,
                routes,
                targetIndividualExposures.Cast<AggregateIndividualExposure>().ToList(),
                absorptionFactorsPerCompound,
                [targetUnit],
                ExposureType.Chronic,
                exposureUnit,
                double.NaN,
                double.NaN
            );

            AssertIsValidView(section);
        }
    }
}
