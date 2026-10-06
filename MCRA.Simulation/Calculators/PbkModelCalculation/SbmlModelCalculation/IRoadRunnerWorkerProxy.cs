namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;

/// <summary>
/// Executes a batch of SBML simulations through a RoadRunner worker implementation.
/// Implementations may run RoadRunner in-process or in a separate process. A batch is always
/// submitted and executed as a single call (e.g. a single HTTP round-trip for an out-of-process
/// worker), reusing the loaded model across all jobs in the batch, rather than issuing one call
/// per job.
/// </summary>
public interface IRoadRunnerWorkerProxy : IDisposable {
    /// <summary>
    /// Runs all given jobs against this proxy's (single) worker/model instance and returns their
    /// results in the same order as <paramref name="jobs"/>.
    /// </summary>
    Task<List<RoadRunnerSimulationResult>> RunAsync(List<RoadRunnerSimulationJob> jobs);
}
