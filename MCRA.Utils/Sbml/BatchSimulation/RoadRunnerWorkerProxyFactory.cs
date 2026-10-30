using MCRA.Utils.Sbml.BatchSimulation.InProcess;
using MCRA.Utils.Sbml.BatchSimulation.OutOfProcess;

namespace MCRA.Utils.Sbml.BatchSimulation;

public enum RoadRunnerWorkerProxyType {
    InProcess,
    OutOfProcess
}

/// <summary>Creates RoadRunner proxies for the simulation web host.</summary>
public sealed class RoadRunnerWorkerProxyFactory {
    private readonly int _workerCount;
    private readonly bool _showConsoleWindow;
    private readonly bool _extensiveConsoleLogging;
    public int WorkerCount => _workerCount;

    public RoadRunnerWorkerProxyFactory(
        int? workerCount = null,
        bool showConsoleWindow = false,
        bool extensiveConsoleLogging = false
    ) {
        _workerCount = workerCount ?? 1;
        _showConsoleWindow = showConsoleWindow;
        _extensiveConsoleLogging = extensiveConsoleLogging;
    }

    public IRoadRunnerWorkerProxy Create(
        RoadRunnerWorkerProxyType proxyType,
        string filename
    ) => proxyType switch {

        RoadRunnerWorkerProxyType.InProcess => new InProcessSbmlSimulationWorkerProxy(filename),

        // Lease a single worker from the shared, process-wide worker pool. The pool is a singleton
        // because several users of this web application may run SBML simulations concurrently; its
        // worker processes are shared and load-balanced across all of their requests rather than
        // spawning a fresh set of worker processes per simulation.
        RoadRunnerWorkerProxyType.OutOfProcess => SbmlSimulationWorkerPool
            .GetInstance(_showConsoleWindow, _extensiveConsoleLogging)
            .LeaseWorker(filename, _workerCount),

        _ => throw new ArgumentOutOfRangeException(nameof(proxyType))
    };
}
