using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the DietaryExposuresOutputWriter
    /// </summary>
    [TestClass]
    public class DietaryExposuresOutputWriterTests {

        protected static string _reportOutputPath = Path.Combine(TestUtilities.TestOutputPath, "DietaryExposuresOutputWriterTests");

        /// <summary>
        /// Runs the DietaryExposures action: OutputWriter Acute
        /// </summary>
        [TestMethod]
        public void DietaryExposuresOutputWriter_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var project = new ProjectDto();
            var config = project.DietaryExposuresSettings;
            config.ExposureType = ExposureType.Acute;
            config.Cumulative = true;

            var data = new ActionData();
            data.ActiveSubstances = FakeSubstancesGenerator.Create(5);
            data.ModelledFoodConsumers = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            data.ReferenceSubstance = data.ActiveSubstances.First();
            data.CorrectedRelativePotencyFactors = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.MembershipProbabilities = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.CumulativeCompound = data.ActiveSubstances.First();
            data.DietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay);

            var foods = FakeFoodsGenerator.Create(8);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(data.ModelledFoodConsumers);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(simulatedIndividualDays, foods, data.ActiveSubstances, 0.5, true, random);
            var result = new DietaryExposuresActionResult() {
                DietaryIndividualDayIntakes = exposures
            };
            var outputRawDataWriter = new CsvRawDataWriter(Path.Combine(_reportOutputPath, "TestAcute"));
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(config, data, result, outputRawDataWriter);
            outputRawDataWriter.Store();
        }

        /// <summary>
        /// Runs the DietaryExposures action: OutputWriter Chronic BBN or LNN0
        /// </summary>
        [TestMethod]
        public void DietaryExposuresOutputWriter_TestChronic1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var project = new ProjectDto();
            var config = project.DietaryExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.Cumulative = true;

            var data = new ActionData();
            data.ActiveSubstances = FakeSubstancesGenerator.Create(5);
            data.ModelledFoodConsumers = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            data.ReferenceSubstance = data.ActiveSubstances.First();
            data.CorrectedRelativePotencyFactors = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.MembershipProbabilities = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.CumulativeCompound = data.ActiveSubstances.First();
            data.DietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay);

            var foods = FakeFoodsGenerator.Create(8);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(data.ModelledFoodConsumers);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(simulatedIndividualDays, foods, data.ActiveSubstances, 0.5, true, random);
            var usualIntakes = exposures
                .Select(c => new DietaryIndividualIntake() {
                    DietaryIntakePerMassUnit= c.TotalExposurePerMassUnit(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, false),
                    SimulatedIndividual = c.SimulatedIndividual,
                    NumberOfDays = 2
                })
                .ToList();

            var result = new DietaryExposuresActionResult() {
                DietaryIndividualDayIntakes = exposures,
                DietaryModelAssistedIntakes = usualIntakes,
            };
            var outputRawDataWriter = new CsvRawDataWriter(Path.Combine(_reportOutputPath, "TestChronic1"));
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(config, data, result, outputRawDataWriter);
            outputRawDataWriter.Store();
        }

        /// <summary>
        /// Runs the DietaryExposures action: OutputWriter Chronic OIM
        /// </summary>
        [TestMethod]
        public void DietaryExposuresOutputWriter_TestChronic2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var project = new ProjectDto();
            var config = project.DietaryExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.Cumulative = true;

            var data = new ActionData();
            data.ActiveSubstances = FakeSubstancesGenerator.Create(5);
            data.ModelledFoodConsumers = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            data.ReferenceSubstance = data.ActiveSubstances.First();
            data.CorrectedRelativePotencyFactors = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.MembershipProbabilities = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.CumulativeCompound = data.ActiveSubstances.First();
            data.DietaryExposureUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay);

            var foods = FakeFoodsGenerator.Create(8);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(data.ModelledFoodConsumers);
            var exposures = FakeDietaryIndividualDayIntakeGenerator.Create(simulatedIndividualDays, foods, data.ActiveSubstances, 0.5, true, random);
            var usualIntakes = exposures
                .Select(c => new DietaryIndividualIntake() {
                    DietaryIntakePerMassUnit = c.TotalExposurePerMassUnit(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, false),
                    SimulatedIndividual = c.SimulatedIndividual,
                    NumberOfDays = 2
                })
                .ToList();

            var result = new DietaryExposuresActionResult() {
                DietaryIndividualDayIntakes = exposures,
                DietaryObservedIndividualMeans = usualIntakes,
            };
            var outputRawDataWriter = new CsvRawDataWriter(Path.Combine(_reportOutputPath, "TestChronic2"));
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(config, data, result, outputRawDataWriter);
            outputRawDataWriter.Store();
        }
    }
}