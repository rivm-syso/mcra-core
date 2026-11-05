using MCRA.Utils.Sbml.BatchSimulation.OutOfProcess.Dto;
using MCRA.Utils.Sbml.Simulation;

namespace MCRA.Utils.Sbml.BatchSimulation.OutOfProcess {
    /// <summary>
    /// A thin, per-lease <see cref="IRoadRunnerWorkerProxy"/> bound to a single shared <see cref="SbmlRoadRunnerWorkerClient"/>.
    /// Disposing a lease does not stop or dispose the underlying worker process, since it is owned and
    /// reused by the pool, potentially across many concurrent leases from different users/requests.
    /// </summary>
    internal sealed class PooledSbmlSimulationWorkerProxy : IRoadRunnerWorkerProxy {

        private SbmlRoadRunnerWorkerClient _client;
        private readonly Func<SbmlRoadRunnerWorkerClient, SbmlRoadRunnerWorkerClient> _restartWorker;
        private readonly Action<SbmlRoadRunnerWorkerClient> _releaseWorker;
        private readonly SbmlSimulationInitializationDto _init;
        private readonly int _restartAttempts;
        private bool _disposed;

        public PooledSbmlSimulationWorkerProxy(
            SbmlRoadRunnerWorkerClient client,
            SbmlSimulationInitializationDto init,
            Func<SbmlRoadRunnerWorkerClient, SbmlRoadRunnerWorkerClient> restartWorker,
            Action<SbmlRoadRunnerWorkerClient> releaseWorker,
            int restartAttempts
        ) {
            _client = client;
            _init = init;
            _restartWorker = restartWorker;
            _releaseWorker = releaseWorker;
            _restartAttempts = restartAttempts;
        }

        public async Task<List<SbmlSimulationJobResult>> RunAsync(List<SbmlSimulationJob> jobs) {
            var batch = createBatchJobDto(jobs);
            var jobResults = await runWithRestartsAsync(batch).ConfigureAwait(false);
            var resultsByJobId = jobResults.ToDictionary(r => r.JobId);

            var results = new List<SbmlSimulationJobResult>(jobs.Count);
            foreach (var job in jobs) {
                if (!resultsByJobId.TryGetValue(job.JobId, out var jobResult)) {
                    throw new InvalidDataException($"RoadRunner worker response did not include a result for job '{job.JobId}'.");
                }
                if (jobResult.Error != null) {
                    var exception = new InvalidOperationException($"RoadRunner worker failed job {job.JobId}: {jobResult.Error}");
                    replaceFailedWorker();
                    throw exception;
                }
                results.Add(new SbmlSimulationJobResult {
                    JobId = job.JobId,
                    Result = new SbmlSimulationResult {
                        OutputStates = jobResult.Result.OutputStates,
                        OutputTimeSeries = jobResult.Result.OutputTimeSeries,
                        Time = jobResult.Result.Time
                    },
                    Error = jobResult.Error
                });
            }
            return results;
        }

        private async Task<List<WorkerJobResultDto>> runWithRestartsAsync(SbmlSimulationBatchJobDto batch) {
            for (var restart = 0; ; restart++) {
                try {
                    return await _client.RunAsync(batch, _init).ConfigureAwait(false);
                } catch when (!_client.IsAlive) {
                    // The worker process has died because of a severe crash, which was not caught as a
                    // regular .NET exception or the process has just been killed. There is no exception information.
                    if (restart >= _restartAttempts) {
                        throw;
                    }
                    Console.WriteLine($"RoadRunner worker with processId {_client.ProcessId} has died for batch of {batch.Jobs.Count} job(s): \nRestart roadrunner worker (attempt {restart + 1} of {_restartAttempts})");
                    var failedClient = _client;
                    _client = _restartWorker(_client);
                    failedClient.Dispose();
                } catch (Exception ex) {
                    // A worker-side exception may be returned as an HTTP 500 while the worker host
                    // process itself remains alive. Replace it either way and retry the batch, up to
                    // the configured number of restart attempts.
                    if (restart >= _restartAttempts) {
                        throw;
                    }
                    Console.WriteLine($"RoadRunner worker failed batch of {batch.Jobs.Count} job(s): {ex.Message}\nRestart roadrunner worker (attempt {restart + 1} of {_restartAttempts})");
                    var failedClient = _client;
                    _client = _restartWorker(_client);
                    failedClient.Dispose();
                }
            }
        }

        private static SbmlSimulationBatchJobDto createBatchJobDto(
            List<SbmlSimulationJob> jobs
        ) {
            var batch = new SbmlSimulationBatchJobDto {
                Type = "simulate",
                Jobs = jobs
                    .Select(job => new SbmlSimulationJobDto {
                        JobId = job.JobId,
                        Parameters = job.Parameters,
                        Events = job.Events
                            .Select(e => new SbmlSimulationEventDto {
                                Id = $"{e.Kind}_{e.Target}_{e.Time}",
                                Kind = e.Kind,
                                Target = e.Target,
                                Time = e.Time,
                                Interval = e.Interval,
                                TimeEnd = e.TimeEnd,
                                Value = e.Value
                            })
                            .ToList(),
                        OutputStates = job.OutputStates,
                        OutputTimeSeries = job.OutputTimeSeries,
                        Duration = job.Duration,
                        Steps = job.Steps
                    })
                    .ToList()
            };
            return batch;
        }

        private void replaceFailedWorker() {
            var failedClient = _client;
            _client = _restartWorker(_client);
            failedClient.Dispose();
        }

        // The underlying worker process is owned by the pool, not by this lease, so there is nothing
        // to dispose here; the worker remains available for subsequent leases.
        public void Dispose() {
            if (_disposed) {
                return;
            }
            _disposed = true;
            _releaseWorker(_client);
        }
    }
}
