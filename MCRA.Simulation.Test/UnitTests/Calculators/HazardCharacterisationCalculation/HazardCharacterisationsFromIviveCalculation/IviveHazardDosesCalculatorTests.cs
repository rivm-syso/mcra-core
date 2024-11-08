using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Simulation.Test.Mock.MockCalculators;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var random = new McraRandomGenerator(seed: 1);
            var effects = FakeEffectsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(3);
            var exposureType = ExposureType.Chronic;

            var intraSpeciesFactor = 10D;
            var inVivoReferenceDoseUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var referenceRecord = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(
                    effects.First(),
                    substances.First(),
                    100,
                    inVivoReferenceDoseUnit,
                    intraSpeciesFactor: intraSpeciesFactor
                );

            var responses = FakeResponsesGenerator.Create(1);
            var effectRepresentations = FakeEffectRepresentationsGenerator
                .Create(effects, responses);
            var doses = new double[] { 1, 10, 100 };
            var doseResponseModels = responses
                .Select(r => FakeDoseResponseModelGenerator.Create(r, substances, random, doses))
                .ToList();
            var species = doseResponseModels
                .Select(r => r.Response?.TestSystem?.Species)
                .ToList();
            var interSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator
                .Create(substances, species, effects.First(), 1);
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(substances);
            var kineticConversionFactor = 0.8;
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator(kineticConversionFactor);

            var calculator = new HazardCharacterisationsFromIviveCalculator();
            var result = calculator.Compute(
                effects.First(),
                doseResponseModels,
                substances,
                referenceRecord,
                effectRepresentations,
                inVivoReferenceDoseUnit,
                exposureType,
                interSpeciesFactorModels,
                kineticConversionFactorCalculator,
                intraSpeciesFactorModels,
                1,
                random
            );
            Assert.AreEqual(3, result.Count);

            var expected = new[] { 100D, 1000D, 10000D };
            for (int i = 0; i < 3; i++) {
                Assert.AreEqual(expected[i], result[i].Value, 1e-5);
            }
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
                target: new ExposureTarget(ExposureRoute.Oral),
                referenceHazardDose: 10,
                benchmarkDoses: [1D, 10D, 100D],
                speciesTestSystem: "Rat",
                kineticConversionFactors: [1D, 1D, 1D],
                interSpeciesFactors: [10D, 10D, 10D],
                intraSpeciesFactors: [10D, 10D, 10D],
                expected: [10D, 100D, 1000D]
            );

            // Change inter-species to { 5, 10, 10 }
            runScenario(
                target: new ExposureTarget(ExposureRoute.Oral),
                referenceHazardDose: 10,
                benchmarkDoses: [1D, 10D, 100D],
                speciesTestSystem: "Rat",
                kineticConversionFactors: [1D, 1D, 1D],
                interSpeciesFactors: [5D, 10D, 10D],
                intraSpeciesFactors: [10D, 10D, 10D],
                expected: [10D, 50D, 500D]
            );

            // Change intra-species to { 5, 10, 10 }
            runScenario(
                target: new ExposureTarget(ExposureRoute.Oral),
                referenceHazardDose: 10,
                benchmarkDoses: [1D, 10D, 100D],
                speciesTestSystem: "Rat",
                kineticConversionFactors: [1D, 1D, 1D],
                interSpeciesFactors: [10D, 10D, 10D],
                intraSpeciesFactors: [5D, 10D, 10D],
                expected: [10D, 50D, 500D]
            );

            // Change absorption factor to { 1, .5, .25 }
            runScenario(
                target: new ExposureTarget(ExposureRoute.Oral),
                referenceHazardDose: 10,
                benchmarkDoses: [1D, 10D, 100D],
                speciesTestSystem: "Rat",
                kineticConversionFactors: [1D, .5, .25],
                interSpeciesFactors: [10D, 10D, 10D],
                intraSpeciesFactors: [10D, 10D, 10D],
                expected: [10D, 200D, 4000D]
            );

            // Change inter-species factors to { 5, 10, 10 }, but set
            // test system to Human (i.e., no inter-species conversion)
            runScenario(
                target: new ExposureTarget(ExposureRoute.Oral),
                referenceHazardDose: 10,
                benchmarkDoses: [1D, 10D, 100D],
                speciesTestSystem: "Human",
                kineticConversionFactors: [1D, 1D, 1D],
                interSpeciesFactors: [5D, 10D, 10D],
                intraSpeciesFactors: [10D, 10D, 10D],
                expected: [10D, 100D, 1000D]
            );

            // Change to internal target dose level
            runScenario(
                target: new ExposureTarget(BiologicalMatrix.Blood),
                referenceHazardDose: 10,
                benchmarkDoses: [1D, 10D, 100D],
                speciesTestSystem: "Rat",
                kineticConversionFactors: [1D, 1D, 1D],
                interSpeciesFactors: [10D, 10D, 10D],
                intraSpeciesFactors: [10D, 10D, 10D],
                expected: [10D, 100D, 1000D]
            );

            // Change to internal target dose level and absorption
            // factors { .5, .25, .125 }
            runScenario(
                target: new ExposureTarget(BiologicalMatrix.Blood),
                referenceHazardDose: 10,
                benchmarkDoses: [1D, 10D, 100D],
                speciesTestSystem: "Rat",
                kineticConversionFactors: [.5, .25, .125],
                interSpeciesFactors: [10D, 10D, 10D],
                intraSpeciesFactors: [10D, 10D, 10D],
                expected: [10D, 100D, 1000D]
            );
        }

        /// <summary>
        /// Runs an IVIVE test scenario. First substance is assumed to be
        /// the reference.
        /// </summary>
        private void runScenario(
            ExposureTarget target,
            double referenceHazardDose,
            double[] benchmarkDoses,
            string speciesTestSystem,
            double[] kineticConversionFactors,
            double[] interSpeciesFactors,
            double[] intraSpeciesFactors,
            double[] expected,
            int seed = 1
        ) {
            var numSubstances = benchmarkDoses.Length;
            var exposureType = ExposureType.Chronic;
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = new TargetUnit(target, exposureUnit);
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(numSubstances);
            var species = new List<string>() { speciesTestSystem };
            var responses = FakeResponsesGenerator.Create(1, species.ToArray());
            var effectRepresentations = FakeEffectRepresentationsGenerator.Create(effects, responses);
            var doseResponseModels = responses
                .Select(r => FakeDoseResponseModelGenerator.Create(r, substances, random, benchmarkDoses))
                .ToList();
            var kineticConversionFactorCalculator = new MockKineticConversionFactorCalculator(
                double.NaN,
                substances.Select((r, ix) => (Substance: r, Factor: kineticConversionFactors[ix])).ToDictionary(r => r.Substance, r => r.Factor)
            );
            var referenceRecord = FakeHazardCharacterisationModelsGenerator.CreateSingle(
                effects.First(),
                substances.First(),
                referenceHazardDose,
                targetUnit,
                interSpeciesFactors[0],
                intraSpeciesFactors[0],
                1D
            );
            var interSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator.Create(
                substances,
                species,
                effects.First(),
                double.NaN,
                interSpeciesFactors,
                null
            );
            var intraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(
                substances,
                effects.First(),
                intraSpeciesFactors
            );

            var calculator = new HazardCharacterisationsFromIviveCalculator();
            var result = calculator.Compute(
                effects.First(),
                doseResponseModels,
                substances,
                referenceRecord,
                effectRepresentations,
                targetUnit,
                exposureType,
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
