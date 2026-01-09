using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.OccupationalExposures;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    [TestClass]
    public class OccupationalExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OccupationalExposuresActionCalculator_TestCompute(
            bool computeExternalDoses
        ) {
            var random = new McraRandomGenerator(1);
            var substances = FakeSubstancesGenerator.Create(2);
            var routes = new ExposureRoute[] { ExposureRoute.Inhalation, ExposureRoute.Dermal };
            var scenarios = FakeOccupationalExposuresGenerator.CreateScenarios([1, 2, 2], random);
            var scenarioTasks = scenarios
                .SelectMany(s => s.Tasks, (s, t) => (t.OccupationalTask, t.Determinants()))
                .ToList();
            var exposures = FakeOccupationalExposuresGenerator
                .CreateExposures(scenarioTasks, routes, substances, [50, 90, 95], random);
            var project = new ProjectDto();
            var config = project.OccupationalExposuresSettings;
            config.SelectedExposureRoutes = [.. routes];
            config.SelectedPercentage = 90D;
            config.ComputeExternalOccupationalDoses = computeExternalDoses;

            var data = new ActionData() {
                ActiveSubstances = substances,
                OccupationalScenarios = scenarios.ToDictionary(r => r.Code),
                OccupationalTaskExposures = exposures
            };

            var runId = "TestCompute_" + (computeExternalDoses ? "SYS" : "EXP");
            var calculator = new OccupationalExposuresActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, runId);
            var result = calculator.Run(data, new CompositeProgressState()) as OccupationalExposuresActionResult;

            var expectedCount = scenarios.Count * substances.Count * routes.Length;
            Assert.HasCount(expectedCount, result.OccupationalScenarioExposures);
        }

        [TestMethod]
        public void OccupationalExposuresActionCalculator_TestComputeIndividualSet() {
            var random = new McraRandomGenerator(1);
            var substances = FakeSubstancesGenerator.Create(2);
            var routes = new ExposureRoute[] { ExposureRoute.Inhalation, ExposureRoute.Dermal };
            var scenarios = FakeOccupationalExposuresGenerator.CreateScenarios([1, 2, 2], random);
            var scenarioTasks = scenarios
                .SelectMany(s => s.Tasks, (s, t) => (t.OccupationalTask, t.Determinants()))
                .ToList();
            var exposures = FakeOccupationalExposuresGenerator
                .CreateExposures(scenarioTasks, routes, substances, [50, 90, 95], random);

            var individuals = FakeIndividualsGenerator.Create(
                200,
                2,
                randomSamplingWeight: random,
                useSamplingWeights: true,
                randomBodyWeight: random
            );
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            var project = new ProjectDto();
            var config = project.OccupationalExposuresSettings;
            config.ExposureType = ExposureType.Chronic;
            config.SelectedExposureRoutes = [.. routes];
            config.SelectedPercentage = 90D;
            config.ComputeExternalOccupationalDoses = true;
            config.OccupationalExposuresCalculationMethod = OccupationalExposuresCalculationMethod.IndividualSet;

            var data = new ActionData() {
                ActiveSubstances = substances,
                OccupationalScenarios = scenarios.ToDictionary(r => r.Code),
                OccupationalTaskExposures = exposures,
                Individuals = [.. individualDays.Cast<IIndividualDay>()]
            };

            var runId = "TestComputeIndividualSet";
            var calculator = new OccupationalExposuresActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, runId);
            var result = calculator.Run(data, new CompositeProgressState()) as OccupationalExposuresActionResult;

            var expectedCount = scenarios.Count * substances.Count * routes.Length;
            Assert.HasCount(expectedCount, result.OccupationalScenarioExposures);
            Assert.HasCount(individualDays.Count, result.ExternalIndividualExposureCollection.ExternalIndividualDayExposures);
        }
    }
}
