namespace MCRA.Utils.Sbml.BatchSimulation {
    public class SbmlBatchSimulationRunner {

        public List<SbmlSimulationJobResult> Compute(
            string filename,
            List<SbmlSimulationJob> jobs,
            RoadRunnerWorkerProxyType proxyType,
            int maxParallelWorkers = 10,
            bool showConsoleWindow = false,
            bool extensiveConsoleLogging = false
        ) {
            var workerCount = Math.Min(jobs.Count, maxParallelWorkers);
            var proxyFactory = new RoadRunnerWorkerProxyFactory(
                workerCount: workerCount,
                showConsoleWindow: showConsoleWindow,
                extensiveConsoleLogging: extensiveConsoleLogging
            );

            // Create at most one worker proxy per unit of work, but never more than the number of
            // workers recommended by the factory, so the total workload (e.g. all individuals) is
            // divided over the configured number of workers/processes.
            var proxies = new List<IRoadRunnerWorkerProxy>(workerCount);
            try {
                for (var w = 0; w < workerCount; w++) {
                    proxies.Add(proxyFactory
                        .Create(proxyType, filename)
                    );
                }

                // Partition the work round-robin across the proxies: each proxy processes its own
                // slice of individuals sequentially, while the slices run concurrently with each other.
                var outputs = new SbmlSimulationJobResult[jobs.Count];
                var partitionTasks = new Task[proxies.Count];
                for (var w = 0; w < proxies.Count; w++) {
                    var proxy = proxies[w];
                    var startIndex = w;
                    partitionTasks[w] = runPartitionAsync(
                        proxy,
                        startIndex,
                        proxies.Count,
                        jobs,
                        outputs
                    );
                }
                Task.WhenAll(partitionTasks).GetAwaiter().GetResult();

                var result = outputs.ToList();
                return result;
            } catch (Exception ex) {
                var jobId = ex.Data["JobId"] ?? "unknown";
                throw new InvalidOperationException($"SBML model simulation failed for job [{jobId}].", ex);
            } finally {
                foreach (var proxy in proxies) {
                    proxy.Dispose();
                }
            }
        }

        private static async Task runPartitionAsync(
            IRoadRunnerWorkerProxy proxy,
            int startIndex,
            int stride,
            List<SbmlSimulationJob> allJobs,
            SbmlSimulationJobResult[] outputs
        ) {
            // Get jobs for partition
            var partitionJobs = new List<SbmlSimulationJob>();
            var indices = new List<int>();
            for (var i = startIndex; i < allJobs.Count; i += stride) {
                partitionJobs.Add(allJobs[i]);
                indices.Add(i);
            }

            // If no jobs were partitioned, then return
            if (partitionJobs.Count == 0) {
                return;
            }

            // Run partition jobs and collect results
            var results = await proxy.RunAsync(partitionJobs).ConfigureAwait(false);
            for (var j = 0; j < indices.Count; j++) {
                outputs[indices[j]] = results[j];
            }
        }
    }
}
