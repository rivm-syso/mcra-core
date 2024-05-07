using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Actions.KineticModels;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    /// <summary>
    /// SBML KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class SbmlPbkModelCalculatorTest {
        /// <summary>
        /// SBML ModelCalculator: calculates individual day target exposures, Euromix SBML, acute
        /// </summary>
        // TODO: enable test when Python and roadrunner are installed on the build/test server
        [TestMethod]
        [Ignore]
        public void SbmlModelCalculator_TestCalculateIndividualDayTargetExposuresEuromix() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary };
            var absorptionFactors = routes.ToDictionary(r => (r, substance), r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = CreateModelInstance("EuroMixGenericPbk_V1", substance);

            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.SpecifyEvents = true;
            instance.SelectedEvents = new[] {1,2,4,6,8,9,10};
            instance.CompartmentCodes = new List<string> { "Fat" };
            var model = new SbmlPbkModelCalculator(instance, absorptionFactors.Get(substance));

            var internalExposures = model.CalculateIndividualDayTargetExposures(
                individualDayExposures,
                routes,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                new List<TargetUnit> { TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL) },
                new ProgressState(),
                random
            );
            Assert.AreEqual(10, internalExposures.Count);
            var positiveExternalExposures = individualDayExposures.Where(r => r.ExposuresPerRouteSubstance.Any(eprc => eprc.Value.Any(ipc => ipc.Amount > 0)));
            Assert.AreEqual(positiveExternalExposures.Count(), internalExposures.First().IndividualDaySubstanceTargetExposures.SelectMany(r => r.SubstanceTargetExposures).Count(c => c.SubstanceAmount > 0));
            Assert.AreEqual(10 * 24 + 1, internalExposures.First().IndividualDaySubstanceTargetExposures.SelectMany(r => r.SubstanceTargetExposures).Select(c => (SubstanceTargetExposurePattern)c).First().TargetExposuresPerTimeUnit.Count);
        }

        private KineticModelInstance CreateModelInstance(string idModel, Compound substance) {
            var modelDefinition = MCRAKineticModelDefinitions.Definitions[idModel];
            modelDefinition = KineticModelsActionCalculator.LoadSbmlModelDefinition(modelDefinition);

            var instance = new KineticModelInstance() {
                KineticModelDefinition = modelDefinition,
                KineticModelSubstances = new List<KineticModelSubstance>() {
                     new KineticModelSubstance() {
                         Substance = substance
                     }
                },
                KineticModelInstanceParameters = new Dictionary<string, KineticModelInstanceParameter>() {
                    {
                        "BM",
                        new KineticModelInstanceParameter() {
                            Parameter = "BM",
                            Value = 80,
                        }
                    }
                },
                IdTestSystem = "Human",
                IdModelDefinition = "EuroMixGenericPbk"
            };
            return instance;
        }
    }
}
