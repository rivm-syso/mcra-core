using MCRA.General.PbkModelDefinitions.SbmlRoadRunnerWorker;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation.OutOfProcess {
    /// <summary>
    /// A thin, per-lease <see cref="IRoadRunnerWorkerProxy"/> bound to a single shared <see cref="WorkerClient"/>.
    /// Disposing a lease does not stop or dispose the underlying worker process, since it is owned and
    /// reused by the pool, potentially across many concurrent leases from different users/requests.
    /// </summary>
    internal sealed class PooledWorkerProxy : IRoadRunnerWorkerProxy {
        private WorkerClient _client;
        private readonly Func<WorkerClient> _restartWorker;
        private readonly WorkerInitializationDto _init;

        public PooledWorkerProxy(WorkerClient client, WorkerInitializationDto init, Func<WorkerClient> restartWorker) {
            _client = client;
            _init = init;
            _restartWorker = restartWorker;
        }

        public async Task<List<RoadRunnerSimulationResult>> RunAsync(List<RoadRunnerSimulationJob> jobs) {
            var batch = new SimulationJobBatch {
                Type = "simulate",
                Jobs = jobs.Select(job => new SimulationJobDto {
                    JobId = job.JobId,
                    Parameters = job.Parameters,
                    Events = job.Events.Select(e => new ExposureEventDto {
                        Kind = e.Kind,
                        Route = e.Route,
                        Time = e.Time,
                        Interval = e.Interval,
                        TimeEnd = e.TimeEnd,
                        Value = e.Value
                    }).ToList(),
                    OutputStates = job.OutputStates,
                    OutputTimeSeries = job.OutputTimeSeries,
                    Duration = job.Duration,
                    Steps = job.Steps,
                    ApplyBodyWeightScaling = job.ApplyBodyWeightScaling
                }).ToList()
            };

            List<WorkerJobResultDto> jobResults;
            try {
                jobResults = await _client.RunAsync(batch, _init).ConfigureAwait(false);
            } catch when (!_client.IsAlive) {
                _client = _restartWorker();
                jobResults = await _client.RunAsync(batch, _init).ConfigureAwait(false);
            } catch (Exception ex) {
                // A worker-side exception may be returned as an HTTP 500 while the worker host
                // process itself remains alive. Replace it either way, but preserve this failure
                // for the caller rather than hiding it by retrying the partition.
                Console.WriteLine($"RoadRunner worker failed batch of {batch.Jobs.Count} job(s): {ex.Message}\nRestartint roadrunner worker one time");
                var failedClient = _client;
                _client = _restartWorker();
                failedClient.Dispose();
                jobResults = await _client.RunAsync(batch, _init).ConfigureAwait(false);
            }
            var resultsByJobId = jobResults.ToDictionary(r => r.JobId);

            var results = new List<RoadRunnerSimulationResult>(jobs.Count);
            foreach (var job in jobs) {
                if (!resultsByJobId.TryGetValue(job.JobId, out var jobResult)) {
                    throw new InvalidDataException($"RoadRunner worker response did not include a result for job '{job.JobId}'.");
                }
                if (jobResult.Error != null) {
                    var exception = new InvalidOperationException($"RoadRunner worker failed job {job.JobId}: {jobResult.Error}");
                    replaceFailedWorker();
                    throw exception;
                }
                results.Add(new RoadRunnerSimulationResult {
                    Time = jobResult.Result.Time,
                    OutputTimeSeries = jobResult.Result.OutputTimeSeries,
                    OutputStates = jobResult.Result.OutputStates
                });
            }
            return results;
        }

        private void replaceFailedWorker() {
            var failedClient = _client;
            _client = _restartWorker();
            failedClient.Dispose();
        }

        // The underlying worker process is owned by the pool, not by this lease, so there is nothing
        // to dispose here; the worker remains available for subsequent leases.
        public void Dispose() {
        }
    }
}
