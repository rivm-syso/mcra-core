using MCRA.Utils.Sbml.Simulation;

namespace MCRA.Utils.Test.UnitTests.Sbml.Simulation {

    [TestClass]
    public class SbmlModelRunnerTests {

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SbmlModelRunner_TestRun_SetParameter(bool reuse) {
            var filename = $"Resources/Sbml/simple.sbml";
            var runner = new SbmlModelRunner();
            SbmlModelRunner.ReuseEvents = reuse;
            runner.LoadModelFromFile(filename);

            var events = new List<SbmlSimulationEvent>() {
                new() {
                    Id = "1",
                    Kind = SbmlSimulationEventType.Single,
                    Time = 0,
                    Value = 1,
                    Target = "AGut"
                }
            };
            var parameters = new Dictionary<string, double>() {
                { "BW", 77 },
            };
            var outputStatesSelection = new List<string>() {
                "BW"
            };
            var outputTimeSeriesSelection = new List<string>();

            var result = runner.Run(
                events: events,
                parameters: parameters,
                outputStatesSelection: outputStatesSelection,
                outputTimeSeriesSelection: outputTimeSeriesSelection,
                evaluationPeriod: 120,
                steps: 121
            );
            Assert.IsTrue(result.OutputStates.ContainsKey("BW"));
            Assert.AreEqual(77, result.OutputStates["BW"]);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SbmlModelRunner_TestRun_SingleEvent(bool reuse) {
            var filename = $"Resources/Sbml/simple.sbml";
            var runner = new SbmlModelRunner();
            SbmlModelRunner.ReuseEvents = reuse;
            runner.LoadModelFromFile(filename);

            var events = new List<SbmlSimulationEvent>() {
                new() {
                    Id = "1",
                    Kind = SbmlSimulationEventType.Single,
                    Time = 0,
                    Value = 1,
                    Target = "AGut"
                }
            };
            var parameters = new Dictionary<string, double>();
            var outputStatesSelection = new List<string>();
            var outputTimeSeriesSelection = new List<string>() {
                "AGut",
                "ABlood"
            };

            for (int i = 0; i < 2; i++) {
                var result = runner.Run(
                    events: events,
                    parameters: parameters,
                    outputStatesSelection: outputStatesSelection,
                    outputTimeSeriesSelection: outputTimeSeriesSelection,
                    evaluationPeriod: 120,
                    steps: 121
                );

                Assert.HasCount(2, result.OutputTimeSeries);
                Assert.HasCount(121, result.OutputTimeSeries.Values.First());
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SbmlModelRunner_TestRun_RepeatedEvents(bool reuse) {
            var filename = $"Resources/Sbml/simple.sbml";
            var runner = new SbmlModelRunner();
            SbmlModelRunner.ReuseEvents = reuse;
            runner.LoadModelFromFile(filename);

            var events = new List<SbmlSimulationEvent>() {
                new() {
                    Id = "1",
                    Kind = SbmlSimulationEventType.Repeated,
                    Time = 0,
                    Interval = 24,
                    Value = 1,
                    Target = "AGut"
                }
            };
            var parameters = new Dictionary<string, double>();
            var outputStatesSelection = new List<string>();
            var outputTimeSeriesSelection = new List<string>() {
                "AGut",
                "ABlood"
            };

            for (int i = 0; i < 2; i++) {
                var result = runner.Run(
                    events: events,
                    parameters: parameters,
                    outputStatesSelection: outputStatesSelection,
                    outputTimeSeriesSelection: outputTimeSeriesSelection,
                    evaluationPeriod: 120,
                    steps: 121
                );

                Assert.HasCount(2, result.OutputTimeSeries);
                Assert.HasCount(121, result.OutputTimeSeries.Values.First());
            }
        }
    }
}
