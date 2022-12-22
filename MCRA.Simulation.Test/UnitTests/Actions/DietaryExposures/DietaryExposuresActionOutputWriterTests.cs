using System.IO;
using System.Linq;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Cumulative = true;
            var data = new ActionData();
            var foods = MockFoodsGenerator.Create(8);
            data.ActiveSubstances = MockSubstancesGenerator.Create(5);
            data.ModelledFoodConsumers = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(data.ModelledFoodConsumers);
            data.ReferenceCompound = data.ActiveSubstances.First();
            data.CorrectedRelativePotencyFactors = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.MembershipProbabilities = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.CumulativeCompound = data.ActiveSubstances.First();
            data.DietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(data.ConsumptionUnit, data.ConcentrationUnit, data.BodyWeightUnit, false);
            data.DietaryExposureUnit.Compartment = "bw";

            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(simulatedIndividualDays, foods, data.ActiveSubstances, 0.5, true, random);
            var result = new DietaryExposuresActionResult() {
                DietaryIndividualDayIntakes = exposures
            };
            var outputRawDataWriter = new CsvRawDataWriter(Path.Combine(_reportOutputPath, "TestAcute"));
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(project, data, result, outputRawDataWriter);
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
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.AssessmentSettings.Cumulative = true;
            var data = new ActionData();
            var foods = MockFoodsGenerator.Create(8);
            data.ActiveSubstances = MockSubstancesGenerator.Create(5);
            data.ModelledFoodConsumers = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(data.ModelledFoodConsumers);
            data.ReferenceCompound = data.ActiveSubstances.First();
            data.CorrectedRelativePotencyFactors = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.MembershipProbabilities = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.CumulativeCompound = data.ActiveSubstances.First();
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(simulatedIndividualDays, foods, data.ActiveSubstances, 0.5, true, random);
            data.DietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(data.ConsumptionUnit, data.ConcentrationUnit, data.BodyWeightUnit, false);
            data.DietaryExposureUnit.Compartment = "bw";
            var usualIntakes = exposures.Select(c => new DietaryIndividualIntake() { 
                DietaryIntakePerMassUnit= c.TotalExposurePerMassUnit(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, false),
                Individual = c.Individual,
                IndividualSamplingWeight = c.IndividualSamplingWeight,
                NumberOfDays = 2,
                SimulatedIndividualId= c.SimulatedIndividualId,
            }).ToList();

            var result = new DietaryExposuresActionResult() {
                DietaryIndividualDayIntakes = exposures,
                DietaryModelAssistedIntakes = usualIntakes,
            };
            var outputRawDataWriter = new CsvRawDataWriter(Path.Combine(_reportOutputPath, "TestChronic1"));
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(project, data, result, outputRawDataWriter);
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
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.AssessmentSettings.Cumulative = true;
            var data = new ActionData();
            var foods = MockFoodsGenerator.Create(8);
            data.ActiveSubstances = MockSubstancesGenerator.Create(5);
            data.ModelledFoodConsumers = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(data.ModelledFoodConsumers);
            data.ReferenceCompound = data.ActiveSubstances.First();
            data.CorrectedRelativePotencyFactors = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.MembershipProbabilities = data.ActiveSubstances.ToDictionary(c => c, c => 1d);
            data.CumulativeCompound = data.ActiveSubstances.First();
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(simulatedIndividualDays, foods, data.ActiveSubstances, 0.5, true, random);
            data.DietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(data.ConsumptionUnit, data.ConcentrationUnit, data.BodyWeightUnit, false);
            data.DietaryExposureUnit.Compartment = "bw";
            var usualIntakes = exposures.Select(c => new DietaryIndividualIntake() {
                DietaryIntakePerMassUnit = c.TotalExposurePerMassUnit(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, false),
                Individual = c.Individual,
                IndividualSamplingWeight = c.IndividualSamplingWeight,
                NumberOfDays = 2,
                SimulatedIndividualId = c.SimulatedIndividualId,
            }).ToList();

            var result = new DietaryExposuresActionResult() {
                DietaryIndividualDayIntakes = exposures,
                DietaryObservedIndividualMeans = usualIntakes,
            };
            var outputRawDataWriter = new CsvRawDataWriter(Path.Combine(_reportOutputPath, "TestChronic2"));
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(project, data, result, outputRawDataWriter);
            outputRawDataWriter.Store();
        }
    }
}