using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Test.Mock.MockCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation {

    /// <summary>
    /// Tests for calculation of hazard characterisations using IVIVE.
    /// </summary>
    [TestClass]
    public class IviveHazardDosesCalculatorTests {

        /// <summary>
        /// Simple IVIVE test for three substances.
        /// Given: 
        /// - Three substances { A, B, C }.
        /// - Reference hazard characterisation for substance A with hazard
        ///   characterisation value 10.
        /// - Dose response model for substances { A, B, C } with BMDs
        ///   { 1, 10, 100 } (hence, RPFs { 1, .1, .100 }).
        /// Assert:
        /// - Hazard characterisation values should be { 10, 100, 1000 }.
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsFromIviveCalculator_TestSimple() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = MockEffectsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(3);
            var responses = MockResponsesGenerator.Create(1);
            var exposureType = ExposureType.Chronic;
            var doses = new double[] { 1, 10, 100 };
            var effectRepresentations = MockEffectRepresentationsGenerator.Create(effects, responses);
            var targetDoseUnit = new TargetUnit(
                ExposureTarget.DietaryExposureTarget,
                SubstanceAmountUnit.Micrograms,
                ConcentrationMassUnit.Kilograms,
                TimeScaleUnit.SteadyState
            );
            var doseResponseModels = responses
                .Select(r => MockDoseResponseModelGenerator.Create(r, substances, random, doses))
                .ToList();
            var absorptionFactor = 0.8;
            var intraSpeciesFactor = 10D;
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator(
                TargetLevelType.External,
                absorptionFactor
            );
            var referenceRecord = MockHazardCharacterisationModelsGenerator.CreateSingle(
                effects.First(),
                substances.First(),
                100,
                targetDoseUnit.Target,
                targetDoseUnit.ExposureUnit,
                intraSpeciesFactor: intraSpeciesFactor
            );
            var species = doseResponseModels.Select(r => r.Response?.TestSystem?.Species).ToList();
            var interSpeciesFactorModels = MockInterSpeciesFactorModelsGenerator.Create(
                substances,
                species,
                effects.First(),
                1
            );
            var intraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(substances);

            var hazardDoseConverter = new HazardDoseConverter(PointOfDepartureType.Noael, targetDoseUnit.ExposureUnit);
            var calculator = new HazardCharacterisationsFromIviveCalculator();
            var result = calculator.Compute(
                effects.First(),
                doseResponseModels,
                substances,
                referenceRecord,
                effectRepresentations,
                targetDoseUnit,
                exposureType,
                hazardDoseConverter,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels,
                1,
                random
            );
            Assert.AreEqual(3, result.Count);
            var targetDoses = result.Select(r => r.Value).ToArray();
            CollectionAssert.AreEqual(new[] { 100D, 1000D, 10000D }, targetDoses);
        }

        /// <summary>
        /// Compute IVIVE hazard characterisations for various scenarios, varying
        /// inter-species factors, intra-species factors, kinetic conversion factors,
        /// target level (internal/external), etc.
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsFromIviveCalculator_TestIviveScenarios() {

            // Base scenario
            runScenario(
                targetDoseLevel: TargetLevelType.External,
                referenceHazardDose: 10,
                benchmarkDoses: new[] { 1D, 10D, 100D },
                speciesTestSystem: "Rat",
                absorptionFactors: new[] { 1D, 1D, 1D },
                interSpeciesFactors: new[] { 10D, 10D, 10D },
                intraSpeciesFactors: new[] { 10D, 10D, 10D },
                expected: new[] { 10D, 100D, 1000D }
            );

            // Change inter-species to { 5, 10, 10 }
            runScenario(
                targetDoseLevel: TargetLevelType.External,
                referenceHazardDose: 10,
                benchmarkDoses: new[] { 1D, 10D, 100D },
                speciesTestSystem: "Rat",
                absorptionFactors: new[] { 1D, 1D, 1D },
                interSpeciesFactors: new[] { 5D, 10D, 10D },
                intraSpeciesFactors: new[] { 10D, 10D, 10D },
                expected: new[] { 10D, 50D, 500D }
            );

            // Change intra-species to { 5, 10, 10 }
            runScenario(
                targetDoseLevel: TargetLevelType.External,
                referenceHazardDose: 10,
                benchmarkDoses: new[] { 1D, 10D, 100D },
                speciesTestSystem: "Rat",
                absorptionFactors: new[] { 1D, 1D, 1D },
                interSpeciesFactors: new[] { 10D, 10D, 10D },
                intraSpeciesFactors: new[] { 5D, 10D, 10D },
                expected: new[] { 10D, 50D, 500D }
            );

            // Change absorption factor to { 1, .5, .25 }
            runScenario(
                targetDoseLevel: TargetLevelType.External,
                referenceHazardDose: 10,
                benchmarkDoses: new[] { 1D, 10D, 100D },
                speciesTestSystem: "Rat",
                absorptionFactors: new[] { 1D, .5, .25 },
                interSpeciesFactors: new[] { 10D, 10D, 10D },
                intraSpeciesFactors: new[] { 10D, 10D, 10D },
                expected: new[] { 10D, 200D, 4000D }
            );

            // Change inter-species factors to { 5, 10, 10 }, but set 
            // test system to Human (i.e., no inter-species conversion)
            runScenario(
                targetDoseLevel: TargetLevelType.External,
                referenceHazardDose: 10,
                benchmarkDoses: new[] { 1D, 10D, 100D },
                speciesTestSystem: "Human",
                absorptionFactors: new[] { 1D, 1D, 1D },
                interSpeciesFactors: new[] { 5D, 10D, 10D },
                intraSpeciesFactors: new[] { 10D, 10D, 10D },
                expected: new[] { 10D, 100D, 1000D }
            );

            // Change to internal target dose level
            runScenario(
                targetDoseLevel: TargetLevelType.Internal,
                referenceHazardDose: 10,
                benchmarkDoses: new[] { 1D, 10D, 100D },
                speciesTestSystem: "Rat",
                absorptionFactors: new[] { 1D, 1D, 1D },
                interSpeciesFactors: new[] { 10D, 10D, 10D },
                intraSpeciesFactors: new[] { 10D, 10D, 10D },
                expected: new[] { 10D, 100D, 1000D }
            );

            // Change to internal target dose level and absorption
            // factors { .5, .25, .125 }
            runScenario(
                targetDoseLevel: TargetLevelType.Internal,
                referenceHazardDose: 10,
                benchmarkDoses: new[] { 1D, 10D, 100D },
                speciesTestSystem: "Rat",
                absorptionFactors: new[] { .5, .25, .125 },
                interSpeciesFactors: new[] { 10D, 10D, 10D },
                intraSpeciesFactors: new[] { 10D, 10D, 10D },
                expected: new[] { 10D, 100D, 1000D }
            );
        }

        /// <summary>
        /// Runs an IVIVE test scenario. First substance is assumed to be
        /// the reference.
        /// </summary>
        /// <param name="targetDoseLevel"></param>
        /// <param name="referenceHazardDose"></param>
        /// <param name="benchmarkDoses"></param>
        /// <param name="speciesTestSystem"></param>
        /// <param name="absorptionFactors"></param>
        /// <param name="interSpeciesFactors"></param>
        /// <param name="intraSpeciesFactors"></param>
        /// <param name="expected"></param>
        /// <param name="seed"></param>
        private void runScenario(
            TargetLevelType targetDoseLevel,
            double referenceHazardDose,
            double[] benchmarkDoses,
            string speciesTestSystem,
            double[] absorptionFactors,
            double[] interSpeciesFactors,
            double[] intraSpeciesFactors,
            double[] expected,
            int seed = 1
        ) {
            var numSubstances = benchmarkDoses.Length;
            var exposureType = ExposureType.Chronic;
            var targetUnit = targetDoseLevel == TargetLevelType.External
                ? TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay)
                : TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerKgBWPerDay);
            var random = new McraRandomGenerator(seed);
            var effects = MockEffectsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(numSubstances);
            var species = new List<string>() { speciesTestSystem };
            var responses = MockResponsesGenerator.Create(1, species.ToArray());
            var effectRepresentations = MockEffectRepresentationsGenerator.Create(effects, responses);
            var doseResponseModels = responses
                .Select(r => MockDoseResponseModelGenerator.Create(r, substances, random, benchmarkDoses))
                .ToList();
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator(
                targetDoseLevel,
                double.NaN,
                substances.Select((r, ix) => (Substance: r, Factor: absorptionFactors[ix])).ToDictionary(r => r.Substance, r => r.Factor)
            );
            var referenceRecord = MockHazardCharacterisationModelsGenerator.CreateSingle(
                effects.First(),
                substances.First(),
                referenceHazardDose,
                targetUnit.Target,
                targetUnit.ExposureUnit,
                interSpeciesFactors[0],
                intraSpeciesFactors[0],
                1D
            );
            var interSpeciesFactorModels = MockInterSpeciesFactorModelsGenerator.Create(
                substances,
                species,
                effects.First(),
                double.NaN,
                interSpeciesFactors,
                null
            );
            var intraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(
                substances,
                effects.First(),
                intraSpeciesFactors
            );

            var hazardDoseConverter = new HazardDoseConverter(PointOfDepartureType.Noael, targetUnit.ExposureUnit);
            var calculator = new HazardCharacterisationsFromIviveCalculator();
            var result = calculator.Compute(
                effects.First(),
                doseResponseModels,
                substances,
                referenceRecord,
                effectRepresentations,
                targetUnit,
                exposureType,
                hazardDoseConverter,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels,
                1,
                random
            );
            Assert.AreEqual(numSubstances, result.Count);
            var targetDoses = result.Select(r => r.Value).ToArray();
            for (int i = 0; i < numSubstances; i++) {
                Assert.AreEqual(expected[i], targetDoses[i], 1e-10);
            }
        }
    }
}
