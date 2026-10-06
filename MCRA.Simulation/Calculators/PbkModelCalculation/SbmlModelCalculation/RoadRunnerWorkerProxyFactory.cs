using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.InProcess;
using MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.OutOfProcess;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;

/// <summary>Creates RoadRunner proxies for the simulation web host.</summary>
public sealed class RoadRunnerWorkerProxyFactory : IRoadRunnerWorkerProxyFactory {
    private readonly int _workerCount;
    public int WorkerCount => _workerCount;

    public RoadRunnerWorkerProxyFactory(int? workerCount = null) {
        _workerCount = workerCount ?? 1;
    }

    public IRoadRunnerWorkerProxy Create(
        RoadRunnerWorkerProxyType proxyType,
        ISbmlPbkModelSpecification specification,
        KineticModelInstance modelInstance,
        Dictionary<ExposureRoute, string> routeMappings
    ) => proxyType switch {

        RoadRunnerWorkerProxyType.InProcess => new InProcessRoadRunnerWorkerProxy(specification, modelInstance, routeMappings),

        // Lease a single worker from the shared, process-wide worker pool. The pool is a singleton
        // because several users of this web application may run SBML simulations concurrently; its
        // worker processes are shared and load-balanced across all of their requests rather than
        // spawning a fresh set of worker processes per simulation.
        RoadRunnerWorkerProxyType.OutOfProcess => SbmlWorkerPool.Instance.LeaseWorker(
            modelInstance.KineticModelDefinition.FileName,
            routeMappings,
            specification,
            _workerCount),
        _ => throw new ArgumentOutOfRangeException(nameof(proxyType))
    };
}
