using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var kineticConversionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 100;

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
                    new List<TargetUnit>() { targetUnit },
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
                new List<TargetUnit>() { targetUnit },
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
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[]  { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var kineticConversionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 100;

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
                    new List<TargetUnit>() { targetUnit },
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
                new List<TargetUnit>() { targetUnit },
                ExposureType.Chronic,
                exposureUnit,
                double.NaN,
                double.NaN
            );

            AssertIsValidView(section);
        }
    }
}
