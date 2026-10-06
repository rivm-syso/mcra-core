namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.OutOfProcess;

/// <summary>Lifecycle hooks for persistent external RoadRunner workers.</summary>
public static class RoadRunnerWorkerPoolLifecycle {
    /// <summary>Stops all external RoadRunner worker processes owned by this application.</summary>
    public static void Shutdown() => SbmlWorkerPool.Instance.Dispose();
}
