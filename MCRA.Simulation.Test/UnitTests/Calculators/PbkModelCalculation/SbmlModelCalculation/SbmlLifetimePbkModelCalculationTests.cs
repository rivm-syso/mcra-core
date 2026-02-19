using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PbkModelCalculation.SbmlModelCalculation {

    /// <summary>
    /// SBML KineticModelCalculation calculator tests.
    /// </summary>
    [TestClass]
    public class SbmlLifetimePbkModelCalculationTests {

        private SbmlPbkModelSpecification GetModelDefinition(string file) {
            return SbmlPbkModelSpecificationBuilder.CreateFromSbmlFile(
                file
            );
        }

        [TestMethod]
        public void SbmlLifetimePbkModelCalculation_TestDefinition() {
            var file = "Resources/PbkModels/simple_lifetime.sbml";
            var modelDefinition = GetModelDefinition(file);
            var expected = new[] { "AGut" };
            CollectionAssert.AreEquivalent(
                expected,
                modelDefinition.GetRouteInputSpecies().Values.Select(r => r.Id).ToArray()
            );

            var bwParam = modelDefinition.GetParameterDefinitionByType(PbkModelParameterType.BodyWeight);
            Assert.AreEqual("BW", bwParam.Id);
            Assert.IsTrue(bwParam.IsInternalParameter);

            var ageParam = modelDefinition.GetParameterDefinitionByType(PbkModelParameterType.Age);
            Assert.AreEqual("Age", ageParam.Id);
            Assert.IsTrue(ageParam.IsInternalParameter);

            var ageInitParam = modelDefinition.GetParameterDefinitionByType(PbkModelParameterType.AgeInit);
            Assert.AreEqual("AgeInit", ageInitParam.Id);
            Assert.IsFalse(ageInitParam.IsInternalParameter);

            var bwRefParam = modelDefinition.GetParameterDefinitionByType(PbkModelParameterType.BodyWeightRef);
            Assert.AreEqual("BWRef", bwRefParam.Id);
            Assert.IsFalse(bwRefParam.IsInternalParameter);

            var ageRefParam = modelDefinition.GetParameterDefinitionByType(PbkModelParameterType.AgeRef);
            Assert.AreEqual("AgeRef", ageRefParam.Id);
            Assert.IsFalse(ageRefParam.IsInternalParameter);
        }

        /// <summary>
        /// SBML ModelCalculator: calculates individual day target exposures, Euromix SBML, acute
        /// </summary>
        [TestMethod]
        public void SbmlLifetimePbkModelCalculation_TestLifeTimeSimulation() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var routes = new[] { ExposureRoute.Oral };
            var externalExposureUnit = ExposureUnitTriple.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var paths = FakeExposurePathGenerator.Create(routes);
            var individual = FakeIndividualsGenerator.CreateSingle(
                bodyWeight: 80,
                age: 30
            );
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays([individual]);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            var file = "Resources/PbkModels/simple_lifetime.sbml";
            var instance = createFakeModelInstance(file, substances.First());
            var simulationSettings = new PbkSimulationSettings() {
                PbkSimulationMethod = PbkSimulationMethod.LifetimeToCurrentAge,
                LifetimeEndAge = 10,
                OutputResolutionStepSize = 1,
                OutputResolutionTimeUnit = PbkModelOutputResolutionTimeUnit.Days
            };
            var modelCalculator = new SbmlPbkModelCalculator(instance, simulationSettings);

            var simulationResults = modelCalculator
                .Calculate(
                    individualDayExposures
                        .GroupBy(r => r.SimulatedIndividual)
                        .Select(r => (r.Key, r.ToList()))
                        .ToList(),
                    externalExposureUnit,
                    routes,
                    [targetUnit],
                    random,
                    new ProgressState()
                )
                .Single();

            // Check time series of age and BW
            var nrOfDays = (int)(30 * 365.25);
            Assert.AreEqual(0, simulationResults.AgeStart);
            Assert.AreEqual(individual.Age.Value, simulationResults.AgeEnd, 1D / 365); // One day tolerance
            Assert.AreEqual(individual.BodyWeight, simulationResults.BodyWeightTimeSeries.Last(),
                individual.BodyWeight / nrOfDays);
            Assert.IsGreaterThan(
                simulationResults.BodyWeightTimeSeries.First(),
                simulationResults.BodyWeightTimeSeries.Last()
            );
        }


        [TestMethod]
        public void SbmlLifetimePbkModelCalculation_TestLifetimeWindowToCurrent() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var routes = new[] { ExposureRoute.Oral };
            var externalExposureUnit = ExposureUnitTriple.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var paths = FakeExposurePathGenerator.Create(routes);
            var individual = FakeIndividualsGenerator.CreateSingle(bodyWeight: 80, age: 30);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays([individual]);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            var file = "Resources/PbkModels/simple_lifetime.sbml";
            var instance = createFakeModelInstance(file, substances.First());
            var simulationSettings = new PbkSimulationSettings() {
                PbkSimulationMethod = PbkSimulationMethod.LifetimeWindowToCurrent,
                LifetimeWindowYears = 10,
                OutputResolutionStepSize = 1,
                OutputResolutionTimeUnit = PbkModelOutputResolutionTimeUnit.Days
            };
            var modelCalculator = new SbmlPbkModelCalculator(instance, simulationSettings);

            var simulationResults = modelCalculator
                .Calculate(
                    individualDayExposures
                        .GroupBy(r => r.SimulatedIndividual)
                        .Select(r => (r.Key, r.ToList()))
                        .ToList(),
                    externalExposureUnit,
                    routes,
                    [targetUnit],
                    random,
                    new ProgressState()
                )
                .Single();

            // Window: starts at age 20 (max(0, 30-10)), ends at 30
            var expectedStart = individual.Age.Value - simulationSettings.LifetimeWindowYears;
            var nrOfDays = (int)(simulationSettings.LifetimeWindowYears * 365.25);
            Assert.AreEqual(expectedStart, simulationResults.AgeStart, 1D / 365);
            Assert.AreEqual(individual.Age.Value, simulationResults.AgeEnd, 1D / 365);
            Assert.AreEqual(individual.BodyWeight, simulationResults.BodyWeightTimeSeries.Last(),
                individual.BodyWeight / nrOfDays);
        }

        [TestMethod]
        public void SbmlLifetimePbkModelCalculation_TestLifetimeRange() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var routes = new[] { ExposureRoute.Oral };
            var externalExposureUnit = ExposureUnitTriple.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var paths = FakeExposurePathGenerator.Create(routes);
            var individual = FakeIndividualsGenerator.CreateSingle(bodyWeight: 80, age: 30);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays([individual]);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            var file = "Resources/PbkModels/simple_lifetime.sbml";
            var instance = createFakeModelInstance(file, substances.First());
            var simulationSettings = new PbkSimulationSettings() {
                PbkSimulationMethod = PbkSimulationMethod.LifetimeRange,
                LifetimeStartAge = 10,
                LifetimeEndAge = 40,
                OutputResolutionStepSize = 1,
                OutputResolutionTimeUnit = PbkModelOutputResolutionTimeUnit.Days
            };
            var modelCalculator = new SbmlPbkModelCalculator(instance, simulationSettings);

            var simulationResults = modelCalculator
                .Calculate(
                    individualDayExposures
                        .GroupBy(r => r.SimulatedIndividual)
                        .Select(r => (r.Key, r.ToList()))
                        .ToList(),
                    externalExposureUnit,
                    routes,
                    [targetUnit],
                    random,
                    new ProgressState()
                )
                .Single();

            var durationYears = simulationSettings.LifetimeEndAge - simulationSettings.LifetimeStartAge;
            var nrOfDays = (int)(durationYears * 365.25);
            Assert.AreEqual(simulationSettings.LifetimeStartAge, simulationResults.AgeStart, 1D / 365);
            Assert.AreEqual(simulationSettings.LifetimeEndAge, simulationResults.AgeEnd, 1D / 365);
        }

        [TestMethod]
        public void SbmlLifetimePbkModelCalculation_TestLifetimeCurrentToSpecifiedAge() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var routes = new[] { ExposureRoute.Oral };
            var externalExposureUnit = ExposureUnitTriple.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var paths = FakeExposurePathGenerator.Create(routes);
            var individual = FakeIndividualsGenerator.CreateSingle(bodyWeight: 80, age: 30);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays([individual]);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            var file = "Resources/PbkModels/simple_lifetime.sbml";
            var instance = createFakeModelInstance(file, substances.First());
            var simulationSettings = new PbkSimulationSettings() {
                PbkSimulationMethod = PbkSimulationMethod.LifetimeCurrentToSpecifiedAge,
                LifetimeEndAge = 50,
                OutputResolutionStepSize = 1,
                OutputResolutionTimeUnit = PbkModelOutputResolutionTimeUnit.Days
            };
            var modelCalculator = new SbmlPbkModelCalculator(instance, simulationSettings);

            var simulationResults = modelCalculator
                .Calculate(
                    individualDayExposures
                        .GroupBy(r => r.SimulatedIndividual)
                        .Select(r => (r.Key, r.ToList()))
                        .ToList(),
                    externalExposureUnit,
                    routes,
                    [targetUnit],
                    random,
                    new ProgressState()
                )
                .Single();

            Assert.AreEqual(individual.Age.Value, simulationResults.AgeStart, 1D / 365);
            Assert.AreEqual(simulationSettings.LifetimeEndAge, simulationResults.AgeEnd, 1D / 365);
            Assert.AreEqual(individual.BodyWeight, simulationResults.BodyWeightTimeSeries.First(),
                individual.BodyWeight / 365);
        }

        [TestMethod]
        public void SbmlLifetimePbkModelCalculation_TestLifetimeCurrentAgeForward() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var routes = new[] { ExposureRoute.Oral };
            var externalExposureUnit = ExposureUnitTriple.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var paths = FakeExposurePathGenerator.Create(routes);
            var individual = FakeIndividualsGenerator.CreateSingle(bodyWeight: 80, age: 30);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays([individual]);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            var file = "Resources/PbkModels/simple_lifetime.sbml";
            var instance = createFakeModelInstance(file, substances.First());
            var simulationSettings = new PbkSimulationSettings() {
                PbkSimulationMethod = PbkSimulationMethod.LifetimeCurrentAgeForward,
                LifetimeWindowYears = 15,
                OutputResolutionStepSize = 1,
                OutputResolutionTimeUnit = PbkModelOutputResolutionTimeUnit.Days
            };
            var modelCalculator = new SbmlPbkModelCalculator(instance, simulationSettings);

            var simulationResults = modelCalculator
                .Calculate(
                    individualDayExposures
                        .GroupBy(r => r.SimulatedIndividual)
                        .Select(r => (r.Key, r.ToList()))
                        .ToList(),
                    externalExposureUnit,
                    routes,
                    [targetUnit],
                    random,
                    new ProgressState()
                )
                .Single();

            Assert.AreEqual(individual.Age.Value, simulationResults.AgeStart, 1D / 365);
            Assert.AreEqual(individual.Age.Value + simulationSettings.LifetimeWindowYears, simulationResults.AgeEnd, 1D / 365);
            // BW at start (age = age_init = current age) should match individual's BW
            Assert.AreEqual(individual.BodyWeight, simulationResults.BodyWeightTimeSeries.First(),
                individual.BodyWeight / 365);
        }

        private KineticModelInstance createFakeModelInstance(
            string file,
            Compound substance
        ) {
            var modelDefinition = GetModelDefinition(file);
            var instance = new KineticModelInstance() {
                KineticModelDefinition = modelDefinition,
                ModelSubstances = [
                     new PbkModelSubstance() {
                         Substance = substance
                     }
                ],
                KineticModelInstanceParameters = new Dictionary<string, KineticModelInstanceParameter>(),
                IdTestSystem = "Human",
                IdModelDefinition = modelDefinition.Id,
            };
            return instance;
        }
    }
}
