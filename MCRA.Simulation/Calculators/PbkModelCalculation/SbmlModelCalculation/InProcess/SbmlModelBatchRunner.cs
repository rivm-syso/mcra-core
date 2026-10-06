using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Simulation.Calculators.PbkModelCalculation.ExposureEventsGeneration;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.InProcess;

/// <summary>
/// Wraps a single <see cref="SbmlModelRunner"/> (i.e. a single loaded RoadRunner model instance)
/// and runs a batch of <see cref="RoadRunnerSimulationJob"/>s against it sequentially, reusing the
/// loaded model across all jobs in the batch.
/// <para>
/// This is the in-process analogue of the batching performed by the out-of-process RoadRunner
/// worker (<c>MCRA.Simulation.SbmlRoadRunner.Worker.RoadRunnerWorker.RunBatch</c>): both loop over
/// a collection of per-individual simulation jobs against one already-loaded model, rather than
/// recreating/reinitializing a model (or, for the out-of-process worker, issuing a separate HTTP
/// request) per job.
/// </para>
/// </summary>
public sealed class SbmlModelBatchRunner : IDisposable {
    private readonly SbmlModelRunner _runner;
    private readonly Dictionary<ExposureRoute, string> _routeInputMappings;

    public SbmlModelBatchRunner(
        ISbmlPbkModelSpecification specification,
        KineticModelInstance modelInstance,
        Dictionary<ExposureRoute, string> routeInputMappings
    ) {
        _runner = new SbmlModelRunner(specification, modelInstance);
        _routeInputMappings = routeInputMappings;
    }

    /// <summary>
    /// Runs all given jobs against the wrapped model and returns their results in the same order
    /// as <paramref name="jobs"/>.
    /// </summary>
    public List<RoadRunnerSimulationResult> RunBatch(List<RoadRunnerSimulationJob> jobs) {
        var results = new List<RoadRunnerSimulationResult>(jobs.Count);
        foreach (var job in jobs) {
            var events = job.Events.Select(toExposureEvent).ToList();
            var output = _runner.Run(
                events,
                job.Parameters,
                _routeInputMappings,
                job.OutputStates,
                job.OutputTimeSeries,
                job.Duration,
                job.Steps,
                job.ApplyBodyWeightScaling);
            results.Add(new RoadRunnerSimulationResult {
                Time = output.Time,
                OutputTimeSeries = output.OutputTimeSeries,
                OutputStates = output.OutputStates
            });
        }
        return results;
    }

    private static IExposureEvent toExposureEvent(RoadRunnerExposureEvent exposureEvent) {
        var route = Enum.Parse<ExposureRoute>(exposureEvent.Route);
        return exposureEvent.Kind switch {
            "single" => new SingleExposureEvent {
                Route = route,
                Time = exposureEvent.Time,
                Value = exposureEvent.Value
            },
            "repeating" => new RepeatingExposureEvent {
                Route = route,
                Time = exposureEvent.Time,
                Interval = exposureEvent.Interval,
                TimeEnd = exposureEvent.TimeEnd,
                Value = exposureEvent.Value
            },
            _ => throw new NotSupportedException($"Unsupported RoadRunner exposure event kind '{exposureEvent.Kind}'.")
        };
    }

    public void Dispose() => _runner.Dispose();
}
