using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var routes = new[] { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var kineticConversionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 1;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);

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
            var routes = new[]  { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var kineticConversionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = FakeExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 1;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

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
