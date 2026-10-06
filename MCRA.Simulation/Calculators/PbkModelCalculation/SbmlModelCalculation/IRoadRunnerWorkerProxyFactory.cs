namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;

public interface IRoadRunnerWorkerProxyFactory {
    /// <summary>
    /// The number of proxies/workers this factory recommends creating so the caller can
    /// divide its total workload (e.g. across individuals) over that many workers.
    /// </summary>
    int WorkerCount { get; }

    IRoadRunnerWorkerProxy Create(
        RoadRunnerWorkerProxyType proxyType,
        General.PbkModelDefinitions.PbkModelSpecifications.Sbml.ISbmlPbkModelSpecification specification,
        Data.Compiled.Objects.KineticModelInstance modelInstance,
        Dictionary<General.ExposureRoute, string> routeMappings);
}
