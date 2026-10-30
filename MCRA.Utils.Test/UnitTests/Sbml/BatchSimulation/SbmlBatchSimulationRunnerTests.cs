using MCRA.Utils.Sbml.BatchSimulation;
using MCRA.Utils.Sbml.Simulation;
using MCRA.Utils.Statistics;

namespace MCRA.Utils.Test.UnitTests.Sbml.BatchSimulation {

    [TestClass]
    public class SbmlBatchSimulationRunnerTests {

        [TestMethod]
        public void SbmlBatchSimulation_TestCompute() {
            var filename = $"Resources/Sbml/simple.sbml";

            // Arrange
            var random = new McraRandomGenerator(seed: 1);
            var events = new List<SbmlSimulationEvent>() {
                new() {
                    Id = "1",
                    Kind = SbmlSimulationEventType.Single,
                    Time = 0,
                    Value = 1,
                    Target = "AGut"
                }
            };
            var outputStatesSelection = new List<string>() { "BW" };
            var outputTimeSeriesSelection = new List<string>() { "AGut", "ABlood" };
            var numJobs = 3;
            var jobs = new List<SbmlSimulationJob>();
            for (var i = 0; i < numJobs; i++) {
                var bw = random.NextDouble(60, 90);
                var parameters = new Dictionary<string, double>() {
                    { "BW", bw }
                };
                var job = new SbmlSimulationJob() {
                    Parameters = parameters,
                    OutputStates = outputStatesSelection,
                    Duration = 120,
                    Events = events,
                    JobId = $"Sim_{i}",
                    OutputTimeSeries = outputTimeSeriesSelection,
                    Steps = 121
                };
                jobs.Add(job);
            }

            // Act
            var runner = new SbmlBatchSimulationRunner();
            var results = runner.Compute(
                filename,
                jobs,
                RoadRunnerWorkerProxyType.InProcess
            );

            // Assert
            Assert.HasCount(jobs.Count, results);
            var resultsDict = results.ToDictionary(r => r.JobId);
            foreach (var job in jobs) {
                var result = resultsDict[job.JobId];
                Assert.AreEqual(job.Parameters["BW"], result.Result.OutputStates["BW"]);
            }
        }
    }
}
