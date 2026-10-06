using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.InProcess {

    /// <summary>
    /// Runs RoadRunner simulations in the current process using <see cref="SbmlModelBatchRunner"/>
    /// (which in turn wraps a single <see cref="SbmlModelRunner"/>). A whole batch of jobs (e.g. one
    /// per individual) is executed in a single call, reusing the loaded model across all jobs.
    /// </summary>
    public sealed class InProcessRoadRunnerWorkerProxy : IRoadRunnerWorkerProxy, IDisposable {
        private readonly SbmlModelBatchRunner _batchRunner;
        private readonly SemaphoreSlim _gate = new(1, 1);

        public InProcessRoadRunnerWorkerProxy(
            ISbmlPbkModelSpecification specification,
            KineticModelInstance modelInstance,
            Dictionary<ExposureRoute, string> routeMappings
        ) {
            _batchRunner = new SbmlModelBatchRunner(specification, modelInstance, routeMappings);
        }

        public async Task<List<RoadRunnerSimulationResult>> RunAsync(List<RoadRunnerSimulationJob> jobs) {
            await _gate.WaitAsync().ConfigureAwait(false);
            try {
                return _batchRunner.RunBatch(jobs);
            } finally {
                _gate.Release();
            }
        }

        public void Dispose() {
            _batchRunner.Dispose();
            _gate.Dispose();
        }
    }
}
