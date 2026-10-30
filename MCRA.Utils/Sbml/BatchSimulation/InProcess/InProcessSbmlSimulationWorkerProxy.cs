using MCRA.Utils.Sbml.Simulation;

namespace MCRA.Utils.Sbml.BatchSimulation.InProcess {

    /// <summary>
    /// Runs RoadRunner simulations in the current process using <see cref="SbmlModelRunner"/>.
    /// A whole batch of jobs is executed in a single call, reusing the loaded model across all
    /// jobs.
    /// </summary>
    public sealed class InProcessSbmlSimulationWorkerProxy : IRoadRunnerWorkerProxy, IDisposable {
        private readonly SbmlModelRunner _runner;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public InProcessSbmlSimulationWorkerProxy(string filename) {
            _runner = new SbmlModelRunner();
            _runner.LoadModelFromFile(filename);
        }

        public async Task<List<SbmlSimulationJobResult>> RunAsync(List<SbmlSimulationJob> jobs) {
            await _gate.WaitAsync().ConfigureAwait(false);
            try {
                var results = new List<SbmlSimulationJobResult>(jobs.Count);
                foreach (var job in jobs) {
                    var output = _runner.Run(
                        job.Events,
                        job.Parameters,
                        job.OutputStates,
                        job.OutputTimeSeries,
                        job.Duration,
                        job.Steps
                    );
                    results.Add(
                        new SbmlSimulationJobResult() {
                            JobId = job.JobId,
                            Result = output
                        }
                    );
                }
                return results;
            } finally {
                _gate.Release();
            }
        }

        public void Dispose() {
            _gate.Dispose();
        }
    }
}
