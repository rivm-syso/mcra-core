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

        protected virtual SbmlPbkModelSpecification GetModelDefinition() {
            return SbmlPbkModelSpecificationBuilder.CreateFromSbmlFile(
                "Resources/PbkModels/simple_lifetime.sbml"
            );
        }

        protected KineticModelInstance getDefaultInstance(params Compound[] substance) {
            var instance = createFakeModelInstance(substance.Single());
            return instance;
        }

        protected PbkSimulationSettings getDefaultSimulationSettings() {
            return new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = true,
            };
        }

        protected SbmlPbkModelCalculator createCalculator(
            KineticModelInstance instance,
            PbkSimulationSettings simulationSettings
        ) {
            return new SbmlPbkModelCalculator(instance, simulationSettings);
        }

        [TestMethod]
        public void SbmlLifetimePbkModelCalculation_TestDefinition() {
            var modelDefinition = GetModelDefinition();
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
            var externalExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var paths = FakeExposurePathGenerator.Create(routes);
            var individual = FakeIndividualsGenerator.CreateSingle(
                bodyWeight: 80,
                age: 30
            );
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays([individual]);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            var instance = createFakeModelInstance(substances.First());
            var simulationSettings = new PbkSimulationSettings() {
                PbkSimulationMethod = PbkSimulationMethod.LifetimeToCurrentAge,
                LifetimeYears = 10,
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

        private KineticModelInstance createFakeModelInstance(
            Compound substance
        ) {
            var modelDefinition = GetModelDefinition();
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
