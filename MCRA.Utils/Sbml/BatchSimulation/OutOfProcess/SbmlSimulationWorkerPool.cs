using MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto;

namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess;

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
internal sealed class SbmlSimulationWorkerPool : IDisposable {

    private static readonly Lock _instanceLock = new();
    private static SbmlSimulationWorkerPool _instance;

    public static SbmlSimulationWorkerPool GetInstance(
        bool showConsoleWindow,
        bool extensiveConsoleLogging
    ) {
        lock (_instanceLock) {
            if (_instance is null) {
                _instance = new SbmlSimulationWorkerPool(showConsoleWindow, extensiveConsoleLogging);
            } else if (_instance._showConsoleWindow != showConsoleWindow
                || _instance._extensiveConsoleLogging != extensiveConsoleLogging) {
                throw new InvalidOperationException(
                    "SbmlSimulationWorkerPool is already initialized with different console settings.");
            }
            return _instance;
        }
    }

    private readonly bool _showConsoleWindow = false;
    private readonly bool _extensiveConsoleLogging = false;

    private readonly object _growLock = new();
    private readonly List<SbmlRoadRunnerWorkerClient> _workers = [];
    private readonly HashSet<SbmlRoadRunnerWorkerClient> _leasedWorkers = [];
    private int _nextWorker = -1;
    private bool _disposed;

    private SbmlSimulationWorkerPool(
        bool showConsoleWindow,
        bool extensiveConsoleLogging
    ) {
        _showConsoleWindow = showConsoleWindow;
        _extensiveConsoleLogging = extensiveConsoleLogging;
    }

    /// <summary>
    /// Leases one worker from the pool (growing the pool if needed, up to
    /// <paramref name="desiredWorkerCount"/> workers) and returns a proxy bound 
    /// to that single worker. The worker is (re)initialized with the given model
    /// the first time the returned proxy is used.
    /// </summary>
    public IRoadRunnerWorkerProxy LeaseWorker(
        string filename,
        int desiredWorkerCount
    ) {
        SbmlRoadRunnerWorkerClient worker;
        lock (_growLock) {
            var count = Math.Max(1, desiredWorkerCount);
            ensureCapacity(count);

            // Lease an available worker exclusively for the lifetime of the returned proxy.
            worker = getNextAvailableWorker() ?? createWorker();
            _leasedWorkers.Add(worker);
        }

        var fullModelFileName = Path.GetFullPath(filename);
        var init = new SbmlSimulationInitializationDto {
            Type = "initialize",
            ModelFileName = fullModelFileName,
            ModelFileContent = File.ReadAllText(fullModelFileName)
        };
        return new PooledSbmlSimulationWorkerProxy(
            worker,
            init,
            createReplacementWorker,
            releaseWorker
        );
    }

    /// <summary>
    /// Grows the pool (never shrinks it) so it has at least <paramref name="count"/> workers.
    /// </summary>
    private void ensureCapacity(int count) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workers.RemoveAll(worker => !worker.IsAlive);
        while (_workers.Count < count) {
            createWorker();
        }
    }

    private SbmlRoadRunnerWorkerClient getNextAvailableWorker() {
        var size = _workers.Count;
        if (size == 0) {
            return null;
        }

        for (var i = 0; i < size; i++) {
            var index = (int)((uint)Interlocked.Increment(ref _nextWorker) % (uint)size);
            var candidate = _workers[index];
            if (!_leasedWorkers.Contains(candidate)) {
                return candidate;
            }
        }

        return null;
    }

    private SbmlRoadRunnerWorkerClient createWorker() {
        var worker = new SbmlRoadRunnerWorkerClient(
            _showConsoleWindow,
            _extensiveConsoleLogging
        );
        worker.Exited += workerExited;
        _workers.Add(worker);
        if (!worker.IsAlive) {
            _workers.Remove(worker);
            throw new InvalidOperationException("A RoadRunner worker exited while the worker pool was being created.");
        }
        return worker;
    }

    private void workerExited(SbmlRoadRunnerWorkerClient worker) {
        lock (_growLock) {
            _workers.Remove(worker);
            _leasedWorkers.Remove(worker);
        }
    }

    private SbmlRoadRunnerWorkerClient createReplacementWorker(SbmlRoadRunnerWorkerClient failedWorker) {
        lock (_growLock) {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _workers.Remove(failedWorker);
            _leasedWorkers.Remove(failedWorker);

            var worker = createWorker();
            _leasedWorkers.Add(worker);
            return worker;
        }
    }

    private void releaseWorker(SbmlRoadRunnerWorkerClient worker) {
        lock (_growLock) {
            _leasedWorkers.Remove(worker);
        }
    }

    public void Dispose() {
        List<SbmlRoadRunnerWorkerClient> workers;
        lock (_growLock) {
            if (_disposed) return;
            _disposed = true;
            workers = [.. _workers];
            _workers.Clear();
        }
        foreach (var worker in workers) {
            worker.Dispose();
        }
    }
}
