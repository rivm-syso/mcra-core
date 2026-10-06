using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.OutOfProcess;

/// <summary>
/// Manages a process-wide, shared pool of persistent RoadRunner worker processes. Each process owns
/// one Python interpreter and (at any given time) one loaded RoadRunner model.
/// <para>
/// This pool is a singleton: because MCRA is a web application, several users may start an SBML PBK
/// simulation at (roughly) the same time. Rather than spawning a fresh set of worker processes for
/// every simulation request, all requests share and are load-balanced across the same pool of worker
/// processes. The pool grows (but never shrinks) to accommodate the largest number of workers
/// requested so far by any caller.
/// </para>
/// <para>
/// Because workers are shared across requests that may target different kinetic models, each worker
/// is (re)initialized with the requested model just before it is used, if it is not already loaded.
/// </para>
/// </summary>
internal sealed class SbmlWorkerPool : IDisposable {
    private static readonly Lazy<SbmlWorkerPool> _instance = new(() => new SbmlWorkerPool());

    /// <summary>The single, process-wide instance of the worker pool.</summary>
    public static SbmlWorkerPool Instance => _instance.Value;

    private readonly object _growLock = new();
    private readonly List<WorkerClient> _workers = [];
    private int _nextWorker = -1;
    private bool _disposed;

    private SbmlWorkerPool() {
    }

    /// <summary>
    /// Leases one worker from the pool (growing the pool if needed, up to <paramref name="desiredWorkerCount"/>
    /// workers) and returns a proxy bound to that single worker. The worker is (re)initialized with the
    /// given model the first time the returned proxy is used.
    /// </summary>
    public IRoadRunnerWorkerProxy LeaseWorker(
        string modelFileName,
        Dictionary<ExposureRoute, string> routeMappings,
        ISbmlPbkModelSpecification specification,
        int desiredWorkerCount
    ) {
        WorkerClient worker;
        lock (_growLock) {
            var count = Math.Max(1, desiredWorkerCount);
            ensureCapacity(count);
            var size = _workers.Count;
            var index = (int)((uint)Interlocked.Increment(ref _nextWorker) % (uint)size);
            worker = _workers[index];
        }

        var fullModelFileName = Path.GetFullPath(modelFileName);
        var init = new WorkerInitializationDto {
            Type = "initialize",
            ModelFileName = fullModelFileName,
            ModelFileContent = File.ReadAllText(fullModelFileName),
            RouteInputMappings = routeMappings.ToDictionary(r => r.Key.ToString(), r => r.Value),
            BodyWeightParameterId = specification.GetParameterDefinitionByType(PbkModelParameterType.BodyWeight)?.Id,
            PbkModelSpecification = SbmlPbkModelSpecificationDto.FromSpecification(specification)
        };
        return new PooledWorkerProxy(worker, init, createReplacementWorker);
    }

    /// <summary>Grows the pool (never shrinks it) so it has at least <paramref name="count"/> workers.</summary>
    private void ensureCapacity(int count) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workers.RemoveAll(worker => !worker.IsAlive);
        while (_workers.Count < count) {
            var worker = new WorkerClient();
            worker.Exited += workerExited;
            _workers.Add(worker);
            if (!worker.IsAlive) {
                _workers.Remove(worker);
                throw new InvalidOperationException("A RoadRunner worker exited while the worker pool was being created.");
            }
        }
    }

    private void workerExited(WorkerClient worker) {
        lock (_growLock) {
            _workers.Remove(worker);
        }
    }

    private WorkerClient createReplacementWorker() {
        lock (_growLock) {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var worker = new WorkerClient();
            worker.Exited += workerExited;
            if (!worker.IsAlive) {
                worker.Dispose();
                throw new InvalidOperationException("A replacement RoadRunner worker exited during startup.");
            }
            _workers.Add(worker);
            return worker;
        }
    }

    public void Dispose() {
        List<WorkerClient> workers;
        lock (_growLock) {
            if (_disposed) return;
            _disposed = true;
            workers = [.. _workers];
            _workers.Clear();
        }
        foreach (var worker in workers) worker.Dispose();
    }
}
